using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Authorize(Roles = "Admin")]
[Route("/panel/[controller]")]
public class EmployeeController(ApplicationDbContext context) : Controller {
    public async Task<IActionResult> Index() {
        try {
            var employees = await context.Employees
                .Include(e => e.Availabilities)
                .Include(e => e.Salon)
                .ToListAsync();
            return View(employees);
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Create")]
    [HttpGet]
    public IActionResult Create() {
        ViewData["Salons"] = new SelectList(context.Salons, "Id", "Name");
        return View(new Employee { Availabilities = [], Skills = [] });
    }

    [Route("Create")]
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,Specialization,Skills,Availabilities,SalonId")] Employee employee) {
        if (employee.Availabilities.Count > 0) {
            var duplicateDays = employee.Availabilities
                .GroupBy(a => new { a.Day, a.StartTime, a.EndTime })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key.Day);

            if (duplicateDays.Any()) {
                ModelState.AddModelError(string.Empty, "Duplicate availability entries are not allowed.");
                ViewData["Salons"] = new SelectList(context.Salons, "Id", "Name", employee.SalonId);
                return View(employee);
            }
        }

        if (ModelState.IsValid) {
            if (employee.Skills.Count > 0) {
                employee.Skills = employee.Skills[0].Split(',').Select(s => s.Trim()).ToList();
            }

            try {
                context.Employees.Add(employee);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            } catch {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the employee.");
                ViewData["Salons"] = new SelectList(context.Salons, "Id", "Name", employee.SalonId);
                return View(employee);
            }
        }

        ViewData["Salons"] = new SelectList(context.Salons, "Id", "Name", employee.SalonId);
        return View(employee);
    }

    [Route("Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
        try {
            var employee = await context.Employees
                .Include(e => e.Availabilities)
                .Include(e => e.Salon)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            ViewData["Salons"] = new SelectList(
                await context.Salons.ToListAsync(),
                "Id",
                "Name",
                employee.SalonId
            );

            return View(employee);
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(Employee employee) {
        if (!ModelState.IsValid) return View(employee);

        try {
            var existingEmployee = await context.Employees
                .Include(e => e.Availabilities)
                .FirstOrDefaultAsync(e => e.Id == employee.Id);

            if (existingEmployee == null) return NotFound();
            var hasChanges = existingEmployee.Name != employee.Name || existingEmployee.Specialization != employee.Specialization;

            var newSkills = employee.Skills.Count > 0
                ? employee.Skills[0].Split(',').Select(s => s.Trim()).ToList()
                : [];

            if (!(existingEmployee.Skills ?? []).SequenceEqual(newSkills)) hasChanges = true;
            
            var existingAvailabilities = existingEmployee.Availabilities;
            var newAvailabilities = employee.Availabilities;

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

            context.Availabilities.RemoveRange(existingEmployee.Availabilities);

            if (newAvailabilities.Count > 0) {
                var availabilitiesToAdd = newAvailabilities.Select(a => new Availability {
                    EmployeeId = existingEmployee.Id,
                    Day = a.Day,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                }).ToList();

                context.Availabilities.AddRange(availabilitiesToAdd);
            }

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee updated successfully!";
            return RedirectToAction(nameof(Index));
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(employee);
        }
    }

    [Route("Delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
        try {
            var employee = await context.Employees
                .Include(e => e.Availabilities)
                .Include(e => e.Salon)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            var hasUser = await context.Users.AnyAsync(u => u.EmployeeId == id);
            if (!hasUser) return View(employee);
            
            TempData["InfoMessage"] = "Cannot delete employee because this employee is assigned to a user.";
            return RedirectToAction(nameof(Index));

        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Delete")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        try {
            var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();

            var hasUser = await context.Users.AnyAsync(u => u.EmployeeId == id);
            if (hasUser) {
                TempData["InfoMessage"] = "Cannot delete employee because this employee is assigned to a user.";
                return RedirectToAction(nameof(Index));
            }

            var hasAppointments = await context.Appointments.AnyAsync(a => a.EmployeeId == id);
            if (hasAppointments) {
                TempData["InfoMessage"] = "Cannot delete employee because they have existing appointments.";
                return RedirectToAction(nameof(Index));
            }

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee has been deleted successfully!";
            return RedirectToAction(nameof(Index));
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }
}
