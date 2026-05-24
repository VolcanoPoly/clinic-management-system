// ClinicReporting — Reporting Application
// ⚠ IMPORTANT: This project has NO reference to ClinicAPI and NO direct database access.
//              All data is retrieved exclusively through HTTP calls to the Web API.

var builder = WebApplication.CreateBuilder(args);
// Explicitly bind to the ports to avoid conflicts with port 5000
builder.WebHost.UseUrls("http://localhost:5053", "https://localhost:7298");

// ── HttpClient — ALL data access goes through the ClinicAPI ─────────────────
builder.Services.AddHttpClient("ClinicApiClient", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

// ── Session — stores the JWT token after the manager logs in ────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ClinicReporting.Session";
});

// ── Authentication & authorization ─────────────────────────────────────────
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = ".ClinicReporting.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ── MVC with Views ──────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/NotFound");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();       // Must come before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// Default route sends unauthenticated users straight to the login page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();