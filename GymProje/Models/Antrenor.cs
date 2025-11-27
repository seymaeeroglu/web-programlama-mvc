using System.ComponentModel.DataAnnotations;

namespace GymProje.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty; // Başlangıç değeri atandı

        [Display(Name = "Fotoğraf")]
        public string? ResimYolu { get; set; } // Zaten '?' var, sorun yok

        [Range(0, 23)]
        public int CalismaBaslangicSaati { get; set; }
        [Range(0, 23)]
        public int CalismaBitisSaati { get; set; }

        // İlişkiler
        public int UzmanlikId { get; set; }
        public Uzmanlik? Uzmanlik { get; set; } // '?' ekledik (Hata vermemesi için)

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>(); // Liste başlatıldı
    }
}