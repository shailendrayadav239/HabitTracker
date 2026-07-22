using Microsoft.AspNetCore.Mvc;
using HabitTracker.ApplicationDBContext;
using HabitTracker.ViewModels;

namespace HabitTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return null;
            return Guid.Parse(userIdStr);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Index", "Login");

            var user = await _context.Signups.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Index", "Login");

            var model = new ProfileViewModel
            {
                Name = user.Name ?? string.Empty,
                Address = user.Address,
                DOB = user.DOB
            };

            ViewBag.Email = user.Email;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
            {
                TempData["ProfileError"] = "Please fill in all required fields correctly.";
                return RedirectToAction("Index");
            }

            var user = await _context.Signups.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Index", "Login");

            user.Name = model.Name.Trim();
            user.Address = model.Address;
            user.DOB = model.DOB;
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", user.Name ?? "User");
            TempData["ProfileSuccess"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = "Please check your password fields.";
                return RedirectToAction("Index");
            }

            var user = await _context.Signups.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Index", "Login");

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                TempData["PasswordError"] = "Current password is incorrect.";
                return RedirectToAction("Index");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["PasswordSuccess"] = "Password changed successfully.";
            return RedirectToAction("Index");
        }
    }
}