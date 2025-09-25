
using Kartverket.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class HomeControllerUnitTests
    {
        [Fact]
        public void Index_HasNullViewName()
        {
            // Arrange
            var controller = GetUnitUnderTest();
            // Act
            var result = controller.Index();
            var viewResult = result as ViewResult;
            // Assert
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void GetAThing_IdGreaterThan10_ReturnsEspen()
        {
            // Arrange
            var controller = GetUnitUnderTest();
            // Act
            var result = controller.GetAThing(11);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Models.ThingModel;
            // Assert
            Assert.Equal("Espen", model.Name);
        }

        [Fact]
        public void GetAThing_Id10OrLess_ReturnsRania()
        {
            // Arrange
            var controller = GetUnitUnderTest();
            // Act
            var result = controller.GetAThing(10);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Models.ThingModel;
            // Assert
            Assert.Equal("Rania", model.Name);
        }

        private HomeController GetUnitUnderTest()
        {
            var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<HomeController>>();
            var config = Substitute.For<IConfiguration>();
            
            return new HomeController(logger,config);
        }
    }

}
