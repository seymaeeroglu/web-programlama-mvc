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
            // Gelen veri kurallara uyuyor mu? 
            {
                _context.Add(uzmanlik);          // RAM'e ekle
                await _context.SaveChangesAsync(); // Veritabanına yaz
                return RedirectToAction(nameof(Index)); // Listeleme sayfasına dön
            }
            return View(uzmanlik); // Hata varsa formu tekrar göster
        }

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
                //Bu branşa bağlı antrenör var mı?
                bool antrenorVarMi = await _context.Antrenorler.AnyAsync(a => a.UzmanlikId == id);

                if (antrenorVarMi)
                {
                    ModelState.AddModelError("", "Bu branşa kayıtlı antrenörler olduğu için isim değişikliği yapılamaz. Önce antrenörlerin branşını değiştiriniz.");
                    return View(uzmanlik); 
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
            //  Bu branşa bağlı antrenör var mı?
            bool antrenorVarMi = await _context.Antrenorler.AnyAsync(a => a.UzmanlikId == id);

            if (antrenorVarMi)
            {
                var uzmanlik = await _context.Uzmanliklar.FindAsync(id);

                ViewBag.HataMesaji = "Bu branşa kayıtlı antrenörler var! Silmek için önce antrenörleri silmeli veya başka branşa almalısınız.";

                return View(uzmanlik);
            }

            
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