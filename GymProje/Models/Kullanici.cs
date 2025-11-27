using Microsoft.AspNetCore.Identity;

namespace GymProje.Models
{
    public class Kullanici : IdentityUser
    {
        // = string.Empty; diyerek başlangıç değerini boş atıyoruz, hata gidiyor.
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;

        // İsterseniz doğum tarihini de ekleyelim, raporda "kullanıcı detayları" olarak geçer.
        public DateTime? DogumTarihi { get; set; }
    }
}