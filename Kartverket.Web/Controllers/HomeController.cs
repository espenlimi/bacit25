using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using Kartverket.Web.Data;

namespace Kartverket.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration config;
    private readonly DataContext dataContext;

    public HomeController(ILogger<HomeController> logger, DataContext dataContext, IConfiguration config)

    {
        _logger = logger;
        this.config = config;
        this.dataContext = dataContext;
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

        var stuff = dataContext.TableClasses.ToList();
        if (stuff.Count < 10)
        { 
            dataContext.Add(new TableClass() { Name = "Test" });
            dataContext.SaveChanges();
        }
        
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
