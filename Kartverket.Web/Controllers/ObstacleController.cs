using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using Kartverket.Web.Data;

namespace Kartverket.Web.Controllers;

public class ObstacleController : Controller
{
    private readonly DataContext dataContext;

    public ObstacleController(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    // blir kalt etter at vi trykker på "Register Obstacle" lenken i Index viewet
    [HttpGet]
    public ActionResult DataForm()
    {
        return View();
    }


    // blir kalt etter at vi trykker på "Submit Data" knapp i DataForm viewet
    [HttpPost]
    public ActionResult DataForm(Kartverket.Web.Models.ObstacleData obstacledata)
    {
        bool isDraft = false;
        if (obstacledata.ObstacleDescription == null)
        {
            isDraft = true;
        }

        if (!isDraft)
        {
            var dbObstacleModel = new Data.ObstacleData
            {
                ObstacleDescription = obstacledata.ObstacleDescription,
                ObstacleHeight = obstacledata.ObstacleHeight,
                ObstacleName = obstacledata.ObstacleName
            };
            dataContext.ObstacleDatas.Add(dbObstacleModel);

            dataContext.SaveChanges();
        }

        return View("Overview", obstacledata);
    }
}
