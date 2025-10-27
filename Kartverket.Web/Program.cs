using Kartverket.Web.Data;
using Kartverket.Web.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionName = "kartverketdb";
builder.AddMySqlDataSource(connectionName: connectionName);
var connectionString = builder.Configuration.GetConnectionString(connectionName);

builder.Services.AddScoped<IObstacleRepository>(provider =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString(connectionName);
    return new ObstacleRepository(connectionString);
});

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

SetupAuthentication(builder);

var app = builder.Build();

/*
// Dependency Injection (Register services)
// Get connection string directly from configuration in appsetting.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Register your service that uses the connection
builder.Services.AddSingleton(new MySqlConnection(connectionString));
*/
app.MapDefaultEndpoints();
using (var scope = app.Services.CreateScope())
{
    /*
    Migration howto:   
    1. Remember to install dotnet tool install --global dotnet-ef
    2. Run the aspire application
    3. Fetch the connection string (set a break point after getting the ConnectionString value from configuration)
    4. Use the connectionstring value in appsettings.js under connectionstrings
    5. While the application is running, move to the package manager console and run the command "dotnet ef migrations add InitialCreate -c DataContext -o Data/Migrations -v"
        InitialCreate is the name of the migration, and should reflect changes being done, eg AddTableName, AlterTableNameToFixSomeStuff etc.. 
        DataContext refers to an actual classname, so this must be correct for your code
        Data/Migrations sets the location of migrations  
    6. Restart the application, the code below db.Database.Migrate(); should apply the changes to the database
    */

    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();

}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.UseAuthentication();
app.UseAuthorization();

app.Run();

void SetupAuthentication(WebApplicationBuilder builder)
{
    //Setup for Authentication
    builder.Services.Configure<IdentityOptions>(options =>
    {
        // Default Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    });

    builder.Services
        .AddIdentityCore<IdentityUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<DataContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    builder.Services.AddAuthentication(o =>
    {
        o.DefaultScheme = IdentityConstants.ApplicationScheme;
        o.DefaultSignInScheme = IdentityConstants.ExternalScheme;

    }).AddIdentityCookies(o => { });

    builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
}
public class AuthMessageSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine(email);
        Console.WriteLine(subject);
        Console.WriteLine(htmlMessage);
        return Task.CompletedTask;
    }
}