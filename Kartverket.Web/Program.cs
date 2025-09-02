var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.AddMySqlDataSource(connectionName: "mysqldb");
var app = builder.Build();

/*
// Dependency Injection (Register services)
// Get connection string directly from configuration in appsetting.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Register your service that uses the connection
builder.Services.AddSingleton(new MySqlConnection(connectionString));
*/


app.MapDefaultEndpoints();

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


app.Run();
