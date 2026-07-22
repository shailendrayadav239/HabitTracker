using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTracker.ApplicationDBContext;

namespace HabitTracker.Controllers
{
    public class HabitDetailController : Controller
    {
        private readonly AppDbContext _context;

        public HabitDetailController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Habit/{id:guid}")]
        public async Task<IActionResult> Index(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Index", "Login");

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == Guid.Parse(userIdStr));

            if (habit == null) return NotFound();

            ViewBag.HabitId = habit.Id;
            ViewBag.HabitName = habit.Name;
            return View();
        }
    }
}