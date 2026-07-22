using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HabitTracker.Models;

namespace HabitTracker.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
{
    var userId = HttpContext.Session.GetString("UserId");
    if (!string.IsNullOrEmpty(userId))
        return RedirectToAction("Index", "Dashboard");

    return RedirectToAction("Index", "Login");
}
}
