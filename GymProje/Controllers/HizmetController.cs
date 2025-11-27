using GymProje.Data;
using GymProje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymProje.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin
    public class HizmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME (Index)
        public async Task<IActionResult> Index()
        {
            // Hizmetleri getirirken bağlı olduğu Uzmanlık bilgisini de (Include) getir
            var hizmetler = await _context.Hizmetler.Include(h => h.Uzmanlik).ToListAsync();
            return View(hizmetler);
        }

        // 2. EKLEME SAYFASI (GET)
        public IActionResult Create()
        {
            // Dropdown için branşları view'a gönderiyoruz
            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad");
            return View();
        }

        // 3. EKLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            // Uzmanlik nesnesi formdan gelmez, sadece ID gelir. Hata vermesin diye siliyoruz.
            ModelState.Remove("Uzmanlik");

            if (ModelState.IsValid)
            {
                _context.Add(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Hata varsa dropdown'ı tekrar doldur
            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad", hizmet.UzmanlikId);
            return View(hizmet);
        }

        // 4. SİLME (Onay Sayfası)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();
            return View(hizmet);
        }

        // 5. SİLME (İşlemi Yap)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}