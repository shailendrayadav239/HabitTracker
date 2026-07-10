using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTracker.ApplicationDBContext;
using HabitTracker.Entities;
using HabitTracker.ViewModels;

namespace HabitTracker.Controllers
{
    public class SignupController : Controller
    {
        private readonly AppDbContext _context;

        public SignupController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View(new SignupViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SignupViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool emailExists = await _context.Signups.AnyAsync(s => s.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var signup = new Signup
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Address = model.Address,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                DOB = model.DOB
            };

            _context.Signups.Add(signup);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Signup successful! You can now log in.";
            return RedirectToAction("Index", "Login");
        }
    }
}