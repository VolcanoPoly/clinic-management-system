using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Services;

var builder = WebApplication.CreateBuilder(args);
// Explicitly bind to the ports to avoid conflicts with port 5000
builder.WebHost.UseUrls("http://localhost:5298", "https://localhost:7268");

// ── Database & EF Core ──────────────────────────────────────────────────────
// Uses the shared ApplicationDbContext from the ClinicAPI project reference.
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
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
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

app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();