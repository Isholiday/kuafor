using backend.Data;
using backend.Services;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;
public class AccountController(ApplicationDbContext context) : Controller {

    private readonly ApplicationDbContext _context = context;

    public IActionResult Index() { return RedirectToAction("Login"); }

    [HttpGet]
    public IActionResult Login() { return View(); }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model) {
        if (!ModelState.IsValid) return View(model);

        try {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !PasswordEncryption.VerifyPassword(model.Password!, user.Password!)) {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            TempData["UserId"] = user.Id;
            return user.IsAdmin
                ? RedirectToAction("AdminDashboard", "Dashboard")
                : RedirectToAction("UserDashboard", "Dashboard");
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register() { return View(); }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model) {
        if (!ModelState.IsValid) return View(model);

        try {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (existingUser != null) {
                ModelState.AddModelError(string.Empty, "Username is already taken.");
                return View(model);
            }

            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingEmail != null) {
                ModelState.AddModelError(string.Empty, "Email is already in use.");
                return View(model);
            }

            _context.Users.Add(PasswordEncryption.HashPassword(model));
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Registration successful! You can log in now.";

            return RedirectToAction("Login");
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(model);
        }
    }


}

