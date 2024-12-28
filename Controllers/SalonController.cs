using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Authorize(Roles = "Admin")]
[Route("/panel/[controller]")]
public class SalonController(ApplicationDbContext context) : Controller {
    public async Task<IActionResult> Index() {
        try {
            var salons = await context.Salons.ToListAsync();
            return View(salons);
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Create")]
    [HttpGet]
    public IActionResult Create() {
        return View(new Salon());
    }

    [Route("Create")]
    [HttpPost]
    public async Task<IActionResult> Create(Salon salon) {
        if (!ModelState.IsValid) return View(salon);
        try {
            context.Salons.Add(salon);
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Salon created successfully!";
            return RedirectToAction(nameof(Index));
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
        }
        return View(salon);
    }

    [Route("Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
        try {
            var salon = await context.Salons.FindAsync(id);
            if (salon == null) return NotFound();
            return View(salon);
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(Salon salon) {
        if (!ModelState.IsValid) return View(salon);

        try {
            var existingSalon = await context.Salons.FindAsync(salon.Id);
            if (existingSalon == null) return NotFound();

            var hasChanges = existingSalon?.Name?.Equals(salon.Name) == false ||
                            existingSalon?.Address?.Equals(salon.Address) == false ||
                            existingSalon?.PhoneNumber?.Equals(salon.PhoneNumber) == false ||
                            existingSalon?.Email?.Equals(salon.Email) == false ||
                            existingSalon?.OpeningTime != salon.OpeningTime ||
                            existingSalon?.ClosingTime != salon.ClosingTime;

            if (!hasChanges) {
                TempData["InfoMessage"] = "No changes were made to the salon.";
                return RedirectToAction(nameof(Index));
            }

            if (existingSalon != null) context.Entry(existingSalon).CurrentValues.SetValues(salon);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Salon updated successfully!";
            return RedirectToAction(nameof(Index));
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(salon);
        }
    }

    [Route("Delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
        try {
            var salon = await context.Salons.FindAsync(id);
            if (salon == null) return NotFound();
            return View(salon);
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Delete")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        try {
            var salon = await context.Salons
                .Include(s => s.Services)
                .Include(s => s.Employees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salon == null) return NotFound();

            if (salon.Services.Count > 0 && salon.Employees.Count > 0) {
                TempData["InfoMessage"] = "Cannot delete salon because it has associated services and employees.";
                return RedirectToAction(nameof(Index));
            } 
            if (salon.Services.Count > 0) {
                TempData["InfoMessage"] = "Cannot delete salon because it has associated services.";
                return RedirectToAction(nameof(Index));
            }
            if (salon.Employees.Count > 0) {
                TempData["InfoMessage"] = "Cannot delete salon because it has associated employees.";
                return RedirectToAction(nameof(Index));
            }

            context.Salons.Remove(salon);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Salon has been deleted successfully!";
            return RedirectToAction(nameof(Index));
        } catch {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }
}
