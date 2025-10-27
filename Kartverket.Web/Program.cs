using Kartverket.Web.Data;
using Kartverket.Web.Repositories;
using Kartverket.Web.Repositories.IdentityStores; // added for custom role store
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

app.MapDefaultEndpoints();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}")
 .WithStaticAssets();

app.Run();

void SetupAuthentication(WebApplicationBuilder builder)
{
    builder.Services.Configure<IdentityOptions>(options =>
    {
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
   // .AddRoleStore<KartverketRoleStore>() // Dapper role store, do this for the other stores you need if not using EF 
    .AddEntityFrameworkStores<DataContext>() //EF variant
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

