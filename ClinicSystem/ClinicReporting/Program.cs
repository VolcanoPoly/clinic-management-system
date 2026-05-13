// ClinicReporting — Reporting Application
// ⚠ IMPORTANT: This project has NO reference to ClinicAPI and NO direct database access.
//              All data is retrieved exclusively through HTTP calls to the Web API.

var builder = WebApplication.CreateBuilder(args);
// Ensure the app binds to the ports defined in launchSettings.json to avoid
// falling back to the default port (5000) which can cause AddressInUse errors
// when multiple apps are run simultaneously.
builder.WebHost.UseUrls("http://localhost:5053", "https://localhost:7298");

// ── HttpClient — ALL data access goes through the ClinicAPI ─────────────────
builder.Services.AddHttpClient("ClinicApiClient", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
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

// ── MVC with Views ──────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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