/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 3 - Authentication & Identity / Stage 6 - Appointment Lifecycle
 * Description : Receptionist portal: dashboard with today's stats and patient search.
 */
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers
{
    [Authorize(Roles = "Receptionist")]
    public class ReceptionistController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReceptionistController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var todayAppts = await _db.Appointments
                .Where(a => a.AppointmentDateTime.Date == today)
                .ToListAsync();

            ViewBag.TodayTotal  = todayAppts.Count;
            ViewBag.Pending     = todayAppts.Count(a => a.Status == AppointmentStatus.Requested);
            ViewBag.CheckedIn   = todayAppts.Count(a =>
                a.Status == AppointmentStatus.CheckedIn ||
                a.Status == AppointmentStatus.InProgress);
            ViewBag.Completed   = todayAppts.Count(a => a.Status == AppointmentStatus.Completed);

            return View();
        }

        public async Task<IActionResult> PatientSearch(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new List<PatientSearchResultViewModel>());

            var q = query.Trim().ToLower();

            var patients = await _db.Patients
                .Include(p => p.User)
                .Where(p =>
                    p.CPRNumber.ToLower().Contains(q) ||
                    (p.User != null && (
                        p.User.FirstName.ToLower().Contains(q) ||
                        p.User.LastName.ToLower().Contains(q) ||
                        (p.User.FirstName.ToLower() + " " + p.User.LastName.ToLower()).Contains(q)
                    ))
                )
                .OrderBy(p => p.User!.LastName)
                .Take(20)
                .ToListAsync();

            var results = patients.Select(p => new PatientSearchResultViewModel
            {
                PatientId   = p.Id,
                FullName    = $"{p.User?.FirstName} {p.User?.LastName}",
                CPRNumber   = p.CPRNumber,
                DateOfBirth = p.DateOfBirth,
                BloodType   = p.BloodType,
                Email       = p.User?.Email ?? ""
            }).ToList();

            ViewBag.Query = query;
            return View(results);
        }
    }
}
