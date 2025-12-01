using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymProje.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin görebilir
    public class RaporController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}