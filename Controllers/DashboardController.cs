using System.Security.Claims;
using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

public class DashboardController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    [Authorize(Roles = "User,Admin")]
    [Route("/Dashboard")]
    public async Task<IActionResult> UserDashboard() {
        try {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
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
            return RedirectToAction("Login", "Account");
        }
    }

    [Authorize(Roles = "Admin")]
    [Route("/Panel")]
    public IActionResult AdminDashboard() {
        return View();
    }


}
