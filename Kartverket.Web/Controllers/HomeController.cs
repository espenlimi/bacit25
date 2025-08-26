using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;

namespace Kartverket.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    // blir kalt etter at vi trykker på "Register Obstacle" lenken i Index viewet
[HttpGet]
public ActionResult DataForm()
{
    return View();
}


// blir kalt etter at vi trykker på "Submit Data" knapp i DataForm viewet
[HttpPost]
public ActionResult DataForm(ObstacleData obstacledata)
{
    return View("Overview", obstacledata);
}


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
