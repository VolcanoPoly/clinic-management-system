using ClinicReporting.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ClinicReporting.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Reports");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ClinicApiClient");
            var requestBody = JsonSerializer.Serialize(new { model.Email, model.Password });
            var response = await client.PostAsync(
                "api/auth/login",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View(model);
            }

            var content = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResponseModel>(content, _jsonOptions);

            if (loginResult == null || !loginResult.Success || loginResult.User == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to authenticate. Please try again.");
                return View(model);
            }

            if (!loginResult.User.Roles.Contains("ClinicManager"))
            {
                ModelState.AddModelError(string.Empty, "Access denied. Reporting is available to clinic managers only.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResult.User.Id),
                new Claim(ClaimTypes.Name, $"{loginResult.User.FirstName} {loginResult.User.LastName}"),
                new Claim(ClaimTypes.Email, loginResult.User.Email),
                new Claim(ClaimTypes.GivenName, loginResult.User.FirstName),
                new Claim(ClaimTypes.Surname, loginResult.User.LastName),
            };

            foreach (var role in loginResult.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            HttpContext.Session.SetString("ReportingJwtToken", loginResult.Token);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl!);
            }

            return RedirectToAction("Index", "Reports");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            ViewData["AuthPage"] = true;
            return View();
        }
    }
}
