using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTracker.ApplicationDBContext;
using HabitTracker.ViewModels;

namespace HabitTracker.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Signups.FirstOrDefaultAsync(s => s.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name ?? "User");

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}