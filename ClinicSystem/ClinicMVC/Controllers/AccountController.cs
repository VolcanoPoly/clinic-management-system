/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 3 - Authentication & Identity
 * Description : Handles user registration, login, and logout using ASP.NET Core Identity. Redirects authenticated users to their role-specific dashboard upon sign-in.
 */
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ClinicMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ApplicationDbContext context,
                                 ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign Patient role (public registration is for patients only)
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Create a Patient profile record
                    var patient = new Patient
                    {
                        UserId = user.Id,
                        CPRNumber = "PENDING", // To be updated by user
                        ReferenceNumber = "REF-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        DateOfBirth = DateTime.Now.AddYears(-20)
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (TempData["AuthCookieCleared"] != null)
            {
                ViewData["InfoMessage"] = "Your browser had a corrupted authentication cookie which was cleared. Please sign in again.";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("Login POST received for {Email}. ModelState.IsValid={IsValid}", model?.Email, ModelState.IsValid);

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                    _logger.LogInformation("PasswordSignInAsync result for {Email}: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, IsNotAllowed={IsNotAllowed}, RequiresTwoFactor={RequiresTwoFactor}",
                        model.Email,
                        result.Succeeded,
                        result.IsLockedOut,
                        result.IsNotAllowed,
                        result.RequiresTwoFactor);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Account locked out.");
                    }
                    else if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "Login not allowed.");
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        ModelState.AddModelError(string.Empty, "Two-factor authentication required.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                catch (FormatException ex)
                {
                    // Likely caused by a corrupted or non-base64 authentication cookie. Clear auth cookies and redirect to GET login.
                    _logger.LogWarning(ex, "FormatException during PasswordSignInAsync, clearing auth cookies.");

                    await _signInManager.SignOutAsync();
                    Response.Cookies.Delete(".AspNetCore.Identity.Application");
                    Response.Cookies.Delete(".ClinicMVC.Identity");

                    TempData["AuthCookieCleared"] = "1";
                    return RedirectToAction("Login");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
