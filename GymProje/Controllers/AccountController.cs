using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymProje.Models;
using GymProje.Models.ViewModels;

namespace GymProje.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Kullanici> _signInManager;

        // Dependency Injection ile SignInManager'ı alıyoruz
        public AccountController(SignInManager<Kullanici> signInManager)
        {
            _signInManager = signInManager;
        }

        // GET: /Account/Login (Sayfayı Göster)
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login (Form Gönderildiğinde)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Giriş yapmayı dene (Kullanıcı adı olarak Email kullanıyoruz)
            // false: hesabı kilitleme, true: lockout (kilitlenme) aktif
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Sifre, model.BeniHatirla, false);

            if (result.Succeeded)
            {
                // Başarılıysa Anasayfaya git
                return RedirectToAction("Index", "Home");
            }

            // Başarısızsa hata mesajı ekle
            ModelState.AddModelError("", "Geçersiz giriş denemesi (E-posta veya şifre hatalı).");
            return View(model);
        }

        // Çıkış Yap
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}