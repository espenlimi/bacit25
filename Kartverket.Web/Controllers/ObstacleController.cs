using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;

namespace Kartverket.Web.Controllers;

public class ObstacleController : Controller
{
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
}
