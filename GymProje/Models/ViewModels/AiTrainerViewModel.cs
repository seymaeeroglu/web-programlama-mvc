using System.ComponentModel.DataAnnotations;

namespace GymProje.Models.ViewModels
{
    public class AiTrainerViewModel
    {
        [Required(ErrorMessage = "Yaş bilgisi gereklidir.")]
        [Range(10, 100, ErrorMessage = "Geçerli bir yaş giriniz.")]
        public int Yas { get; set; }

        [Required(ErrorMessage = "Boy bilgisi gereklidir (cm).")]
        [Range(100, 250, ErrorMessage = "Geçerli bir boy giriniz.")]
        public int Boy { get; set; }

        [Required(ErrorMessage = "Kilo bilgisi gereklidir (kg).")]
        [Range(30, 200, ErrorMessage = "Geçerli bir kilo giriniz.")]
        public int Kilo { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçiniz.")]
        public string Cinsiyet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hedefinizi seçiniz.")]
        public string Hedef { get; set; } = string.Empty;

        // AI Cevabı
        public string? AiCevabi { get; set; }
    }
}