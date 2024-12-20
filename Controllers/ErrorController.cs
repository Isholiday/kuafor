using System.Diagnostics;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorController : Controller {

    public IActionResult Index() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult NotFound(int statusCode) {
        if (statusCode == 404) return View("NotFound");
        return View();
    }
}
