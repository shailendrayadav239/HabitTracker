using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Login");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}