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

        private static readonly Dictionary<string, (int Attempts, DateTime LastAttempt)> LoginAttempts = new();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (LoginAttempts.ContainsKey(clientIp))
            {
                var (attempts, lastAttempt) = LoginAttempts[clientIp];

                if (attempts >= 5 && lastAttempt.AddMinutes(1) > DateTime.Now)
                {
                    return Content("Zbyt wiele nieudanych prób logowania. Spróbuj ponownie za minutę.");
                }

                if (lastAttempt.AddMinutes(1) <= DateTime.Now)
                {
                    LoginAttempts[clientIp] = (0, DateTime.Now); // Reset prób po czasie
                }
            }

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


            await HttpContext.SignInAsync(
                 principal,
                new AuthenticationProperties
                {
                    //IsPersistent = true, 
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30), 
                    AllowRefresh = true
                });

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
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Password cannot be empty";
                return View();
            }

            if (password.Length < 12)
            {
                ViewBag.ErrorMessage = "Password should have at least 12 characters.";
                return View();
            }

            if (!password.Any(char.IsLower))
            {
                ViewBag.ErrorMessage = "Password should contain at least one lowercase letter.";
                return View();
            }

            if (!password.Any(char.IsUpper))
            {
                ViewBag.ErrorMessage = "Password should contain at least one uppercase letter.";
                return View();
            }

            if (!password.Any(char.IsDigit))
            {
                ViewBag.ErrorMessage = "Password should contain at least one digit.";
                return View();
            }

            if (!password.Any(ch => char.IsPunctuation(ch) || char.IsSymbol(ch)))
            {
                ViewBag.ErrorMessage = "Password should contain at least one special character.";
                return View();
            }

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
        public IActionResult AccessDenied(string returnUrl = "/home/login")
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
