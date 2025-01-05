using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SwimTrainingApp.Data;
using SwimTrainingApp.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;
using System.Text;

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
            // Hash the entered password
            string hashedPassword = HashPassword(password);

            // Check if a user exists with the provided username and hashed password
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }

            // Create claims and sign in the user
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
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(); // Render the registration form
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Update Register method
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(string username, string password, UserRole role)
        {
            if (_db.Users.Any(u => u.Username == username))
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            var hashedPassword = HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Password = hashedPassword, // Save the hashed password
                Role = role
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

    }
}
