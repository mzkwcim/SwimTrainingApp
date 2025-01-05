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

            if (username == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Username = username;

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            
            string hashedPassword = HashPassword(password);

          
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }

            
            string roleName = ((UserRole)user.Role).ToString();

           
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()), 
                new Claim(ClaimTypes.GivenName, user.Username), 
                new Claim(ClaimTypes.Role, roleName) 
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Username and password are required.";
                return View();
            }

            if (_db.Users.Any(u => u.Username == username))
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            var hashedPassword = HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Password = hashedPassword,
                Role = UserRole.Athlete 
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string returnUrl = "/")
        {
            ViewBag.ReturnUrl = returnUrl ?? "/";
            ViewData["Title"] = "Access Denied";
            return View();
        }


        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
