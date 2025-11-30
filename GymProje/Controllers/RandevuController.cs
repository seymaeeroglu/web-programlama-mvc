using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymProje.Data;
using GymProje.Models;

namespace GymProje.Controllers
{
    // Sadece giriş yapmış (Üye veya Admin) kullanıcılar randevu alabilir
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Kullanici> _userManager;

        public RandevuController(ApplicationDbContext context, UserManager<Kullanici> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. RANDEVU GEÇMİŞİ
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .ThenInclude(h => h.Uzmanlik) // Hizmetin branşını da görelim
                .AsQueryable();

            if (user != null && !User.IsInRole("Admin"))
            {
                randevular = randevular.Where(r => r.KullaniciId == user.Id);
            }

            return View(await randevular.OrderByDescending(r => r.Tarih).ToListAsync());
        }

        // 2. RANDEVU ALMA SAYFASI (GET)
        [HttpGet]
        public IActionResult Create(int? hizmetId)
        {
            // Antrenör yoksa hata vermesin
            if (!_context.Antrenorler.Any())
            {
                ViewBag.Hata = "Sistemde kayıtlı antrenör bulunamadı.";
                ViewData["AntrenorId"] = new SelectList(new List<string>());
                ViewData["HizmetId"] = new SelectList(new List<string>());
                return View();
            }

            // Antrenörleri doldur
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad");

            // Hizmetleri özel formatla doldur (Ad sütunu silindiği için burası önemli)
            var hizmetListesi = _context.Hizmetler
                .Include(h => h.Uzmanlik)
                .Select(h => new
                {
                    Id = h.Id,
                    Metin = $"{(h.Uzmanlik != null ? h.Uzmanlik.Ad : "Genel")} - {h.SureDk}dk - {h.Ucret} TL"
                })
                .ToList();

            ViewData["HizmetId"] = new SelectList(hizmetListesi, "Id", "Metin", hizmetId);

            return View();
        }

        // 3. RANDEVU KAYDETME (POST) - İŞ MANTIĞI BURADA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            var user = await _userManager.GetUserAsync(User);
            randevu.KullaniciId = user.Id;

            // Validasyon temizliği
            ModelState.Remove("Kullanici");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");
            ModelState.Remove("KullaniciId");

            if (ModelState.IsValid)
            {
                // A. ÇALIŞMA SAATİ KONTROLÜ
                var antrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);

                // Gelen saat "14:00" -> Sadece 14'ü alıyoruz
                int randevuSaati = int.Parse(randevu.Saat.Split(':')[0]);

                if (randevuSaati < antrenor?.CalismaBaslangicSaati || randevuSaati >= antrenor?.CalismaBitisSaati)
                {
                    ModelState.AddModelError("", $"Seçtiğiniz eğitmen sadece {antrenor.CalismaBaslangicSaati}:00 - {antrenor.CalismaBitisSaati}:00 saatleri arasında çalışmaktadır.");
                    YenidenDoldur(randevu); // Hata durumunda kutuları tekrar doldur
                    return View(randevu);
                }

                // B. EĞİTMEN DOLU MU? (Çakışma Kontrolü)
                // İptal edilen randevular (Durum != "İptal Edildi") engel teşkil etmemeli.
                bool egitmenDoluMu = await _context.Randevular.AnyAsync(r =>
                    r.AntrenorId == randevu.AntrenorId &&
                    r.Tarih == randevu.Tarih &&
                    r.Saat == randevu.Saat &&
                    r.Durum != "İptal Edildi");

                if (egitmenDoluMu)
                {
                    ModelState.AddModelError("", "Üzgünüz, seçtiğiniz eğitmen bu tarih ve saatte dolu.");
                    YenidenDoldur(randevu);
                    return View(randevu);
                }

                // C. KULLANICI DOLU MU? (Senin aynı saatte başka randevun var mı?)
                bool kullaniciDoluMu = await _context.Randevular.AnyAsync(r =>
                    r.KullaniciId == user.Id &&
                    r.Tarih == randevu.Tarih &&
                    r.Saat == randevu.Saat &&
                    r.Durum != "İptal Edildi");

                if (kullaniciDoluMu)
                {
                    ModelState.AddModelError("", "Aynı tarih ve saatte zaten başka bir randevunuz var.");
                    YenidenDoldur(randevu);
                    return View(randevu);
                }

                // Her şey temizse kaydet
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Model hatalıysa sayfayı geri yükle
            YenidenDoldur(randevu);
            return View(randevu);
        }

        // --- YARDIMCI METOD: Hata Durumunda Dropdownları Tekrar Doldur ---
        private void YenidenDoldur(Randevu randevu)
        {
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);

            // Sadece o hocanın hizmetlerini mi, yoksa hepsini mi getirelim?
            // Hata durumunda JS tekrar çalışmayabilir, en garantisi seçili hocaya göre doldurmak.
            if (randevu.AntrenorId != 0)
            {
                var antrenor = _context.Antrenorler.Find(randevu.AntrenorId);
                if (antrenor != null)
                {
                    var hizmetler = _context.Hizmetler
                        .Where(h => h.UzmanlikId == antrenor.UzmanlikId)
                         .Select(h => new
                         {
                             Id = h.Id,
                             Metin = $"{h.SureDk}dk - {h.Ucret} TL"
                         }).ToList();
                    ViewData["HizmetId"] = new SelectList(hizmetler, "Id", "Metin", randevu.HizmetId);
                    return;
                }
            }

            // Eğer hoca seçili değilse boş gönder
            ViewData["HizmetId"] = new SelectList(new List<string>());
        }

        // --- ADMİN İŞLEMLERİ (ONAYLA / İPTAL ET) ---
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumDegistir(int id, string yeniDurum)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = yeniDurum;
            _context.Update(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- AJAX: EĞİTMENE GÖRE DERSLERİ GETİR ---
        [HttpGet]
        public async Task<JsonResult> GetHizmetlerByAntrenor(int antrenorId)
        {
            var antrenor = await _context.Antrenorler.FindAsync(antrenorId);
            if (antrenor == null) return Json(null);

            var hizmetler = await _context.Hizmetler
                .Where(h => h.UzmanlikId == antrenor.UzmanlikId)
                .Select(h => new
                {
                    id = h.Id,
                    metin = $"{h.SureDk}dk - {h.Ucret} TL"
                })
                .ToListAsync();

            return Json(hizmetler);
        }
    }
}