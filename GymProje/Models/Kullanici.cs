using Microsoft.AspNetCore.Identity;

namespace GymProje.Models
{
    public class Kullanici : IdentityUser
    {
        // = string.Empty; diyerek başlangıç değerini boş atadım, hata yok.
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;

        public DateTime? DogumTarihi { get; set; }
    }
}