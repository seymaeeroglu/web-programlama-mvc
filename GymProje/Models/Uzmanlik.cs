using System.ComponentModel.DataAnnotations;

namespace GymProje.Models
{
    public class Uzmanlik
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Uzmanlık adı zorunludur.")]
        [Display(Name = "Uzmanlık Adı")]
        public string Ad { get; set; } = string.Empty; 

     
        public ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();


    }


}