using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebNetCoreAuth;

var builder = WebApplication.CreateBuilder(args);
var key = "8c6f9125-df11-41dd-9991-c2be4557214f";
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<TokenService>(sp => new TokenService(
    secretKey: key, // Replace with a real secret key
    issuer: "http://localhost",        // Set your issuer
    audience: "ApiUsers"     // Set your audience
));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        // This could be a symmetric key or a key from an external source (e.g., Identity Server)
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            // The key to validate the token signature
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), // Use your secret key
            ValidateIssuer = false, // You can validate the issuer if needed
            ValidateAudience = false, // You can validate the audience if needed
            ValidateLifetime = true, // Validate token expiration
            ClockSkew = TimeSpan.Zero // Optionally adjust the clock skew for token expiration tolerance
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/token", async (TokenService tokenService, HttpContext httpContext) =>
{
    var clientId = httpContext.Request.Form["client_id"];
    var secret = httpContext.Request.Form["secret"];



    // Here you can add any claim you need
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "John Doe"),
        new Claim(ClaimTypes.Email, "johndoe@example.com"),
        new Claim("CustomClaim", "CustomValue") // Example of custom claim
    };

    var token = tokenService.GenerateToken(claims);
    return Results.Ok(new { token });
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("api/foo", () => "OK").RequireAuthorization(policy => policy.RequireClaim("CustomClaim", "CustomValue"));

app.Run();
