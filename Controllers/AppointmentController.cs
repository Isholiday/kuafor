using System.Security.Claims;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Authorize]
[Route("/Dashboard/[controller]")]
public class AppointmentController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index() {
        var appointments = await _context.Appointments
            .Include(a => a.Employee)
            .Include(a => a.Service)
            .Where(a => a.UserId == GetUserId())
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }

    [Route("Create")]
    [HttpGet]
    public IActionResult Create() {
        try {
            ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name");
            return View(new Appointment());
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(new Appointment());
        }
    }

    [Route("Create")]
    [HttpPost]
    public async Task<IActionResult> Create([Bind("SalonId,EmployeeId,ServiceId,AppointmentDate")] Appointment appointment) {
        if (!ModelState.IsValid) {
            ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", appointment.SalonId);
            return View(appointment);
        }

        try {
            var employee = await _context.Employees
                .Include(e => e.Availabilities)
                .FirstOrDefaultAsync(e => e.Id == appointment.EmployeeId);

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (employee == null || service == null) {
                ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", appointment.SalonId);
                ModelState.AddModelError(string.Empty, "Selected employee or service not found.");
                return View(appointment);
            }

            var appointmentDay = appointment.AppointmentDate.DayOfWeek;
            var appointmentTime = appointment.AppointmentDate.TimeOfDay;
            var appointmentEndTime = appointmentTime.Add(service.Duration);

            var availability = employee.Availabilities
                .FirstOrDefault(a => a.Day == appointmentDay);

            if (availability == null) {
                ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", appointment.SalonId);
                ModelState.AddModelError(string.Empty, "Employee is not available on this day.");
                return View(appointment);
            }

            var existingAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.EmployeeId == appointment.EmployeeId
                    && a.AppointmentDate.Date == appointment.AppointmentDate.Date)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            bool isValidTimeSlot = true;
            foreach (var existing in existingAppointments) {
                var existingStart = existing.AppointmentDate.TimeOfDay;
                var existingEnd = existingStart.Add(existing.Service!.Duration);

                if ((appointmentTime >= existingStart && appointmentTime < existingEnd) ||
                    (appointmentEndTime > existingStart && appointmentEndTime <= existingEnd) ||
                    (appointmentTime <= existingStart && appointmentEndTime >= existingEnd)) {
                    isValidTimeSlot = false;
                    break;
                }
            }

            if (!isValidTimeSlot) {
                ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", appointment.SalonId);
                ModelState.AddModelError(string.Empty, "This time slot conflicts with another appointment.");
                return View(appointment);
            }

            if (appointmentTime >= availability.StartTime && appointmentEndTime <= availability.EndTime) {
                appointment.UserId = GetUserId();
                appointment.IsConfirmed = false;
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your appointment has been scheduled successfully!";
                return RedirectToAction("UserDashboard", "Dashboard");
            } else {
                ViewData["Salons"] = new SelectList(_context.Salons, "Id", "Name", appointment.SalonId);
                ModelState.AddModelError(string.Empty,
                    $"Appointment time {appointment.AppointmentDate.ToString("HH:mm")} " +
                    $"with duration of {service.Duration.TotalMinutes} minutes " +
                    $"must be within employee's availability hours ({availability.StartTime.ToString(@"hh\:mm")} - {availability.EndTime.ToString(@"hh\:mm")}).");
                return View(appointment);
            }
        } catch (Exception) {
            ModelState.AddModelError(string.Empty, "An error occurred while scheduling the appointment.");
            return View(appointment);
        }
    }

    [HttpGet("GetSalonData")]
    public async Task<IActionResult> GetSalonData(int salonId) {
        var employees = await _context.Employees
            .Where(e => e.SalonId == salonId)
            .Select(e => new { e.Id, e.Name })
            .ToListAsync();

        var services = await _context.Services
            .Where(s => s.SalonId == salonId)
            .Select(s => new { s.Id, s.Name, Duration = s.Duration.TotalMinutes })
            .ToListAsync();

        return Json(new { employees, services });
    }

    [HttpGet("GetAvailabilityTable")]
    public async Task<IActionResult> GetAvailabilityTable(int salonId) {
        var employees = await _context.Employees
            .Include(e => e.Availabilities)
            .Where(e => e.SalonId == salonId)
            .ToListAsync();

        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.AppointmentDate.Date >= DateTime.Today)
            .ToListAsync();

        ViewData["Appointments"] = appointments;
        return PartialView("_AvailabilityTable", employees);
    }

    [HttpGet("PendingRequests")]
    public async Task<IActionResult> PendingRequests(int employeeId) {
        var pendingAppointments = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Service)
            .Where(a => a.EmployeeId == employeeId && !a.IsConfirmed)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return View(pendingAppointments);
    }

    [HttpGet("EmployeeSchedule")]
    public async Task<IActionResult> EmployeeSchedule(int employeeId) {
        var confirmedAppointments = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Service)
            .Where(a => a.EmployeeId == employeeId && a.IsConfirmed)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return View(confirmedAppointments);
    }

    [HttpPost("ConfirmAppointment")]
    public async Task<IActionResult> ConfirmAppointment(int appointmentId) {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment != null) {
            appointment.IsConfirmed = true;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment confirmed successfully!";
        }
        return RedirectToAction("PendingRequests", new { employeeId = appointment!.EmployeeId });
    }

    [Route("Delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
        var userId = GetUserId();
        var appointment = await _context.Appointments
            .Include(a => a.Employee)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (appointment == null)
            return NotFound();

        if (appointment.IsConfirmed) {
            TempData["InfoMessage"] = "Confirmed appointments cannot be cancelled.";
            return RedirectToAction(nameof(Index));
        }

        return View(appointment);
    }

    [Route("Delete")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        var userId = GetUserId();
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (appointment == null)
            return NotFound();

        if (appointment.IsConfirmed) {
            TempData["InfoMessage"] = "Confirmed appointments cannot be cancelled.";
            return RedirectToAction(nameof(Index));
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Appointment cancelled successfully.";
        return RedirectToAction(nameof(Index));
    }

    private int GetUserId() {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}


