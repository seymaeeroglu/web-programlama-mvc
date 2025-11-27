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

        // 1. RANDEVU GEÇMİŞİ (Kullanıcı sadece kendi randevularını görür)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Şu anki kullanıcıyı bul

            // Eğer Admisse hepsini görsün, Üye ise sadece kendininkini görsün
            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                randevular = randevular.Where(r => r.KullaniciId == user.Id);
            }

            return View(await randevular.OrderByDescending(r => r.Tarih).ToListAsync());
        }

        // 2. RANDEVU ALMA SAYFASI (GET)
        public IActionResult Create()
        {
            // Dropdownları dolduruyoruz
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad");
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");
            return View();
        }

        // 3. RANDEVU KAYDETME (POST) - İŞTE BEYİN KISMI BURASI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            // 1. Giriş yapan kullanıcıyı bul ve randevuya ata
            var user = await _userManager.GetUserAsync(User);
            randevu.KullaniciId = user.Id;

            // Bazı validasyonları temizle (Kullanıcı ve Durum otomatik atanacak)
            ModelState.Remove("Kullanici");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");
            ModelState.Remove("KullaniciId");

            if (ModelState.IsValid)
            {
                // --- İŞ MANTIĞI (BUSINESS LOGIC) --- 

                // A. Antrenörün çalışma saatlerini kontrol et
                var antrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);

                // Formdan gelen saat "14:00" string formatında, bunu sayıya (14) çevirelim
                int randevuSaati = int.Parse(randevu.Saat.Split(':')[0]);

                if (randevuSaati < antrenor.CalismaBaslangicSaati || randevuSaati >= antrenor.CalismaBitisSaati)
                {
                    ModelState.AddModelError("", "Seçilen saat antrenörün çalışma saatleri dışındadır.");
                    // Dropdownları tekrar doldur
                    ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
                    return View(randevu);
                }

                // B. O saatte başka randevu var mı? (Çakışma Kontrolü)
                bool doluMu = await _context.Randevular.AnyAsync(r =>
                    r.AntrenorId == randevu.AntrenorId &&
                    r.Tarih.Date == randevu.Tarih.Date &&
                    r.Saat == randevu.Saat);

                if (doluMu)
                {
                    ModelState.AddModelError("", "Üzgünüz, antrenör bu saatte dolu. Lütfen başka bir saat seçiniz.");
                    // Dropdownları tekrar doldur
                    ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
                    return View(randevu);
                }

                // Her şey temizse kaydet
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Model hatalıysa sayfayı geri yükle
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            return View(randevu);
        }

        // --- ADMİN İŞLEMLERİ (ONAYLA / İPTAL ET) ---

        [Authorize(Roles = "Admin")] // Sadece Admin tetikleyebilir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumDegistir(int id, string yeniDurum)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            // Durumu güncelle (Örn: "Onaylandı", "İptal Edildi")
            randevu.Durum = yeniDurum;

            _context.Update(randevu);
            await _context.SaveChangesAsync();

            // İşlem bitince listeye geri dön
            return RedirectToAction(nameof(Index));
        }
    }
}