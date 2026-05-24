using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Models.ViewModels;
using ClinicMVC.Services;

namespace ClinicMVC.Controllers
{
    [Authorize(Roles = "ClinicManager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notifications;

        public ManagerController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            INotificationService notifications)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _notifications = notifications;
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
            var viewModel = await BuildScheduleViewModelAsync(doctorId);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SchedulesEdit(int doctorId, List<DayScheduleViewModel> daySchedules)
        {
            if (daySchedules == null || !daySchedules.Any())
            {
                ModelState.AddModelError(string.Empty, "Schedule data is missing.");
                var emptyVm = await BuildScheduleViewModelAsync(doctorId);
                return emptyVm == null ? NotFound() : View("SchedulesIndex", emptyVm);
            }

            var parsedSchedules = new List<(DayScheduleViewModel Day, TimeSpan Start, TimeSpan End)>();
            foreach (var daySchedule in daySchedules)
            {
                if (!TimeSpan.TryParse(daySchedule.StartTimeString, out var startTime) ||
                    !TimeSpan.TryParse(daySchedule.EndTimeString, out var endTime))
                {
                    ModelState.AddModelError(string.Empty,
                        $"{daySchedule.DayName}: enter valid start and end times.");
                    continue;
                }

                if (endTime <= startTime)
                {
                    ModelState.AddModelError(string.Empty,
                        $"{daySchedule.DayName}: end time must be after start time.");
                    continue;
                }

                parsedSchedules.Add((daySchedule, startTime, endTime));
            }

            if (!ModelState.IsValid)
            {
                var invalidVm = await BuildScheduleViewModelAsync(doctorId);
                if (invalidVm == null)
                    return NotFound();

                foreach (var submitted in daySchedules)
                {
                    var match = invalidVm.DaySchedules.FirstOrDefault(d => d.Id == submitted.Id);
                    if (match != null)
                    {
                        match.StartTimeString = submitted.StartTimeString;
                        match.EndTimeString = submitted.EndTimeString;
                    }
                }

                return View("SchedulesIndex", invalidVm);
            }

            try
            {
                foreach (var (daySchedule, startTime, endTime) in parsedSchedules)
                {
                    if (!daySchedule.Id.HasValue)
                        continue;

                    var schedule = await _dbContext.DoctorSchedules
                        .FirstOrDefaultAsync(s => s.Id == daySchedule.Id && s.DoctorId == doctorId);

                    if (schedule != null)
                    {
                        schedule.StartTime = startTime;
                        schedule.EndTime = endTime;
                        _dbContext.Update(schedule);
                    }
                }

                await _dbContext.SaveChangesAsync();

                // Notify patients with upcoming appointments for this doctor
                var activeStatuses = new[] { AppointmentStatus.Requested, AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn };
                var affectedAppointments = await _dbContext.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .Where(a => a.DoctorId == doctorId
                        && activeStatuses.Contains(a.Status)
                        && a.AppointmentDateTime >= DateTime.Now)
                    .ToListAsync();

                foreach (var appt in affectedAppointments)
                {
                    if (appt.Patient?.User?.Id != null)
                        await _notifications.SendNotificationAsync(
                            appt.Patient.User.Id,
                            $"The schedule for your doctor has been updated. Please check your appointment on {appt.AppointmentDateTime:dd MMM yyyy} is still valid.",
                            appt.Id);
                }

                return RedirectToAction(nameof(SchedulesIndex), new { doctorId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                var errorVm = await BuildScheduleViewModelAsync(doctorId);
                return errorVm == null ? NotFound() : View("SchedulesIndex", errorVm);
            }
        }

        public async Task<IActionResult> SchedulesLeave(int doctorId)
        {
            var viewModel = await BuildLeaveViewModelAsync(doctorId);
            return viewModel == null ? NotFound() : View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SchedulesLeaveCreate(int doctorId, DoctorLeaveCreateViewModel leaveForm)
        {
            leaveForm.DoctorId = doctorId;

            if (!ModelState.IsValid)
                return await SchedulesLeaveViewResultAsync(doctorId, leaveForm);

            try
            {
                if (!DateTime.TryParse(leaveForm.StartDateString, out var startDate) ||
                    !DateTime.TryParse(leaveForm.EndDateString, out var endDate))
                {
                    ModelState.AddModelError(string.Empty, "Enter valid start and end dates.");
                    return await SchedulesLeaveViewResultAsync(doctorId, leaveForm);
                }

                if (startDate > endDate)
                {
                    ModelState.AddModelError(string.Empty, "End date must be on or after the start date.");
                    return await SchedulesLeaveViewResultAsync(doctorId, leaveForm);
                }

                var leave = new DoctorLeave
                {
                    DoctorId = doctorId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Reason = leaveForm.Reason ?? ""
                };

                _dbContext.DoctorLeaves.Add(leave);
                await _dbContext.SaveChangesAsync();

                // Notify patients whose appointments fall within the leave period
                var activeStatuses = new[] { AppointmentStatus.Requested, AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn };
                var affectedAppointments = await _dbContext.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .Where(a => a.DoctorId == doctorId
                        && activeStatuses.Contains(a.Status)
                        && a.AppointmentDateTime.Date >= startDate.Date
                        && a.AppointmentDateTime.Date <= endDate.Date)
                    .ToListAsync();

                foreach (var appt in affectedAppointments)
                {
                    if (appt.Patient?.User?.Id != null)
                        await _notifications.SendNotificationAsync(
                            appt.Patient.User.Id,
                            $"Your doctor is on leave from {startDate:dd MMM yyyy} to {endDate:dd MMM yyyy}. Your appointment on {appt.AppointmentDateTime:dd MMM yyyy} may be affected. Please contact the clinic.",
                            appt.Id);
                }

                return RedirectToAction(nameof(SchedulesLeave), new { doctorId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return await SchedulesLeaveViewResultAsync(doctorId, leaveForm);
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
            {
                TempData["Error"] = string.Join(" ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Specializations));
            }

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
            {
                TempData["Error"] = string.Join(" ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Specializations));
            }

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

        private async Task<ScheduleViewModel?> BuildScheduleViewModelAsync(int doctorId)
        {
            var doctor = await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return null;

            var schedules = doctor.Schedules.OrderBy(s => s.DayOfWeek).ToList();

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

            return new ScheduleViewModel
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
        }

        private async Task<LeaveViewModel?> BuildLeaveViewModelAsync(int doctorId, DoctorLeaveCreateViewModel? leaveForm = null)
        {
            var doctor = await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return null;

            return new LeaveViewModel
            {
                DoctorId = doctorId,
                DoctorName = $"{doctor.User?.FirstName} {doctor.User?.LastName}",
                LeaveForm = leaveForm ?? new DoctorLeaveCreateViewModel { DoctorId = doctorId },
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
        }

        private async Task<IActionResult> SchedulesLeaveViewResultAsync(int doctorId, DoctorLeaveCreateViewModel leaveForm)
        {
            var viewModel = await BuildLeaveViewModelAsync(doctorId, leaveForm);
            return viewModel == null ? NotFound() : View("SchedulesLeave", viewModel);
        }

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
