using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Authorize(Roles = "Admin")]
[Route("/panel/[controller]")]
public class SalonController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index() {
        try {
            var salons = await _context.Salons.ToListAsync();
            return View(salons);
        } catch (Exception) {
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
        if (ModelState.IsValid) {
            try {
                _context.Salons.Add(salon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Salon created successfully!";
                return RedirectToAction(nameof(Index));
            } catch (Exception) {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            }
        }
        return View(salon);
    }

    [Route("Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
        try {
            var salon = await _context.Salons.FindAsync(id);
            if (salon == null) return NotFound();
            return View(salon);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(Salon salon) {
        if (!ModelState.IsValid) return View(salon);

        try {
            var existingSalon = await _context.Salons.FindAsync(salon.Id);
            if (existingSalon == null) return NotFound();

            bool hasChanges = existingSalon?.Name?.Equals(salon.Name) == false ||
                            existingSalon?.Address?.Equals(salon.Address) == false ||
                            existingSalon?.PhoneNumber?.Equals(salon.PhoneNumber) == false ||
                            existingSalon?.Email?.Equals(salon.Email) == false ||
                            existingSalon?.OpeningTime != salon.OpeningTime ||
                            existingSalon?.ClosingTime != salon.ClosingTime;

            if (!hasChanges) {
                TempData["InfoMessage"] = "No changes were made to the salon.";
                return RedirectToAction(nameof(Index));
            }

            if (existingSalon != null) _context.Entry(existingSalon).CurrentValues.SetValues(salon);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Salon updated successfully!";
            return RedirectToAction(nameof(Index));
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(salon);
        }
    }

    [Route("Delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
        try {
            var salon = await _context.Salons.FindAsync(id);
            if (salon == null) return NotFound();
            return View(salon);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Delete")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        try {
            var salon = await _context.Salons.FindAsync(id);
            if (salon == null) return NotFound();

            _context.Salons.Remove(salon);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Salon has been deleted successfully!";
            return RedirectToAction(nameof(Index));
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }
}
