using System.ComponentModel.DataAnnotations;

namespace GymProje.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required]
        public string Saat { get; set; } = string.Empty; // Başlangıç değeri atandı

        public string Durum { get; set; } = "Bekliyor";

        // İlişkiler
        public string KullaniciId { get; set; } = string.Empty; // ID boş olamaz
        public Kullanici? Kullanici { get; set; } // Nesne null olabilir (?)

        public int AntrenorId { get; set; }
        public Antrenor? Antrenor { get; set; } // Nesne null olabilir (?)

        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; } // Nesne null olabilir (?)
    }
}