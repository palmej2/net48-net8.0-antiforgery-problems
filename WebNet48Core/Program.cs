using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();


// Configure DataProtection to match .NET Framework's machineKey settings
builder.Services.AddDataProtection()
    .SetApplicationName("YourAppName") // This should be consistent across all apps using the same keys
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Custom")) // Ensure a consistent location for keys
    .UseCryptographicAlgorithms(new Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ValidationAlgorithm.HMACSHA256
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery( c => {
    c.HeaderName = "XSRF-HEADER";
    c.Cookie.Name = "XSRF-COOKIE";
});
// Add YARP to the services
builder.Services.AddHttpForwarder();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddMvc();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();
app.MapDefaultControllerRoute();

app.MapGet("antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-HEADER", tokens.RequestToken!,
            new CookieOptions { HttpOnly = false });

    return Results.Ok();
});

app.MapReverseProxy();


app.Run();
