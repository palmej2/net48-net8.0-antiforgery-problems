using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
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
// Configure DataProtection to match .NET Framework's machineKey settings
builder.Services.AddDataProtection()
    .SetApplicationName("YourAppName") // This should be consistent across all apps using the same keys
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Custom")); // Ensure a consistent location for keys

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Bearer";
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Ensure it's set correctly
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme) // Add cookie-based persistence

    //.AddJwtBearer(options =>
    //{
    //    // This could be a symmetric key or a key from an external source (e.g., Identity Server)
    //    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    //    {
    //        // The key to validate the token signature
    //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), // Use your secret key
    //        ValidateIssuer = false, // You can validate the issuer if needed
    //        ValidateAudience = false, // You can validate the audience if needed
    //        ValidateLifetime = true, // Validate token expiration
    //        ClockSkew = TimeSpan.Zero // Optionally adjust the clock skew for token expiration tolerance
    //    };
    //    options.Events = new JwtBearerEvents
    //    {
    //        OnTokenValidated = context =>
    //        {
    //            // Custom validation logic
    //            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
    //            if (claimsIdentity != null)
    //            {
    //                // Example: Check for a specific claim
    //                var customClaim = claimsIdentity.FindFirst("CustomClaim");
    //                if (customClaim == null || customClaim.Value != "CustomValue")
    //                {
    //                    context.Fail("Unauthorized");
    //                }
    //            }

    //            return Task.CompletedTask;
    //        },
    //        OnAuthenticationFailed = context =>
    //        {
    //            // Handle authentication failures
    //            context.Response.StatusCode = 401;
    //            context.Response.ContentType = "application/json";
    //            var result = JsonSerializer.Serialize(new { message = "Authentication failed" });
    //            return context.Response.WriteAsync(result);
    //        }
    //    };
    //})
    .AddOAuth("Bearer", options =>
    {
        options.AuthorizationEndpoint = "/signin-oauth";
        options.ClientId = "blank";
        options.ClientSecret = "blank";
        options.TokenEndpoint = "/token1";
        options.CallbackPath = "/CallbackPath";
        
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var accessToken = context.AccessToken;
                // Custom logic to handle the OAuth ticket
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "John Doe"),
                    new Claim(ClaimTypes.Email, "johndoe@example.com"),
                    new Claim("CustomClaim", "CustomValue") // Example of custom claim
                };

                var identity = new ClaimsIdentity(claims, context.Scheme.Name);
                context.Principal = new ClaimsPrincipal(identity);

                await Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                context.Response.Redirect("/error?failureMessage=" + context.Failure.Message);
                context.HandleResponse();
                return Task.CompletedTask;
            }
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

app.MapPost("/token1", async (HttpContext context) =>
{
    var clientId = context.Request.Form["client_id"];
    var clientSecret = context.Request.Form["secret"];

    // Validate clientId and clientSecret
    if (clientId != "" && clientSecret != "")
    {
        var identity = new ClaimsIdentity("OAuth");
        identity.AddClaim(new Claim("FirstName", "888"));
        identity.AddClaim(new Claim("LastName", "Doe"));
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties();

        AuthenticationTicket authenticationTicket = new AuthenticationTicket(principal, properties, "Bearer");
        var token = Convert.ToBase64String(TicketSerializer.Default.Serialize(authenticationTicket));

        return Results.Ok(new { token = token });
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.MapGet("CallbackPath", () => "OK");
app.MapGet("signin-oauth",  (context) =>
{
    return Task.CompletedTask;
});
app.MapGet("api/foo", () => "OK").RequireAuthorization(policy => policy.RequireClaim("CustomClaim", "CustomValue"));
app.MapGet("api/foo1", () => "OK").RequireAuthorization(policy => policy.RequireClaim(ClaimTypes.Email, "johndoe@example.com"));


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
