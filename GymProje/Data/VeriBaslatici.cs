using Microsoft.AspNetCore.Identity;
using GymProje.Models; // Kendi proje isminizi yazın

namespace GymProje.Data // Kendi proje isminizi yazın
{
    public static class VeriBaslatici
    {
        public static async Task Baslat(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Kullanici>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Rolleri Oluştur (Admin ve Uye)
            string[] roller = { "Admin", "Uye" };

            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. Admin Kullanıcısını Oluştur
            // Belgedeki Format: ogrencinumarasi@sakarya.edu.tr
            // LÜTFEN AŞAĞIDAKİ E-POSTAYI KENDİ NUMARANIZLA GÜNCELLEYİN
            string adminEmail = "G231210062@sakarya.edu.tr";

            var adminKullanici = await userManager.FindByEmailAsync(adminEmail);

            if (adminKullanici == null)
            {
                var yeniAdmin = new Kullanici
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Ad = "Sistem",
                    Soyad = "Yöneticisi",
                    EmailConfirmed = true
                };

                // Şifre: sau (Belgede istenen şifre)
                var sonuc = await userManager.CreateAsync(yeniAdmin, "sau");

                if (sonuc.Succeeded)
                {
                    // Kullanıcıya Admin rolünü ata
                    await userManager.AddToRoleAsync(yeniAdmin, "Admin");
                }
            }
        }
    }
}