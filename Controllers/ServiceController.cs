using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Authorize(Roles = "Admin")]
[Route("/panel/[controller]")]
public class ServiceController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index() {
        try {
            var services = await _context.Services
                .Include(s => s.Salon)
                .ToListAsync();
            return View(services);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Create")]
    [HttpGet]
    public IActionResult Create() {
        ViewBag.Salons = new SelectList(_context.Salons, "Id", "Name");
        return View(new Service());
    }

    [Route("Create")]
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,Price,Duration,SalonId")] Service service) {
        if (ModelState.IsValid) {
            try {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service created successfully!";
                return RedirectToAction(nameof(Index));
            } catch (Exception) {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the service.");
            }
        }
        ViewBag.Salons = new SelectList(_context.Salons, "Id", "Name", service.SalonId);
        return View(service);
    }

    [Route("Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
        try {
            var service = await _context.Services
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            ViewBag.Salons = new SelectList(_context.Salons, "Id", "Name", service.SalonId);
            return View(service);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(Service service) {
        if (!ModelState.IsValid) {
            ViewBag.Salons = new SelectList(_context.Salons, "Id", "Name", service.SalonId);
            return View(service);
        }

        try {
            var existingService = await _context.Services.FindAsync(service.Id);
            if (existingService == null) return NotFound();

            if (existingService.Name == service.Name &&
                existingService.Price == service.Price &&
                existingService.Duration == service.Duration &&
                existingService.SalonId == service.SalonId) {
                TempData["InfoMessage"] = "No changes were made to the service.";
                return RedirectToAction(nameof(Index));
            }

            _context.Entry(existingService).CurrentValues.SetValues(service);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service updated successfully!";
            return RedirectToAction(nameof(Index));
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            ViewBag.Salons = new SelectList(_context.Salons, "Id", "Name", service.SalonId);
            return View(service);
        }
    }

    [Route("Delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
        try {
            var service = await _context.Services
                .Include(s => s.Salon)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            return View(service);
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }

    [Route("Delete")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        try {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service has been deleted successfully!";
            return RedirectToAction(nameof(Index));
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View();
        }
    }
}
