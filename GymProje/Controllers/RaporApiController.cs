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

  

        // 1. GÜNLÜK RAPOR GETİR
        [HttpGet("GunlukRapor")]
        public async Task<IActionResult> GetGunlukRapor(DateTime? tarih)
        {
            if (tarih == null) tarih = DateTime.Today;

            var sonuclar = await _context.Randevular
                .Include(r => r.Kullanici)  // Üye bilgilerini getir
                .Include(r => r.Hizmet)     // Hizmet detaylarını getir (Süre, Ücret)
                    .ThenInclude(h => h.Uzmanlik) // Hizmetin içindeki Uzmanlık Adını almak için
                .Where(r => r.Tarih.Date == tarih.Value.Date) // LINQ Filtreleme
                .Select(r => new
                {
                    Musteri = r.Kullanici.Ad + " " + r.Kullanici.Soyad,

                    // DÜZELTME BURADA: Hizmet.Ad yoktu, Hizmet.Uzmanlik.Ad yaptık.
                    Hizmet = r.Hizmet.Uzmanlik != null ? r.Hizmet.Uzmanlik.Ad : "Genel Hizmet",

                    Saat = r.Saat,
                    Durum = r.Durum // Senin sistemde string olduğu için (Bekliyor/Onaylandı)
                })
                .ToListAsync();

            return Ok(sonuclar);
        }

        // 2. TÜM ANTRENÖRLERİ LİSTELE
        [HttpGet("trainers")]
        public async Task<IActionResult> GetTrainers()
        {
            var trainers = await _context.Antrenorler
                .Include(t => t.Uzmanlik)
                .Select(t => new
                {
                    AdSoyad = t.AdSoyad,
                    Uzmanlik = t.Uzmanlik != null ? t.Uzmanlik.Ad : "Genel",
                    Saatler = $"{t.CalismaBaslangicSaati}:00 - {t.CalismaBitisSaati}:00"
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // 3. GENEL İSTATİSTİKLER
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

        // 4. POPÜLER EĞİTMENLER
        [HttpGet("PopulerEgitmenler")]
        public async Task<IActionResult> GetPopulerEgitmenler()
        {
            var liste = await _context.Randevular
                .Include(r => r.Antrenor)
                .Where(r => r.Antrenor != null)
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