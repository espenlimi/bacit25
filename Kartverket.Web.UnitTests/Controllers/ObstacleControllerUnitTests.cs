using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory; // <-- Add this using directive
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class ObstacleControllerUnitTests
    {
        private DataContext dataContext { get; set; }
        [Fact]
        public void DataFormReturnsCorrectView()
        {
            // Arrange
            var controller = new ObstacleController(null);
            controller.ModelState.AddModelError("ObstacleName", "Required");

            // Act
            var result = controller.DataForm() as ViewResult;

            // Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public void DataForm_Post_ValidModel_SavesToDatabase()
        {
            // Arrange
            SetupDatabase();

            var controller = new ObstacleController(dataContext);
            var obstacleData = new Kartverket.Web.Models.ObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 10,
                ObstacleDescription = "This is a test obstacle."
            };

            // Act
            var result = controller.DataForm(obstacleData) as ViewResult;

            // Assert
            var savedObstacle = dataContext.ObstacleDatas.First();
            Assert.Equal("This is a test obstacle.", savedObstacle.ObstacleDescription);
        }

        private  void SetupDatabase()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
            dataContext = new DataContext(options);
            
        }

        [Fact]
        public void DataForm_Post_DraftModel_DoesNotSaveToDatabase()
        {
            // Arrange
            var controller = CreateControllerUnderTest();
            var obstacleData = new Kartverket.Web.Models.ObstacleData();
          
            // Act
            var result = controller.DataForm(obstacleData) as ViewResult;

            // Assert
            var savedObstacle = dataContext.ObstacleDatas.FirstOrDefault();
            Assert.Null(savedObstacle);
        }

        private ObstacleController CreateControllerUnderTest()
        {
            SetupDatabase();

            var controller = new ObstacleController(dataContext);
            return controller;
        }
    }
}
