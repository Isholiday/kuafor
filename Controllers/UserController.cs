using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Authorize(Roles = "Admin")]
[Route("/panel/[controller]")]
public class UserController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index() {
        try {
            var users = await _context.Users
                .Include(u => u.Employee)
                .ToListAsync();
            return View(users);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("make-admin/{id}")]
    [HttpPost]
    public async Task<IActionResult> MakeAdmin(int id) {
        try {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsAdmin = true;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User has been made admin successfully.";
        } catch (Exception) {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Route("revoke-admin/{id}")]
    [HttpPost]
    public async Task<IActionResult> RevokeAdmin(int id) {
        try {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsAdmin = false;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Admin status has been revoked successfully.";
        } catch (Exception) {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Route("select-employee/{id}")]
    [HttpGet]
    public async Task<IActionResult> SelectEmployee(int id) {
        var availableEmployees = await _context.Employees
            .Where(e => !_context.Users.Any(u => u.EmployeeId == e.Id))
            .ToListAsync();

        ViewBag.AvailableEmployees = availableEmployees;
        return View(id);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmployee(int userId, int employeeId) {
        try {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.EmployeeId = employeeId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User has been assigned to employee successfully.";
        } catch (Exception) {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Route("revoke-employee/{id}")]
    [HttpPost]
    public async Task<IActionResult> RevokeEmployee(int id) {
        try {
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            user.EmployeeId = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee status has been revoked successfully.";
        } catch (Exception) {
            TempData["InfoMessage"] = "An error occurred while updating user status.";
        }
        return RedirectToAction(nameof(Index));
    }
}
