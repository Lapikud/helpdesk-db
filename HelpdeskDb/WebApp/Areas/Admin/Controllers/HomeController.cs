using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admins,helpdesk_db_admins")]
public class HomeController : Controller
{
    private readonly ILogger<WebApp.Controllers.HomeController> _logger;

    public HomeController(ILogger<WebApp.Controllers.HomeController> logger)
    {
        _logger = logger;
    }
    public IActionResult Index()
    {
        return View();
    }
}