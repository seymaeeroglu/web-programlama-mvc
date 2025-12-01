using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymProje.Data;
using GymProje.Models;

namespace GymProje.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaporApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RaporApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- BÖLÜM 1: HOCANIN İSTEDİĞİ URL SORGULARI ---

        // 1. TÜM ANTRENÖRLERİ GETİR
        // URL: /api/RaporApi/trainers
        [HttpGet("trainers")]
        public async Task<IActionResult> GetTrainers()
        {
            var trainers = await _context.Antrenorler
                .Include(t => t.Uzmanlik)
                .Select(t => new
                {
                    Id = t.Id,
                    AdSoyad = t.AdSoyad,
                    Uzmanlik = t.Uzmanlik != null ? t.Uzmanlik.Ad : "Belirtilmemiş",
                    CalismaSaatleri = $"{t.CalismaBaslangicSaati}:00 - {t.CalismaBitisSaati}:00"
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // 2. BELİRLİ TARİHTEKİ RANDEVULARI GETİR
        // URL: /api/RaporApi/appointments?date=2025-11-30
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments(DateTime? date)
        {
            if (date == null) return BadRequest("Tarih giriniz.");

            var appointments = await _context.Randevular
                .Include(a => a.Antrenor)
                .Include(a => a.Kullanici)
                .Where(a => a.Tarih.Date == date.Value.Date)
                .Select(a => new
                {
                    Tarih = a.Tarih.ToString("dd.MM.yyyy"),
                    Saat = a.Saat,
                    Hoca = a.Antrenor != null ? a.Antrenor.AdSoyad : "-",
                    Durum = a.Durum
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // --- BÖLÜM 2: ADMIN PANELİ EKRANI İÇİN GEREKLİ OLANLAR (BUNLARI GERİ EKLEDİK) ---

        // 3. GENEL İSTATİSTİKLER (Kutular İçin)
        // URL: /api/RaporApi/GenelIstatistik
        [HttpGet("GenelIstatistik")]
        public async Task<IActionResult> GetGenelIstatistik()
        {
            var veri = new
            {
                ToplamUye = await _context.Users.CountAsync(),
                ToplamAntrenor = await _context.Antrenorler.CountAsync(),
                ToplamHizmet = await _context.Hizmetler.CountAsync(),
                BekleyenRandevular = await _context.Randevular.CountAsync(r => r.Durum == "Bekliyor")
            };
            return Ok(veri);
        }

        // 4. POPÜLER EĞİTMENLER (Tablo İçin)
        // URL: /api/RaporApi/PopulerEgitmenler
        [HttpGet("PopulerEgitmenler")]
        public async Task<IActionResult> GetPopulerEgitmenler()
        {
            var liste = await _context.Randevular
                .Include(r => r.Antrenor)
                .Where(r => r.Antrenor != null) // Silinen hocaları getirme
                .GroupBy(r => r.Antrenor.AdSoyad)
                .Select(g => new
                {
                    EgitmenAdi = g.Key,
                    RandevuSayisi = g.Count()
                })
                .OrderByDescending(x => x.RandevuSayisi)
                .Take(5)
                .ToListAsync();

            return Ok(liste);
        }
    }
}