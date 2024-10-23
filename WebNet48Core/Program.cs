using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using WebNet48Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddXmlFile("app.config")
    .AddEnvironmentVariables();

// Configure DataProtection to match .NET Framework's machineKey settings
builder.Services.AddDataProtection()
    .SetApplicationName("YourAppName") // This should be consistent across all apps using the same keys
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Custom")); // Ensure a consistent location for keys

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery( c => {
    c.HeaderName = "CORE-XSRF-HEADER";
    c.Cookie.Name = "CORE-XSRF-COOKIE";
});
// Add YARP to the services
builder.Services.AddHttpForwarder();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
//builder.Services.AddScoped<RedisSessionManager>();


builder.Services.AddMvc();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseMiddleware<CustomSessionMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();
app.MapDefaultControllerRoute();

app.MapGet("antiforgery/tokenValue", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    return Results.Ok(tokens.RequestToken);
});

app.MapReverseProxy();


app.Run();
