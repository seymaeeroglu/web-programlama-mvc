using Microsoft.AspNetCore.Identity;
using GymProje.Models; 

namespace GymProje.Data 
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

                var sonuc = await userManager.CreateAsync(yeniAdmin, "sau");

                if (sonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(yeniAdmin, "Admin");
                }
            }
        }
    }
}