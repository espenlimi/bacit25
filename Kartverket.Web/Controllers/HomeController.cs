using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using Kartverket.Web.Data;

namespace Kartverket.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext dataContext;

    public HomeController(ILogger<HomeController> logger, DataContext dataContext)
    {
        _logger = logger;
        this.dataContext = dataContext;
    }

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
