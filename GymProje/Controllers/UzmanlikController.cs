using GymProje.Data;
using GymProje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProjeIsmi.Controllers
{
    // Sadece Admin yetkisi olanlar bu sınıfa erişebilir
    [Authorize(Roles = "Admin")]
    public class UzmanlikController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection (Bağımlılık Enjeksiyonu)
        // Veritabanı bağlantısını buraya çağırıyoruz.
        public UzmanlikController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME SAYFASI (GET)
        public async Task<IActionResult> Index()
        {
            // Veritabanındaki tüm uzmanlıkları liste (List) olarak getir
            var uzmanliklar = await _context.Uzmanliklar.ToListAsync();
            return View(uzmanliklar);
        }

        // 2. EKLEME SAYFASI (GET) - Formu Göster
        public IActionResult Create()
        {
            return View();
        }

        // 3. EKLEME İŞLEMİ (POST) - Formdan Gelen Veriyi Kaydet
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik önlemi (CSRF saldırılarına karşı)
        public async Task<IActionResult> Create(Uzmanlik uzmanlik)
        {
            // Gelen veri kurallara uyuyor mu? (Örn: Ad alanı dolu mu?)
            if (ModelState.IsValid)
            {
                _context.Add(uzmanlik);          // RAM'e ekle
                await _context.SaveChangesAsync(); // Veritabanına yaz
                return RedirectToAction(nameof(Index)); // Listeleme sayfasına dön
            }
            return View(uzmanlik); // Hata varsa formu tekrar göster
        }

        // --- DÜZENLEME (EDIT) ---

        // 1. Düzenleme Sayfasını Getir (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var uzmanlik = await _context.Uzmanliklar.FindAsync(id);
            if (uzmanlik == null) return NotFound();

            return View(uzmanlik);
        }

        // --- DÜZENLEME İŞLEMİ (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Uzmanlik uzmanlik)
        {
            if (id != uzmanlik.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // 1. KONTROL: Bu branşa bağlı antrenör var mı?
                bool antrenorVarMi = await _context.Antrenorler.AnyAsync(a => a.UzmanlikId == id);

                if (antrenorVarMi)
                {
                    // Hata Mesajı Ekle
                    ModelState.AddModelError("", "Bu branşa kayıtlı antrenörler olduğu için isim değişikliği yapılamaz. Önce antrenörlerin branşını değiştiriniz.");
                    return View(uzmanlik); // Sayfayı hata ile geri döndür
                }

                try
                {
                    _context.Update(uzmanlik);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Uzmanliklar.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(uzmanlik);
        }

        // --- SİLME (DELETE) ---

        // 1. Silme Onay Sayfasını Getir (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var uzmanlik = await _context.Uzmanliklar.FirstOrDefaultAsync(m => m.Id == id);
            if (uzmanlik == null) return NotFound();

            return View(uzmanlik);
        }

        // --- SİLME İŞLEMİ (POST) ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. KONTROL: Bu branşa bağlı antrenör var mı?
            bool antrenorVarMi = await _context.Antrenorler.AnyAsync(a => a.UzmanlikId == id);

            if (antrenorVarMi)
            {
                // Silinecek veriyi tekrar bul (View'a göndermek için)
                var uzmanlik = await _context.Uzmanliklar.FindAsync(id);

                // Hata mesajını ViewBag ile taşıyalım
                ViewBag.HataMesaji = "Bu branşa kayıtlı antrenörler var! Silmek için önce antrenörleri silmeli veya başka branşa almalısınız.";

                return View(uzmanlik); // Silme sayfasına geri dön (uyarı ile)
            }

            // Engel yoksa sil
            var silinecekUzmanlik = await _context.Uzmanliklar.FindAsync(id);
            if (silinecekUzmanlik != null)
            {
                _context.Uzmanliklar.Remove(silinecekUzmanlik);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}