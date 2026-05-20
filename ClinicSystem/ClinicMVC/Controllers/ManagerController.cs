using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Models.ViewModels;

namespace ClinicMVC.Controllers
{
    [Authorize(Roles = "ClinicManager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagerController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // ??????????????????????????????????????????????????????????
        // Doctor Management Actions
        // ??????????????????????????????????????????????????????????

        public async Task<IActionResult> DoctorsIndex()
        {
            var doctors = await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Specializations)
                    .ThenInclude(ds => ds.Specialization)
                .ToListAsync();

            var viewModels = doctors.Select(d => new DoctorViewModel
            {
                Id = d.Id,
                FirstName = d.User?.FirstName ?? "",
                LastName = d.User?.LastName ?? "",
                Email = d.User?.Email ?? "",
                LicenseNumber = d.LicenseNumber,
                Bio = d.Bio,
                SpecializationNames = d.Specializations
                    .Select(ds => ds.Specialization?.Name ?? "")
                    .ToList()
            }).ToList();

            return View(viewModels);
        }

        public async Task<IActionResult> DoctorsCreate()
        {
            var specializations = await _dbContext.Specializations.ToListAsync();
            var viewModel = new DoctorFormViewModel
            {
                AvailableSpecializations = specializations
                    .Select(s => new SpecializationOption { Id = s.Id, Name = s.Name })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoctorsCreate(DoctorFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableSpecializations = (await _dbContext.Specializations.ToListAsync())
                    .Select(s => new SpecializationOption { Id = s.Id, Name = s.Name })
                    .ToList();
                return View(model);
            }

            try
            {
                // Create a new ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, "TempPassword@123");
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Failed to create user account.");
                    model.AvailableSpecializations = (await _dbContext.Specializations.ToListAsync())
                        .Select(s => new SpecializationOption { Id = s.Id, Name = s.Name })
                        .ToList();
                    return View(model);
                }

                // Assign Doctor role
                await _userManager.AddToRoleAsync(user, "Doctor");

                // Create Doctor profile
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    LicenseNumber = model.LicenseNumber,
                    Bio = model.Bio
                };

                _dbContext.Doctors.Add(doctor);
                await _dbContext.SaveChangesAsync();

                // Add specializations
                if (model.SelectedSpecializationIds != null && model.SelectedSpecializationIds.Any())
                {
                    foreach (var specId in model.SelectedSpecializationIds)
                    {
                        var doctorSpec = new DoctorSpecialization
                        {
                            DoctorId = doctor.Id,
                            SpecializationId = specId
                        };
                        _dbContext.DoctorSpecializations.Add(doctorSpec);
                    }
                    await _dbContext.SaveChangesAsync();
                }

                // Initialize default schedule (9 AM to 5 PM, Mon-Fri)
                for (int i = 0; i < 5; i++)
                {
                    var schedule = new DoctorSchedule
                    {
                        DoctorId = doctor.Id,
                        DayOfWeek = (DayOfWeek)i,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0)
                    };
                    _dbContext.DoctorSchedules.Add(schedule);
                }
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(DoctorsIndex));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                model.AvailableSpecializations = (await _dbContext.Specializations.ToListAsync())
                    .Select(s => new SpecializationOption { Id = s.Id, Name = s.Name })
                    .ToList();
                return View(model);
            }
        }

        public async Task<IActionResult> DoctorsEdit(int id)
        {
            var doctor = await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Specializations)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            var specializations = await _dbContext.Specializations.ToListAsync();
            var viewModel = new DoctorFormViewModel
            {
                Id = doctor.Id,
                UserId = doctor.UserId,
                FirstName = doctor.User?.FirstName ?? "",
                LastName = doctor.User?.LastName ?? "",
                Email = doctor.User?.Email ?? "",
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                SelectedSpecializationIds = doctor.Specializations
                    .Select(ds => ds.SpecializationId)
                    .ToList(),
                AvailableSpecializations = specializations
                    .Select(s => new SpecializationOption { Id = s.Id, Name = s.Name })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoctorsEdit(int id, DoctorFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.AvailableSpecializations = (await _dbContext.Specializations.ToListAsync())
                    .Select(s => new SpecializationOption { Id = s.Id, Name = s.Name })
                    .ToList();
                return View(model);
            }

            try
            {
                var doctor = await _dbContext.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Specializations)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (doctor == null)
                    return NotFound();

                // Update user info
                if (doctor.User != null)
                {
                    doctor.User.FirstName = model.FirstName;
                    doctor.User.LastName = model.LastName;
                    doctor.User.Email = model.Email;
                    doctor.User.UserName = model.Email;
                }

                // Update doctor info
                doctor.LicenseNumber = model.LicenseNumber;
                doctor.Bio = model.Bio;

                // Update specializations
                var existingSpecIds = doctor.Specializations.Select(ds => ds.SpecializationId).ToList();
                var newSpecIds = model.SelectedSpecializationIds ?? new List<int>();

                // Remove unselected specializations
                var toRemove = existingSpecIds.Except(newSpecIds).ToList();
                foreach (var specId in toRemove)
                {
                    var spec = doctor.Specializations.FirstOrDefault(ds => ds.SpecializationId == specId);
                    if (spec != null)
                        _dbContext.DoctorSpecializations.Remove(spec);
                }

                // Add new specializations
                var toAdd = newSpecIds.Except(existingSpecIds).ToList();
                foreach (var specId in toAdd)
                {
                    doctor.Specializations.Add(new DoctorSpecialization
                    {
                        DoctorId = doctor.Id,
                        SpecializationId = specId
                    });
                }

                _dbContext.Update(doctor);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(DoctorsIndex));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                model.AvailableSpecializations = (await _dbContext.Specializations.ToListAsync())
                    .Select(s => new SpecializationOption { Id = s.Id, Name = s.Name })
                    .ToList();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoctorsDelete(int id)
        {
            var doctor = await _dbContext.Doctors
                .Include(d => d.Specializations)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            try
            {
                // Remove specializations
                _dbContext.DoctorSpecializations.RemoveRange(doctor.Specializations);

                // Remove doctor
                _dbContext.Doctors.Remove(doctor);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(DoctorsIndex));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to delete doctor: {ex.Message}");
                return RedirectToAction(nameof(DoctorsIndex));
            }
        }

        // ??????????????????????????????????????????????????????????
        // Schedule Management Actions
        // ??????????????????????????????????????????????????????????

        public async Task<IActionResult> SchedulesIndex(int doctorId)
        {
            var doctor = await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return NotFound();

            var schedules = doctor.Schedules.OrderBy(s => s.DayOfWeek).ToList();

            // Ensure all weekdays (Mon-Fri) exist
            for (int i = 0; i < 5; i++)
            {
                if (!schedules.Any(s => s.DayOfWeek == (DayOfWeek)i))
                {
                    var newSchedule = new DoctorSchedule
                    {
                        DoctorId = doctorId,
                        DayOfWeek = (DayOfWeek)i,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0)
                    };
                    _dbContext.DoctorSchedules.Add(newSchedule);
                    schedules.Add(newSchedule);
                }
            }
            await _dbContext.SaveChangesAsync();

            schedules = schedules.OrderBy(s => s.DayOfWeek).ToList();

            var viewModel = new ScheduleViewModel
            {
                DoctorId = doctorId,
                DoctorName = $"{doctor.User?.FirstName} {doctor.User?.LastName}",
                DaySchedules = schedules.Select(s => new DayScheduleViewModel
                {
                    Id = s.Id,
                    DoctorId = s.DoctorId,
                    DayOfWeek = s.DayOfWeek,
                    DayName = GetDayName(s.DayOfWeek),
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    StartTimeString = s.StartTime.ToString(@"hh\:mm"),
                    EndTimeString = s.EndTime.ToString(@"hh\:mm")
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SchedulesEdit(int doctorId, List<DayScheduleViewModel> daySchedules)
        {
            if (daySchedules == null || !daySchedules.Any())
                return BadRequest();

            try
            {
                foreach (var daySchedule in daySchedules)
                {
                    if (daySchedule.Id.HasValue)
                    {
                        var schedule = await _dbContext.DoctorSchedules
                            .FirstOrDefaultAsync(s => s.Id == daySchedule.Id && s.DoctorId == doctorId);

                        if (schedule != null)
                        {
                            // Parse times from string format
                            if (TimeSpan.TryParse(daySchedule.StartTimeString, out var startTime) &&
                                TimeSpan.TryParse(daySchedule.EndTimeString, out var endTime))
                            {
                                schedule.StartTime = startTime;
                                schedule.EndTime = endTime;
                                _dbContext.Update(schedule);
                            }
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(SchedulesIndex), new { doctorId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return RedirectToAction(nameof(SchedulesIndex), new { doctorId });
            }
        }

        public async Task<IActionResult> SchedulesLeave(int doctorId)
        {
            var doctor = await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return NotFound();

            var viewModel = new LeaveViewModel
            {
                DoctorId = doctorId,
                DoctorName = $"{doctor.User?.FirstName} {doctor.User?.LastName}",
                Leaves = doctor.Leaves
                    .OrderByDescending(l => l.StartDate)
                    .Select(l => new DoctorLeaveItem
                    {
                        Id = l.Id,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        Reason = l.Reason,
                        StartDateString = l.StartDate.ToString("yyyy-MM-dd"),
                        EndDateString = l.EndDate.ToString("yyyy-MM-dd")
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SchedulesLeaveCreate(int doctorId, string startDateString, string endDateString, string reason)
        {
            if (string.IsNullOrEmpty(startDateString))
                return BadRequest();

            try
            {
                if (!DateTime.TryParse(startDateString, out var startDate) ||
                    !DateTime.TryParse(endDateString, out var endDate))
                {
                    return BadRequest("Invalid date format");
                }

                if (startDate > endDate)
                {
                    ModelState.AddModelError(string.Empty, "End date must be after or equal to start date.");
                    return RedirectToAction(nameof(SchedulesLeave), new { doctorId });
                }

                var leave = new DoctorLeave
                {
                    DoctorId = doctorId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Reason = reason ?? ""
                };

                _dbContext.DoctorLeaves.Add(leave);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(SchedulesLeave), new { doctorId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return RedirectToAction(nameof(SchedulesLeave), new { doctorId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SchedulesLeaveDelete(int doctorId, int leaveId)
        {
            var leave = await _dbContext.DoctorLeaves
                .FirstOrDefaultAsync(l => l.Id == leaveId && l.DoctorId == doctorId);

            if (leave == null)
                return NotFound();

            try
            {
                _dbContext.DoctorLeaves.Remove(leave);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(SchedulesLeave), new { doctorId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to delete leave: {ex.Message}");
                return RedirectToAction(nameof(SchedulesLeave), new { doctorId });
            }
        }

        // ??????????????????????????????????????????????????????????
        // Specialization Management Actions
        // ??????????????????????????????????????????????????????????

        public async Task<IActionResult> Specializations()
        {
            var specializations = await _dbContext.Specializations
                .Include(s => s.Doctors)
                .ToListAsync();

            var viewModels = specializations.Select(s => new SpecializationViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                DoctorCount = s.Doctors.Count
            }).ToList();

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpecializationsCreate(SpecializationViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Specializations));

            var duplicate = await _dbContext.Specializations
                .AnyAsync(s => s.Name.ToLower() == model.Name.ToLower());

            if (duplicate)
            {
                TempData["Error"] = $"A specialization named \"{model.Name}\" already exists.";
                return RedirectToAction(nameof(Specializations));
            }

            _dbContext.Specializations.Add(new ClinicAPI.Models.Specialization
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim() ?? string.Empty
            });
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = $"Specialization \"{model.Name}\" created.";
            return RedirectToAction(nameof(Specializations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpecializationsEdit(SpecializationViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Specializations));

            var spec = await _dbContext.Specializations.FindAsync(model.Id);
            if (spec == null)
                return NotFound();

            var duplicate = await _dbContext.Specializations
                .AnyAsync(s => s.Name.ToLower() == model.Name.ToLower() && s.Id != model.Id);

            if (duplicate)
            {
                TempData["Error"] = $"A specialization named \"{model.Name}\" already exists.";
                return RedirectToAction(nameof(Specializations));
            }

            spec.Name = model.Name.Trim();
            spec.Description = model.Description?.Trim() ?? string.Empty;
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = $"Specialization \"{spec.Name}\" updated.";
            return RedirectToAction(nameof(Specializations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpecializationsDelete(int id)
        {
            var spec = await _dbContext.Specializations
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (spec == null)
                return NotFound();

            if (spec.Doctors.Any())
            {
                TempData["Error"] = $"Cannot delete \"{spec.Name}\" — it is assigned to {spec.Doctors.Count} doctor(s).";
                return RedirectToAction(nameof(Specializations));
            }

            _dbContext.Specializations.Remove(spec);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = $"Specialization \"{spec.Name}\" deleted.";
            return RedirectToAction(nameof(Specializations));
        }

        // ??????????????????????????????????????????????????????????
        // Helper Methods
        // ??????????????????????????????????????????????????????????

        private string GetDayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Sunday => "Sunday",
                DayOfWeek.Monday => "Monday",
                DayOfWeek.Tuesday => "Tuesday",
                DayOfWeek.Wednesday => "Wednesday",
                DayOfWeek.Thursday => "Thursday",
                DayOfWeek.Friday => "Friday",
                DayOfWeek.Saturday => "Saturday",
                _ => ""
            };
        }
    }
}
