using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;

namespace Kartverket.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration config;

    //private readonly string _connectionString;

    public HomeController(ILogger<HomeController> logger, IConfiguration config)
    {
        _logger = logger;
        this.config = config;
    }

/*
// Send avhengighet til konstruktøren
    public HomeController(ILogger<HomeController> logger, IConfiguration config)
    {
        _logger = logger;
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }
*/
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult GetAThing(int id) 
    {
        _logger.LogInformation("GetAThing called with id {Id}", id);
        if (id > 10) 
        {
            return View(new ThingModel { Name = "Espen" });
        }
        return View(new ThingModel { Name = "Rania" });

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
