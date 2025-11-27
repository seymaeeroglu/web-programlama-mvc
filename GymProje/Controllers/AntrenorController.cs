using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Dropdown (SelectList) için gerekli
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
        // Antrenörleri, uzmanlık bilgileriyle beraber getirir.
        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                                            .Include(a => a.Uzmanlik) // SQL'deki JOIN işlemi
                                            .ToListAsync();
            return View(antrenorler);
        }

        // 2. EKLEME SAYFASI (GET)
        // Form açılmadan önce Uzmanlıkları doldurup View'a gönderiyoruz.
        public IActionResult Create()
        {
            // ViewBag, Controller'dan View'a veri taşıyan basit bir çanta gibidir.
            // SelectList: (Kaynak, ArkaPlandaTutulacakDeger, EkrandaGorunecekDeger)
            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad");
            return View();
        }

        // 3. EKLEME İŞLEMİ (POST)
        // Resim yükleme ve kayıt işlemleri burada yapılır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Antrenor antrenor, IFormFile? resimDosyasi)
        {
            // Dropdown'dan veri geleceği için Uzmanlik nesnesinin kendisi boştur, hata vermesin diye temizliyoruz.
            ModelState.Remove("Uzmanlik");
            ModelState.Remove("Randevular");

            if (ModelState.IsValid)
            {
                // A. RESİM YÜKLEME İŞLEMİ
                if (resimDosyasi != null)
                {
                    // Dosya uzantısını al (.jpg, .png)
                    var uzanti = Path.GetExtension(resimDosyasi.FileName);
                    // Benzersiz isim oluştur (Çakışmayı önlemek için)
                    var yeniDosyaAdi = Guid.NewGuid().ToString() + uzanti;
                    // Kaydedilecek yer: wwwroot/img/
                    var kayitYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", yeniDosyaAdi);

                    // Dosyayı diske kaydet
                    using (var stream = new FileStream(kayitYolu, FileMode.Create))
                    {
                        await resimDosyasi.CopyToAsync(stream);
                    }

                    // Veritabanına dosya yolunu yaz ("/img/dosyaadi.jpg")
                    antrenor.ResimYolu = "/img/" + yeniDosyaAdi;
                }

                // B. VERİTABANI KAYDI
                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa (örneğin ad boşsa), Dropdown'ı tekrar doldurup sayfayı geri yükle
            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad", antrenor.UzmanlikId);
            return View(antrenor);
        }

        // --- DÜZENLEME (EDIT) ---

        // 1. Düzenleme Sayfasını Getir
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            // Uzmanlık dropdown'ını doldur (Mevcut uzmanlığı seçili gelsin)
            ViewData["UzmanlikId"] = new SelectList(_context.Uzmanliklar, "Id", "Ad", antrenor.UzmanlikId);
            return View(antrenor);
        }

        // 2. Düzenlemeyi Kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Antrenor antrenor, IFormFile? resimDosyasi)
        {
            if (id != antrenor.Id) return NotFound();

            // Uzmanlık ve Randevular nesnesi formdan gelmez, hata vermesin
            ModelState.Remove("Uzmanlik");
            ModelState.Remove("Randevular");

            if (ModelState.IsValid)
            {
                try
                {
                    // Yeni resim yüklendi mi?
                    if (resimDosyasi != null)
                    {
                        // 1. Yeni resmi kaydet
                        var uzanti = Path.GetExtension(resimDosyasi.FileName);
                        var yeniDosyaAdi = Guid.NewGuid().ToString() + uzanti;
                        var kayitYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", yeniDosyaAdi);

                        using (var stream = new FileStream(kayitYolu, FileMode.Create))
                        {
                            await resimDosyasi.CopyToAsync(stream);
                        }

                        // 2. Veritabanı yolunu güncelle
                        antrenor.ResimYolu = "/img/" + yeniDosyaAdi;
                    }
                    // Eğer resim yüklenmediyse, formdaki gizli inputtan gelen eski yolu koru (HTML tarafında halledeceğiz)

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

        // --- SİLME (DELETE) ---

        // 1. Silme Onay Sayfası
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Uzmanlik)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // 2. Silme İşlemini Yap
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}