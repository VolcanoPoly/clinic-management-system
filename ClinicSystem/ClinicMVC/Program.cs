using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.Models;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = currentDir
});

// Explicitly bind to the ports to avoid conflicts with port 5000
builder.WebHost.UseUrls("http://localhost:5298", "https://localhost:7268");

// ── Database & EF Core ─────────────────────────────────────────────────────-
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── ASP.NET Core Identity (cookie-based authentication for MVC) ─────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie redirect paths
builder.Services.ConfigureApplicationCookie(options =>
{
    // Use a distinct cookie name to avoid collisions with other apps or JWT usage
    options.Cookie.Name = ".ClinicMVC.Identity";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    // security-friendly defaults
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ── HttpClient — used ONLY for the public appointment lookup page ────────────
// All other features in ClinicMVC use EF Core directly (no HttpClient).
builder.Services.AddHttpClient("ClinicApiClient", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── Application Services ────────────────────────────────────────────────────
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ── MVC with Views ─────────────────────────────────────────────────────────-
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────────────----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/NotFound");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// diagnostic middleware: log incoming cookie names/lengths and response Set-Cookie headers
app.Use(async (context, next) =>
{
    var logger = app.Logger;
    try
    {
        foreach (var c in context.Request.Cookies)
        {
            logger.LogInformation("Incoming cookie '{Name}' length={Length}", c.Key, c.Value?.Length ?? 0);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to read request cookies");
    }

    await next();

    try
    {
        if (context.Response.Headers.TryGetValue("Set-Cookie", out var setCookie))
        {
            logger.LogInformation("Response Set-Cookie header: {SetCookie}", setCookie.ToString());
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to read Set-Cookie response header");
    }
});

// Remove malformed or non-base64 auth cookies before the authentication middleware runs
app.Use(async (context, next) =>
{
    const string cookieName = ".ClinicMVC.Identity";
    if (context.Request.Cookies.TryGetValue(cookieName, out var cookieValue))
    {
        bool IsLikelyBase64(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            // Accept base64 URL-safe alphabet (used by ASP.NET Core data protection) and optional padding '='
            // Characters allowed: A-Z a-z 0-9 - _ and up to two '=' padding chars
            return Regex.IsMatch(s, "^[A-Za-z0-9_-]+={0,2}$");
        }

        if (!IsLikelyBase64(cookieValue))
        {
            // Delete both the new cookie name and the default historical cookie name
            context.Response.Cookies.Delete(cookieName);
            context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
            app.Logger.LogInformation("Deleted malformed auth cookie '{CookieName}'", cookieName);
        }
    }

    await next();
});

app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();