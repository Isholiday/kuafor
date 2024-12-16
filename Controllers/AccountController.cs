using backend.Models;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;
public class AccountController(ApplicationDbContext context) : Controller {

    private readonly ApplicationDbContext _context = context;

    public IActionResult Index() { return RedirectToAction("Login"); }

    [HttpGet]
    public IActionResult Login() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(User model) {
        if (!ModelState.IsValid) return View(model);

        try {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !PasswordEncryption.VerifyPassword(model.Password!, user.Password!)) {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            return user.IsAdmin
                ? RedirectToAction("AdminDashboard", "Dashboard")
                : RedirectToAction("UserDashboard", "Dashboard");
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register() {
        return View();
    }

    [HttpPost]
    public IActionResult Register(User model) {
        if (!ModelState.IsValid) {
            return View(model);
        }

        return RedirectToAction("Login");
    }
}

