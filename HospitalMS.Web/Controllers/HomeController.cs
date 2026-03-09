using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HospitalMS.Web.Models;

namespace HospitalMS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // home landing page
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (User.IsInRole("Patient"))
            {
                return RedirectToAction("Dashboard", "Patient");
            }
        }
        return View();
    }

    // privacy page
    public IActionResult Privacy()
    {
        return View();
    }

    // error page
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}