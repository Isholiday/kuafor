using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

public class EmployeeController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;
    public IActionResult Index() {
        var employees = _context.Employees.Include(e => e.Availabilities).ToList();
        return View(employees);
    }

}
