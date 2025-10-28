using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Web.Controllers;

public class ObstacleController : Controller
{
    private readonly DataContext dataContext;
    private readonly IObstacleRepository obstacleRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public ObstacleController(DataContext dataContext,
                              IObstacleRepository obstacleRepository,
                              UserManager<IdentityUser> userManager  )
    {
        this.dataContext = dataContext;
        this.obstacleRepository = obstacleRepository;
        _userManager = userManager;
    }

    // blir kalt etter at vi trykker på "Register Obstacle" lenken i Index viewet
    [HttpGet]
    [Authorize]
    public async Task<ActionResult> DataForm()
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext.User);
        var roles = await _userManager.GetRolesAsync(currentUser);

        if(roles.Contains("Admin"))
        {
           //do admin stuff
        }
        else
        {
            //No admin stuff
        }

        var data = await obstacleRepository.GetAllObstacleData();

        return View();
    }


    // blir kalt etter at vi trykker på "Submit Data" knapp i DataForm viewet
    [HttpPost]
    [Authorize(Roles ="Admin")]
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
