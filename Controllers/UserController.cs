using backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class UserController(ApplicationDbContext context) : Controller {
    private readonly ApplicationDbContext _context = context;
    public IActionResult Index() {
        return View();
    }
}
