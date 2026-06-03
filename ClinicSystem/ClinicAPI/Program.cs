using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    // Local-only ports. Azure App Service provides its own port binding in production.
    builder.WebHost.UseUrls("http://localhost:5235", "https://localhost:7053");
}

// ── Database & EF Core ──────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── ASP.NET Core Identity ───────────────────────────────────────────────────
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

// Configure cookie name for the API (distinct from MVC) to avoid collisions
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".ClinicAPI.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ── JWT Authentication ──────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

    // Allow SignalR hub connections to pass JWT via query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ── SignalR ─────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── CORS — allow MVC app and Reporting app to reach this API ────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClinicClients", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["AllowedOrigins:MvcApp"] ?? "https://localhost:7268",
                builder.Configuration["AllowedOrigins:ReportingApp"] ?? "https://localhost:7298"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR WebSocket handshake
    });
});

// ── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clinic Management API",
        Version = "v1",
        Description = "ASP.NET Core Web API for the Healthcare Clinic system (IT8118)"
    });

    // Enable JWT Bearer auth in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Enter: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowClinicClients");

// Remove malformed or non-base64 auth cookies before the authentication middleware runs
app.Use(async (context, next) =>
{
    // Consider both API and MVC cookie names
    string[] cookieNames = new[] { ".ClinicAPI.Identity", ".ClinicMVC.Identity", ".AspNetCore.Identity.Application" };
    foreach (var cookieName in cookieNames)
    {
        if (context.Request.Cookies.TryGetValue(cookieName, out var cookieValue))
        {
            bool IsLikelyBase64(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return false;
                s = s.Trim();
                // Accept base64 URL-safe alphabet (used by ASP.NET Core data protection) and optional padding '='
                return Regex.IsMatch(s, "^[A-Za-z0-9_-]+={0,2}$");
            }

            if (!IsLikelyBase64(cookieValue))
            {
                context.Response.Cookies.Delete(cookieName);
            }
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AppointmentHub>("/hubs/appointment");

// ── Seed development data on startup ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
