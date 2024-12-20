using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

public class EmployeeController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index() {
        try {
            var employees = await _context.Employees.Include(e => e.Availabilities).ToListAsync();
            return View(employees);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [HttpGet]
    public IActionResult Create() {
        return View(new Employee { Availabilities = [], Skills = [] });
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,Specialization,Skills,Availabilities")] Employee employee) {
        if (employee.Skills == null || (employee.Skills.Count == 1 && string.IsNullOrWhiteSpace(employee.Skills[0]))) {
            employee.Skills = [];
            ModelState.ClearValidationState("Skills");
            ModelState.MarkFieldValid("Skills");
        }

        if (employee.Availabilities != null && employee.Availabilities.Count > 0) {
            var duplicateDays = employee.Availabilities
                .GroupBy(a => a.Day)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateDays.Any()) {
                ModelState.AddModelError(string.Empty, "Duplicate availability days are not allowed.");
                return View(employee);
            }
        }

        if (ModelState.IsValid) {
            if (employee.Skills != null && employee.Skills.Count > 0) {
                employee.Skills = employee.Skills[0].Split(',').Select(s => s.Trim()).ToList();
            }
            try {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                if (employee.Availabilities != null && employee.Availabilities.Count != 0) {
                    foreach (var availability in employee.Availabilities) {
                        availability.Id = 0;
                        availability.EmployeeId = employee.Id;
                    }

                    _context.Availabilities.AddRange(employee.Availabilities);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction("Index");
            } catch (Exception) {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View();
            }

        }
        return View(employee);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
        try {
            var employee = await _context.Employees
                .Include(e => e.Availabilities)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }


    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        try {
            var employee = await _context.Employees
                .Include(e => e.Availabilities)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();
            if (employee.Availabilities != null) _context.Availabilities.RemoveRange(employee.Availabilities);

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee has been deleted successfully!";
            return RedirectToAction(nameof(Index));
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }
}

