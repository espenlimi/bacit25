using Kartverket.Web.Controllers;
using Kartverket.Web.Data;
using Kartverket.Web.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Kartverket.Web.UnitTests.Controllers
{
    public class ObstacleControllerUnitTests
    {
        private DataContext dataContext = null!;
        private IObstacleRepository obstacleRepository = null!;
        private UserManager<IdentityUser> userManager = null!; // mocked minimal

        [Fact]
        public async Task DataFormReturnsCorrectView()
        {
            SetupDatabase();
            obstacleRepository = new FakeObstacleRepository();
            userManager = TestUserManager();
            var controller = new ObstacleController(dataContext, obstacleRepository, userManager);
            AttachUser(controller, isAdmin: true);
            var actionResult = await controller.DataForm();
            var viewResult = Assert.IsType<ViewResult>(actionResult);
            Assert.Null(viewResult.ViewName); // default view
        }

        [Fact]
        public void DataForm_Post_ValidModel_SavesToDatabase_WhenUserInAdminRole()
        {
            SetupDatabase();
            obstacleRepository = new FakeObstacleRepository();
            userManager = TestUserManager();
            var controller = new ObstacleController(dataContext, obstacleRepository, userManager);
            AttachUser(controller, isAdmin: true);
            var obstacleData = new Kartverket.Web.Models.ObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 10,
                ObstacleDescription = "This is a test obstacle."
            };
            var vr = Assert.IsType<ViewResult>(controller.DataForm(obstacleData));
            var savedObstacle = dataContext.ObstacleDatas.Single();
            Assert.Equal("This is a test obstacle.", savedObstacle.ObstacleDescription);
        }

        [Fact]
        public void DataForm_Post_DraftModel_DoesNotSaveToDatabase()
        {
            SetupDatabase();
            obstacleRepository = new FakeObstacleRepository();
            userManager = TestUserManager();
            var controller = new ObstacleController(dataContext, obstacleRepository, userManager);
            AttachUser(controller, isAdmin: true);
            var obstacleData = new Kartverket.Web.Models.ObstacleData(); // draft (no description)
            var vr = Assert.IsType<ViewResult>(controller.DataForm(obstacleData));
            var savedObstacle = dataContext.ObstacleDatas.FirstOrDefault();
            Assert.Null(savedObstacle);
        }

        private void SetupDatabase()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;
            dataContext = new DataContext(options);
        }

        private static void AttachUser(Controller controller, bool isAdmin)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "unit-test-user")
            };
            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
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

        private UserManager<IdentityUser> TestUserManager()
        {
            var store = new MockUserStore();
            return new UserManager<IdentityUser>(store, null, null, null, null, null, null, null, null);
        }
        private class MockUserStore : IUserPasswordStore<IdentityUser>, IUserRoleStore<IdentityUser>
        {
            public Task AddToRoleAsync(IdentityUser user, string roleName, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
            public Task<IdentityResult> CreateAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
            public Task<IdentityResult> DeleteAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
            public void Dispose() { }
            public Task<IdentityUser?> FindByIdAsync(string userId, System.Threading.CancellationToken cancellationToken) => Task.FromResult<IdentityUser?>(new IdentityUser { Id = userId });
            public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, System.Threading.CancellationToken cancellationToken) => Task.FromResult<IdentityUser?>(new IdentityUser { UserName = normalizedUserName });
            public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.UserName);
            public Task<string?> GetPasswordHashAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult<string?>(null);
            public Task<IList<string>> GetRolesAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
            {
                IList<string> roles = new List<string> { "Admin" }; // always admin for tests
                return Task.FromResult(roles);
            }
            public Task<string?> GetUserIdAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.Id);
            public Task<string?> GetUserNameAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.UserName);
            public Task<bool> HasPasswordAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(false);
            public Task RemoveFromRoleAsync(IdentityUser user, string roleName, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
            public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, System.Threading.CancellationToken cancellationToken) { user.UserName = normalizedName; return Task.CompletedTask; }
            public Task SetPasswordHashAsync(IdentityUser user, string? passwordHash, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
            public Task SetUserNameAsync(IdentityUser user, string? userName, System.Threading.CancellationToken cancellationToken) { user.UserName = userName; return Task.CompletedTask; }
            public Task<IdentityResult> UpdateAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
            public Task<bool> IsInRoleAsync(IdentityUser user, string roleName, System.Threading.CancellationToken cancellationToken) => Task.FromResult(roleName == "Admin");
            public Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName, System.Threading.CancellationToken cancellationToken) => Task.FromResult<IList<IdentityUser>>(new List<IdentityUser>());
        }
    }
}
