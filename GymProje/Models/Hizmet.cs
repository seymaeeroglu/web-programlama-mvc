using System.ComponentModel.DataAnnotations;

namespace GymProje.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        // Ad satırını sildik, artık yok.

        [Required]
        [Display(Name = "Süre (Dakika)")]
        public int SureDk { get; set; }

        [Required]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }

        // İlişkiler
        [Display(Name = "Branş")]
        public int UzmanlikId { get; set; }
        public Uzmanlik? Uzmanlik { get; set; }
    }
}