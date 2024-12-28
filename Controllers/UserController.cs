using System.Security.Claims;
using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Authorize(Roles = "Admin")]
[Route("/panel/[controller]")]
public class UserController(ApplicationDbContext context) : Controller {
    public async Task<IActionResult> Index() {
        try {
            var users = await context.Users
                .Include(u => u.Employee)
                .ToListAsync();
            return View(users);
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("make-admin/{id:int}")]
    [HttpPost]
    public async Task<IActionResult> MakeAdmin(int id) {
        try {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsAdmin = true;
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User has been made admin successfully.";
        } catch {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Route("revoke-admin/{id:int}")]
    [HttpPost]
    public async Task<IActionResult> RevokeAdmin(int id) {
        try {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var isCurrentUser = currentUserId == id;

            user.IsAdmin = false;
            await context.SaveChangesAsync();

            if (isCurrentUser) {
                Response.Cookies.Delete("JWT");
                return RedirectToAction("Login", "Account");
            }

            TempData["SuccessMessage"] = "Admin status has been revoked successfully.";
            return RedirectToAction(nameof(Index));
        } catch {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Route("select-employee/{id:int}")]
    [HttpGet]
    public async Task<IActionResult> SelectEmployee(int id) {
        var availableEmployees = await context.Employees
            .Where(e => !context.Users.Any(u => u.EmployeeId == e.Id))
            .ToListAsync();

        ViewBag.AvailableEmployees = availableEmployees;
        return View(id);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmployee(int userId, int employeeId) {
        try {
            var user = await context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.EmployeeId = employeeId;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User has been assigned to employee successfully.";
        } catch {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Route("revoke-employee/{id:int}")]
    [HttpPost]
    public async Task<IActionResult> RevokeEmployee(int id) {
        try {
            var user = await context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (user.Employee != null) {
                var appointments = await context.Appointments
                    .Where(a => a.EmployeeId == user.Employee.Id)
                    .ToListAsync();
                context.Appointments.RemoveRange(appointments);
            }

            user.EmployeeId = null;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee status has been revoked successfully.";
        } catch {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
        }
        return RedirectToAction(nameof(Index));
    }
}
