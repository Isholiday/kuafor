using backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class DashboardController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;

    [Route("/Dashboard")]
    public IActionResult UserDashboard() {
        return View();
    }

    public IActionResult AdminDashboard() {
        return View();
    }
}
