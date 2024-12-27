using System.Security.Claims;
using backend.Data;
using backend.Services;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;
public class AccountController(ApplicationDbContext context, JwtService jwtService) : Controller {

    private readonly ApplicationDbContext _context = context;
    private readonly JwtService _jwtService = jwtService;

    public IActionResult Index() { return RedirectToAction("Login"); }

    [HttpGet]
    public IActionResult Login() {
        var token = Request.Cookies["JWT"];
        if (token != null && _jwtService.ValidateToken(token, out var principal)) {
            var role = principal?.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Admin"
                ? RedirectToAction("AdminDashboard", "Dashboard")
                : RedirectToAction("UserDashboard", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model) {
        if (!ModelState.IsValid) return View(model);

        try {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !PasswordEncryption.VerifyPassword(model.Password!, user.Password!)) {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var role = user.IsAdmin ? "Admin" : "User";
            var token = _jwtService.GenerateToken(user.Id, user.Username!, role);

            Response.Cookies.Append("JWT", token, new CookieOptions {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(120)
            });

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

    [HttpPost]
    public IActionResult Logout() {
        Response.Cookies.Delete("JWT", new CookieOptions {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });

        return RedirectToAction(nameof(Login));
    }
}

