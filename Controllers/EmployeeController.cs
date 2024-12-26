using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Route("/panel/[controller]")]
public class EmployeeController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index() {
        try {
            var employees = await _context.Employees
            .Include(e => e.Availabilities)
            .Include(e => e.Salon)
            .ToListAsync();
            return View(employees);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Create")]
    [HttpGet]
    public IActionResult Create() {
        ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name");
        return View(new Employee { Availabilities = [], Skills = [] });
    }

    [Route("Create")]
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,Specialization,Skills,Availabilities,SalonId")] Employee employee) {
        if (employee.Availabilities != null && employee.Availabilities.Count > 0) {
            var duplicateDays = employee.Availabilities
                .GroupBy(a => new { a.Day, a.StartTime, a.EndTime })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key.Day);

            if (duplicateDays.Any()) {
                ModelState.AddModelError(string.Empty, "Duplicate availability entries are not allowed.");
                ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", employee.SalonId);
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
                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            } catch (Exception) {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the employee.");
                ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", employee.SalonId);
                return View(employee);
            }
        }

        ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", employee.SalonId);
        return View(employee);
    }

    [Route("Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
        try {
            var employee = await _context.Employees
                .Include(e => e.Availabilities)
                .Include(e => e.Salon)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            ViewData["Salons"] = new SelectList(
                await _context.Salons.ToListAsync(),
                "Id",
                "Name",
                employee.SalonId
            );

            return View(employee);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(Employee employee) {
        if (!ModelState.IsValid) return View(employee);

        try {
            var existingEmployee = await _context.Employees
                .Include(e => e.Availabilities)
                .FirstOrDefaultAsync(e => e.Id == employee.Id);

            if (existingEmployee == null) return NotFound();

            bool hasChanges = false;

            if (existingEmployee.Name != employee.Name ||
                existingEmployee.Specialization != employee.Specialization) {
                hasChanges = true;
            }

            var newSkills = employee.Skills != null && employee.Skills.Count > 0
                ? employee.Skills[0].Split(',').Select(s => s.Trim()).ToList()
                : [];

            if (!Enumerable.SequenceEqual(
                existingEmployee.Skills ?? [],
                newSkills)) {
                hasChanges = true;
            }

            var existingAvailabilities = existingEmployee.Availabilities ?? new List<Availability>();
            var newAvailabilities = employee.Availabilities ?? new List<Availability>();

            if (existingAvailabilities.Count != newAvailabilities.Count ||
                !existingAvailabilities.All(ea =>
                    newAvailabilities.Any(na =>
                        na.Day == ea.Day &&
                        na.StartTime == ea.StartTime &&
                        na.EndTime == ea.EndTime))) {
                hasChanges = true;
            }

            if (!hasChanges) {
                TempData["InfoMessage"] = "No changes were made to the employee.";
                return RedirectToAction(nameof(Index));
            }

            existingEmployee.Name = employee.Name;
            existingEmployee.Specialization = employee.Specialization;
            existingEmployee.Skills = newSkills;

            if (existingEmployee.Availabilities != null)
                _context.Availabilities.RemoveRange(existingEmployee.Availabilities);

            if (newAvailabilities.Count > 0) {
                var availabilitiesToAdd = newAvailabilities.Select(a => new Availability {
                    EmployeeId = existingEmployee.Id,
                    Day = a.Day,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                }).ToList();

                _context.Availabilities.AddRange(availabilitiesToAdd);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee updated successfully!";
            return RedirectToAction(nameof(Index));
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(employee);
        }
    }

    [Route("Delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
        try {
            var employee = await _context.Employees
                .Include(e => e.Availabilities)
                .Include(e => e.Salon)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Delete")]
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
