using System.ComponentModel.DataAnnotations;

namespace GymProje.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Süre (Dakika)")]
        public int SureDk { get; set; }

        [Required]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }


        [Display(Name = "Branş")]
        public int UzmanlikId { get; set; }
        public Uzmanlik? Uzmanlik { get; set; }
    }
}