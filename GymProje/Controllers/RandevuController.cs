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
                .ThenInclude(h => h.Uzmanlik)
                .AsQueryable();

            if (user != null && !User.IsInRole("Admin"))
            {
                randevular = randevular.Where(r => r.KullaniciId == user.Id);
            }

            return View(await randevular.OrderByDescending(r => r.Tarih).ToListAsync());
        }

        // 2. RANDEVU ALMA SAYFASI (GET)
        [HttpGet]
        public IActionResult Create(int? hizmetId, int? antrenorId)
        {
            // Veri kontrolü
            if (!_context.Antrenorler.Any())
            {
                ViewBag.Hata = "Sistemde kayıtlı antrenör bulunamadı.";
                return View();
            }

            var antrenorListesi = _context.Antrenorler
                .Include(a => a.Uzmanlik)
                .Select(a => new
                {
                    Id = a.Id,
                    AdBilgisi = $"{a.AdSoyad} - {(a.Uzmanlik != null ? a.Uzmanlik.Ad : "Genel")}"
                })
                .ToList();

            // Eğer URL'den antrenorId geldiyse onu seçili yap
            ViewData["AntrenorId"] = new SelectList(antrenorListesi, "Id", "AdBilgisi", antrenorId);

            // 2. HİZMETİ SEÇİLİ GETİRMEK İÇİN
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            randevu.KullaniciId = user.Id;

            ModelState.Remove("Kullanici");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");
            ModelState.Remove("KullaniciId");

            if (ModelState.IsValid)
            {
                // 1. SAAT KONTROLÜ (09:00 - 00:00)
                int randevuSaati = 0;
                try
                {
                    randevuSaati = int.Parse(randevu.Saat.Split(':')[0]);
                }
                catch { randevuSaati = 0; }

                if (randevuSaati < 9)
                {
                    ModelState.AddModelError("", "Spor salonumuz sadece 09:00 - 00:00 saatleri arasında hizmet vermektedir.");
                    YenidenDoldur(randevu);
                    return View(randevu);
                }

                var antrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);

                // 2. EĞİTMEN SAAT KONTROLÜ
                if (randevuSaati < antrenor?.CalismaBaslangicSaati || randevuSaati >= antrenor?.CalismaBitisSaati)
                {
                    ModelState.AddModelError("", $"Seçtiğiniz eğitmen sadece {antrenor.CalismaBaslangicSaati}:00 - {antrenor.CalismaBitisSaati}:00 saatleri arasında çalışmaktadır.");
                    YenidenDoldur(randevu);
                    return View(randevu);
                }

                // 3. EĞİTMEN DOLULUK KONTROLÜ
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

                // 4. KULLANICI DOLULUK KONTROLÜ
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

                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            YenidenDoldur(randevu);
            return View(randevu);
        }

        // --- YARDIMCI METOD: Hata Durumunda Dropdownları Tekrar Doldur ---
        private void YenidenDoldur(Randevu randevu)
        {               
            var antrenorListesi = _context.Antrenorler
                .Include(a => a.Uzmanlik)
                .Select(a => new
                {
                    Id = a.Id,
                    AdBilgisi = $"{a.AdSoyad} - {(a.Uzmanlik != null ? a.Uzmanlik.Ad : "Genel")}"
                })
                .ToList();

            ViewData["AntrenorId"] = new SelectList(antrenorListesi, "Id", "AdBilgisi", randevu.AntrenorId);

            if (randevu.AntrenorId != 0)
            {
                var antrenor = _context.Antrenorler.Find(randevu.AntrenorId);
                if (antrenor != null)
                {
                    var hizmetler = _context.Hizmetler
                        .Include(h => h.Uzmanlik)
                        .Where(h => h.UzmanlikId == antrenor.UzmanlikId)
                         .Select(h => new
                         {
                             Id = h.Id,
                             Metin = $"{(h.Uzmanlik != null ? h.Uzmanlik.Ad : "")} - {h.SureDk}dk - {h.Ucret} TL"
                         }).ToList();
                    ViewData["HizmetId"] = new SelectList(hizmetler, "Id", "Metin", randevu.HizmetId);
                    return;
                }
            }
            ViewData["HizmetId"] = new SelectList(new List<string>());
        }

        // --- ADMİN İŞLEMLERİ ---
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
                .Include(h => h.Uzmanlik)
                .Where(h => h.UzmanlikId == antrenor.UzmanlikId)
                .Select(h => new
                {
                    id = h.Id,
                    metin = $"{h.Uzmanlik.Ad} - {h.SureDk}dk - {h.Ucret} TL"
                })
                .ToListAsync();

            return Json(hizmetler);
        }
    }
}