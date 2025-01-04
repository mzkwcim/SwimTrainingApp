using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SwimTrainingApp.Data;
using SwimTrainingApp.Models;
using Microsoft.AspNetCore.Authentication;

namespace SwimTrainingApp.Controllers
{
    [Authorize] 
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        
        public IActionResult Index()
        {
            
            var username = User.Identity?.Name;

            
            ViewBag.Username = username;

            return View(); 
        }

        [AllowAnonymous] 
        [HttpGet]
        public IActionResult Login()
        {
            return View(); 
        }

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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);

            return RedirectToAction("Index"); 
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
