using System.Security.Claims;
using backend.Data;
using backend.ViewModels;
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

            if (user.EmployeeId.HasValue) return View(user);
            ViewBag.Salons = new SelectList(_context.Salons, "Id", "Name");
            ViewBag.UserAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(user);
        } catch {
            return RedirectToAction("Login", "Account");
        }
    }

    [Authorize(Roles = "Admin")]
    [Route("Panel")]
    public async Task<IActionResult> AdminDashboard() {
        var today = DateTime.Today;
        var nextMonday = today.AddDays(((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7);
        var nextSunday = nextMonday.AddDays(6);

        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Employee)
            .Where(a => a.AppointmentDate.Date >= nextMonday &&
                        a.AppointmentDate.Date <= nextSunday &&
                        a.IsConfirmed)
            .ToListAsync();

        var employeeRevenues = await _context.Employees
            .Select(e => e.Id)
            .ToListAsync();

        var viewModel = employeeRevenues
            .Select(employeeId => {
                var employeeAppointments = appointments
                    .Where(a => a.EmployeeId == employeeId)
                    .ToList();

                return new EmployeeRevenueViewModel {
                    EmployeeName = _context.Employees.First(e => e.Id == employeeId).Name!,
                    AppointmentCount = employeeAppointments.Count,
                    TotalHours = employeeAppointments.Sum(a => a.Service!.Duration.TotalHours),
                    Revenue = employeeAppointments.Sum(a => a.Service!.Price)
                };
            })
            .ToList();

        return View(viewModel);
    }
}
