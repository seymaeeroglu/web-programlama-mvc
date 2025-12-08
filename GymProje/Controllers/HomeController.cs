// Bu Controller, uygulamanýn vitrin (Anasayfa, Hakkýmýzda vb.) sayfalarýný yönetir.
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using GymProje.Models;
using GymProje.Data;

namespace GymProje.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // 1. Veritabaný Baðlantýsý için deðiþken tanýmlýyoruz
        private readonly ApplicationDbContext _context;

        // 2. Constructor (Yapýcý Metot) içine context'i ekliyoruz
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Veritabaný baðlantýsýný aldýk
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // --- YENÝ EKLENEN METOD: HÝZMETLERÝ LÝSTELE ---
        // Kullanýcý menüden "Hizmetler"e týkladýðýnda burasý çalýþacak
        public async Task<IActionResult> Hizmetler()
        {
            // Hizmetleri, baðlý olduðu branþ (Uzmanlik) bilgisiyle beraber getir
            var hizmetler = await _context.Hizmetler.Include(h => h.Uzmanlik).ToListAsync();
            return View(hizmetler);
        }

        // --- YENÝ: EÐÝTMENLER VÝTRÝNÝ ---
        public async Task<IActionResult> Egitmenler()
        {
            // Antrenörleri ve Uzmanlýk alanlarýný getir
            var egitmenler = await _context.Antrenorler
                                           .Include(a => a.Uzmanlik)
                                           .ToListAsync();
            return View(egitmenler);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}