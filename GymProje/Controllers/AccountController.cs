using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymProje.Models;
using GymProje.Models.ViewModels;

namespace GymProje.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Kullanici> _signInManager;
        private readonly UserManager<Kullanici> _userManager;

       
        public AccountController(SignInManager<Kullanici> signInManager, UserManager<Kullanici> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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

            // Giriş yapmayı dene
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Sifre, model.BeniHatirla, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Geçersiz giriş denemesi (E-posta veya şifre hatalı).");
            return View(model);
        }

        // --- KAYIT OL (REGISTER) ---

        // 1. Sayfayı Getir
        public IActionResult Register()
        {
            return View();
        }

        // 2. Kayıt İşlemini Yap
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Kullanici
                {
                    UserName = model.Email, // Kullanıcı adı olarak e-posta kullanıyoruz
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad
                };

                // Kullanıcıyı oluştur (Artık _userManager tanımlı olduğu için hata vermez)
                var result = await _userManager.CreateAsync(user, model.Sifre);

                if (result.Succeeded)
                {
                    // Otomatik olarak "Uye" rolü ver
                    await _userManager.AddToRoleAsync(user, "Uye");

                    // Kayıttan sonra Login sayfasına yönlendir
                    return RedirectToAction("Login");
                }

                // Hata varsa ekrana bas
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
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