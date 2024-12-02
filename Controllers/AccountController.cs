using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;
public class AccountController : Controller {
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger) {
        _logger = logger;
    }

    public IActionResult Index() {
        return RedirectToAction("Login");
    }

    public IActionResult Login() {
        return View();
    }

    public IActionResult Register() {
        return View();
    }

}
