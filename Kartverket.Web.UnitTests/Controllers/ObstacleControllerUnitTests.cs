using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Kartverket.Web.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class ObstacleControllerUnitTests
    {
        private DataContext dataContext = null!;
        private IObstacleRepository obstacleRepository = null!;

        [Fact]
        public async Task DataFormReturnsCorrectView()
        {
            SetupDatabase();
            obstacleRepository = new FakeObstacleRepository();
            var controller = new ObstacleController(dataContext, obstacleRepository);
            controller.ModelState.AddModelError("ObstacleName", "Required");
            var actionResult = await controller.DataForm();
            var viewResult = Assert.IsType<ViewResult>(actionResult);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void DataForm_Post_ValidModel_SavesToDatabase()
        {
            SetupDatabase();
            obstacleRepository = new FakeObstacleRepository();
            var controller = new ObstacleController(dataContext, obstacleRepository);
            var obstacleData = new Kartverket.Web.Models.ObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 10,
                ObstacleDescription = "This is a test obstacle."
            };
            var vr = Assert.IsType<ViewResult>(controller.DataForm(obstacleData));
            var savedObstacle = dataContext.ObstacleDatas.First();
            Assert.Equal("This is a test obstacle.", savedObstacle.ObstacleDescription);
        }

        private void SetupDatabase()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;
            dataContext = new DataContext(options);
        }

        [Fact]
        public void DataForm_Post_DraftModel_DoesNotSaveToDatabase()
        {
            SetupDatabase();
            obstacleRepository = new FakeObstacleRepository();
            var controller = new ObstacleController(dataContext, obstacleRepository);
            var obstacleData = new Kartverket.Web.Models.ObstacleData();
            var vr = Assert.IsType<ViewResult>(controller.DataForm(obstacleData));
            var savedObstacle = dataContext.ObstacleDatas.FirstOrDefault();
            Assert.Null(savedObstacle);
        }

        private class FakeObstacleRepository : IObstacleRepository
        {
            public Task<IEnumerable<ObstacleData>> GetAllObstacleData()
            {
                IEnumerable<ObstacleData> list = Array.Empty<ObstacleData>();
                return Task.FromResult(list);
            }
            public Task InsertObstacleData(ObstacleData data)
            {
                return Task.CompletedTask;
            }
        }
    }
}
