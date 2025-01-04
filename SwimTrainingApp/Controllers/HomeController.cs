using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SwimTrainingApp.Data;
using SwimTrainingApp.Models;
using Microsoft.AspNetCore.Authentication;

namespace SwimTrainingApp.Controllers
{
    [Authorize] // Wymaga autoryzacji do uzyskania dostępu do strony głównej
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        // Strona główna (GET)
        public IActionResult Index()
        {
            // Pobranie nazwy zalogowanego użytkownika
            var username = User.Identity?.Name;

            // Przekazanie nazwy użytkownika do widoku
            ViewBag.Username = username;

            return View(); // Renderowanie widoku Index.cshtml
        }

        // Strona logowania (GET)
        [AllowAnonymous] // Logowanie nie wymaga autoryzacji
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Renderowanie widoku Login.cshtml
        }

        // Logowanie użytkownika (POST)
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }

            // Tworzenie tożsamości użytkownika
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            // Zalogowanie użytkownika
            await HttpContext.SignInAsync(principal);

            return RedirectToAction("Index"); // Przekierowanie na stronę główną
        }

        // Wylogowanie użytkownika (GET)
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
