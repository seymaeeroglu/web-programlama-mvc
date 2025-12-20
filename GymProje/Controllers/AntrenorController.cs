using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymProje.Data;
using GymProje.Models;

namespace GymProje.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Adminler erişebilir
    public class AntrenorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME (Index)
        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                                            .Include(a => a.Uzmanlik)
                                            .ToListAsync();
            return View(antrenorler);
        }

        // 2. EKLEME SAYFASI (GET)
        public IActionResult Create()
        {
            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad");
            return View();
        }

        // 3. EKLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Antrenor antrenor)
        {
            // Dropdown ve Randevular validasyon hatası vermesin diye temizliyoruz
            ModelState.Remove("Uzmanlik");
            ModelState.Remove("Randevular");

            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad", antrenor.UzmanlikId);
            return View(antrenor);
        }

        // 4. DÜZENLEME SAYFASI (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad", antrenor.UzmanlikId);
            return View(antrenor);
        }

        // 5. DÜZENLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Antrenor antrenor)
        {
            if (id != antrenor.Id) return NotFound();

            ModelState.Remove("Uzmanlik");
            ModelState.Remove("Randevular");

            if (ModelState.IsValid)
            {
                try
                {
                    // Sadece metin bilgilerini güncelliyoruz
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Antrenorler.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad", antrenor.UzmanlikId);
            return View(antrenor);
        }

        // 6. SİLME ONAY SAYFASI (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Uzmanlik)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // 7. SİLME İŞLEMİ (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // GÜVENLİK KONTROLÜ: Randevusu olan silinemez
            bool randevuVarMi = await _context.Randevular.AnyAsync(r => r.AntrenorId == id);

            if (randevuVarMi)
            {
                TempData["HataMesaji"] = $"{antrenor.AdSoyad} isimli eğitmenin kayıtlı randevuları olduğu için SİLİNEMEZ! Lütfen önce randevuları iptal edin.";
                return RedirectToAction(nameof(Index));
            }

            _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}