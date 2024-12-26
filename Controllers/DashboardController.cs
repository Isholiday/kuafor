using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

public class DashboardController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    [Route("/Dashboard")]
    public async Task<IActionResult> UserDashboard() {
        try {
            if (TempData["UserId"] == null) return RedirectToAction("Login", "Account");

            var userId = (int)TempData["UserId"]!;
            TempData.Keep("UserId");
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login", "Account");

            if (!user.EmployeeId.HasValue) {
                ViewBag.Salons = new SelectList(_context.Salons, "Id", "Name");
                ViewBag.UserAppointments = await _context.Appointments
                    .Include(a => a.Service)
                    .Include(a => a.Employee)
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();
            }

            return View(user);
        } catch (Exception) {
            TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            return RedirectToAction("Login", "Account");
        }
    }

    [Route("/Panel")]
    public IActionResult AdminDashboard() {
        try {
            if (TempData["UserId"] == null) return RedirectToAction("Login", "Account");

            var userId = (int)TempData["UserId"]!;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null || !user.IsAdmin) return RedirectToAction("Login", "Account");

            return View();
        } catch (Exception) {
            TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            return RedirectToAction("Login", "Account");
        }
    }


}
