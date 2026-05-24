/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : Full appointment booking wizard (4 steps) and lifecycle management. Enforces role-based status transitions: Requested > Confirmed > CheckedIn > InProgress > Completed/Cancelled/Missed.
 */
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Hubs;
using ClinicMVC.Models.ViewModels;
using ClinicMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AvailabilityService _availability;
        private readonly INotificationService _notifications;
        private readonly IHubContext<AppointmentHub> _appointmentHub;

        public AppointmentController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            AvailabilityService availability,
            INotificationService notifications,
            IHubContext<AppointmentHub> appointmentHub)
        {
            _db = db;
            _userManager = userManager;
            _availability = availability;
            _notifications = notifications;
            _appointmentHub = appointmentHub;
        }

        // ──────────────────────────────────────────────────────────────────────
        // STAGE 5 — Appointment Booking
        // ──────────────────────────────────────────────────────────────────────

        // Step 1: Select specialization (and patient if receptionist)
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpGet]
        public async Task<IActionResult> Book()
        {
            var specs = await _db.Specializations.OrderBy(s => s.Name).ToListAsync();

            var vm = new AppointmentBookViewModel
            {
                Specializations = specs
                    .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                    .ToList()
            };

            if (User.IsInRole("Receptionist"))
            {
                var patientUsers = await _userManager.GetUsersInRoleAsync("Patient");
                var patientIds = patientUsers.Select(u => u.Id).ToList();
                var patients = await _db.Patients
                    .Where(p => patientIds.Contains(p.UserId))
                    .Include(p => p.User)
                    .OrderBy(p => p.User!.LastName)
                    .ToListAsync();

                vm.Patients = patients
                    .Select(p => new SelectListItem(
                        $"{p.User!.FirstName} {p.User.LastName} ({p.CPRNumber})",
                        p.Id.ToString()))
                    .ToList();
            }

            return View(vm);
        }

        // Step 2: Show doctors for the selected specialization
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectDoctor(AppointmentBookViewModel input)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Book));

            if (User.IsInRole("Receptionist") && (input.PatientId == null || input.PatientId == 0))
            {
                TempData["Error"] = "Please select a patient.";
                return RedirectToAction(nameof(Book));
            }

            var spec = await _db.Specializations.FindAsync(input.SpecializationId);
            if (spec == null) return NotFound();

            var doctors = await _db.Doctors
                .Include(d => d.User)
                .Include(d => d.Specializations)
                    .ThenInclude(ds => ds.Specialization)
                .Where(d => d.Specializations.Any(ds => ds.SpecializationId == input.SpecializationId))
                .ToListAsync();

            var doctorOptions = new List<DoctorOption>();
            foreach (var doc in doctors)
            {
                var nextDate = await _availability.GetNextAvailableDateAsync(doc.Id);
                doctorOptions.Add(new DoctorOption
                {
                    Id = doc.Id,
                    FullName = $"Dr. {doc.User?.FirstName} {doc.User?.LastName}",
                    Bio = doc.Bio,
                    SpecializationNames = doc.Specializations
                        .Select(ds => ds.Specialization?.Name ?? "")
                        .ToList(),
                    NextAvailableDate = nextDate
                });
            }

            var vm = new AppointmentSelectDoctorViewModel
            {
                SpecializationId = input.SpecializationId,
                SpecializationName = spec.Name,
                PatientId = input.PatientId,
                Doctors = doctorOptions
            };

            return View(vm);
        }

        // Step 3: Select date and time slot
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectSlot(AppointmentSelectDoctorViewModel input)
        {
            var doctor = await _db.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == input.SelectedDoctorId);

            var spec = await _db.Specializations.FindAsync(input.SpecializationId);

            if (doctor == null || spec == null)
                return NotFound();

            // Default to next available date
            var defaultDate = await _availability.GetNextAvailableDateAsync(doctor.Id)
                              ?? DateTime.Today.AddDays(1);

            var slots = await _availability.GetAvailableSlotsAsync(doctor.Id, defaultDate);

            var vm = new AppointmentSelectSlotViewModel
            {
                SpecializationId = input.SpecializationId,
                SpecializationName = spec.Name,
                DoctorId = doctor.Id,
                DoctorName = $"Dr. {doctor.User?.FirstName} {doctor.User?.LastName}",
                PatientId = input.PatientId,
                Date = defaultDate,
                AvailableSlots = slots
            };

            return View(vm);
        }

        // AJAX: refresh slots when the date changes
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpGet]
        public async Task<IActionResult> GetSlots(int doctorId, DateTime date)
        {
            if (date.Date <= DateTime.Today)
                return Json(new List<string>());

            var slots = await _availability.GetAvailableSlotsAsync(doctorId, date);
            return Json(slots);
        }

        // Step 3 GET: re-render slot selection (used by the Back button on the Confirm page)
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpGet]
        public async Task<IActionResult> SelectSlot(int doctorId, DateTime date, int specializationId, int? patientId, string? notes)
        {
            var doctor = await _db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == doctorId);
            var spec = await _db.Specializations.FindAsync(specializationId);
            if (doctor == null || spec == null) return RedirectToAction(nameof(Book));

            var slots = await _availability.GetAvailableSlotsAsync(doctorId, date);
            var vm = new AppointmentSelectSlotViewModel
            {
                DoctorId = doctorId,
                DoctorName = $"Dr. {doctor.User?.FirstName} {doctor.User?.LastName}",
                Date = date,
                SpecializationId = specializationId,
                SpecializationName = spec.Name,
                PatientId = patientId,
                Notes = notes,
                AvailableSlots = slots
            };
            return View(vm);
        }

        // Step 4 GET: render confirmation page from TempData (PRG pattern — prevents ERR_CACHE_MISS on back/refresh)
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpGet]
        public IActionResult Confirm()
        {
            if (TempData["ConfirmData"] is not string json)
                return RedirectToAction(nameof(Book));

            var vm = System.Text.Json.JsonSerializer.Deserialize<AppointmentConfirmViewModel>(json);
            if (vm == null) return RedirectToAction(nameof(Book));

            // Re-store so the page survives a browser refresh without ERR_CACHE_MISS
            TempData["ConfirmData"] = json;
            return View(vm);
        }

        // Step 4 POST: validate slot, build summary, redirect to GET (PRG)
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(AppointmentSelectSlotViewModel input)
        {
            if (string.IsNullOrEmpty(input.SelectedSlot))
            {
                // Re-load slots and return to slot selection
                input.AvailableSlots = await _availability.GetAvailableSlotsAsync(input.DoctorId, input.Date);
                ModelState.AddModelError("SelectedSlot", "Please select a time slot.");
                var doc2 = await _db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == input.DoctorId);
                input.DoctorName = doc2 != null ? $"Dr. {doc2.User?.FirstName} {doc2.User?.LastName}" : "";
                return View("SelectSlot", input);
            }

            if (input.Date.Date <= DateTime.Today)
            {
                TempData["Error"] = "You cannot book an appointment in the past.";
                return RedirectToAction(nameof(Book));
            }

            var doctor = await _db.Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == input.DoctorId);
            var spec = await _db.Specializations.FindAsync(input.SpecializationId);

            if (doctor == null || spec == null) return NotFound();

            // Parse the slot string "HH:mm"
            if (!TimeSpan.TryParse(input.SelectedSlot, out var timeSpan))
            {
                TempData["Error"] = "Invalid time slot selected.";
                return RedirectToAction(nameof(Book));
            }

            var appointmentDateTime = input.Date.Date.Add(timeSpan);

            // Re-check slot is still available
            var stillAvailable = await _availability.GetAvailableSlotsAsync(input.DoctorId, input.Date);
            if (!stillAvailable.Contains(input.SelectedSlot))
            {
                TempData["Error"] = "That slot was just booked by someone else. Please choose another.";
                input.AvailableSlots = stillAvailable;
                return View("SelectSlot", input);
            }

            string patientName = "";
            if (User.IsInRole("Receptionist") && input.PatientId.HasValue)
            {
                var pat = await _db.Patients.Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == input.PatientId.Value);
                patientName = pat != null ? $"{pat.User?.FirstName} {pat.User?.LastName}" : "";
            }

            var vm = new AppointmentConfirmViewModel
            {
                SpecializationId = input.SpecializationId,
                SpecializationName = spec.Name,
                DoctorId = doctor.Id,
                DoctorName = $"Dr. {doctor.User?.FirstName} {doctor.User?.LastName}",
                PatientId = input.PatientId,
                PatientName = patientName,
                AppointmentDateTime = appointmentDateTime,
                Notes = input.Notes
            };

            // PRG: store in TempData and redirect to GET so back/refresh never triggers ERR_CACHE_MISS
            TempData["ConfirmData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction(nameof(Confirm));
        }

        // Final POST: create the appointment
        [Authorize(Roles = "Patient,Receptionist")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentConfirmViewModel vm)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Determine patient
            int patientId;
            if (User.IsInRole("Receptionist") && vm.PatientId.HasValue)
            {
                patientId = vm.PatientId.Value;
            }
            else
            {
                var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null)
                {
                    TempData["Error"] = "No patient profile found for your account.";
                    return RedirectToAction(nameof(Book));
                }
                patientId = patient.Id;
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = vm.DoctorId,
                SpecializationId = vm.SpecializationId,
                AppointmentDateTime = vm.AppointmentDateTime,
                Status = AppointmentStatus.Requested,
                Notes = vm.Notes ?? string.Empty
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            // Notify the doctor
            var doctor = await _db.Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == vm.DoctorId);

            if (doctor?.UserId != null)
                await _notifications.SendNotificationAsync(
                    doctor.UserId,
                    $"New appointment request from {currentUser.FirstName} {currentUser.LastName} on {vm.AppointmentDateTime:dd MMM yyyy} at {vm.AppointmentDateTime:hh:mm tt}.",
                    appointment.Id);

            TempData["Success"] = "Appointment request submitted successfully.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // ──────────────────────────────────────────────────────────────────────
        // STAGE 6 — Appointment Lifecycle Management
        // ──────────────────────────────────────────────────────────────────────

        // Patient: view own appointments
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == user!.Id);
            if (patient == null) return View(new List<AppointmentListItemViewModel>());

            var appointments = await _db.Appointments
                .Where(a => a.PatientId == patient.Id)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Specialization)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            return View(MapToListItems(appointments));
        }

        // Receptionist: view today's appointments
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> TodaysAppointments()
        {
            var today = DateTime.Today;
            var appointments = await _db.Appointments
                .Where(a => a.AppointmentDateTime.Date == today)
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Specialization)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync();

            return View(MapToListItems(appointments));
        }

        // Doctor: view own upcoming and past appointments
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == user!.Id);
            if (doctor == null) return View(new List<AppointmentListItemViewModel>());

            var appointments = await _db.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Specialization)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            return View(MapToListItems(appointments));
        }

        // Manager: view all appointments with optional filters
        [Authorize(Roles = "ClinicManager")]
        public async Task<IActionResult> AllAppointments(
            DateTime? from, DateTime? to, int? doctorId, AppointmentStatus? status)
        {
            var query = _db.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Specialization)
                .AsQueryable();

            if (from.HasValue)  query = query.Where(a => a.AppointmentDateTime >= from.Value);
            if (to.HasValue)    query = query.Where(a => a.AppointmentDateTime <= to.Value.AddDays(1));
            if (doctorId.HasValue) query = query.Where(a => a.DoctorId == doctorId.Value);
            if (status.HasValue)   query = query.Where(a => a.Status == status.Value);

            var appointments = await query.OrderByDescending(a => a.AppointmentDateTime).ToListAsync();

            ViewBag.Doctors = await _db.Doctors
                .Include(d => d.User)
                .Select(d => new SelectListItem(
                    $"Dr. {d.User!.FirstName} {d.User.LastName}", d.Id.ToString()))
                .ToListAsync();

            ViewBag.Statuses = Enum.GetValues<AppointmentStatus>()
                .Select(s => new SelectListItem(s.ToString(), ((int)s).ToString()))
                .ToList();

            ViewBag.FromDate   = from?.ToString("yyyy-MM-dd");
            ViewBag.ToDate     = to?.ToString("yyyy-MM-dd");
            ViewBag.DoctorId   = doctorId;
            ViewBag.StatusFilter = status;

            return View(MapToListItems(appointments));
        }

        // All roles: appointment detail
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Specialization)
                .Include(a => a.StatusHistory)
                .Include(a => a.VisitRecord)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            // Access control: patient can only see their own; doctor only their own
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Patient"))
            {
                var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == user!.Id);
                if (patient == null || appointment.PatientId != patient.Id) return Forbid();
            }
            else if (User.IsInRole("Doctor"))
            {
                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == user!.Id);
                if (doctor == null || appointment.DoctorId != doctor.Id) return Forbid();
            }

            // Build status history with user names
            var historyVms = new List<StatusHistoryItemViewModel>();
            foreach (var h in appointment.StatusHistory.OrderBy(h => h.ChangedAt))
            {
                var changedBy = await _userManager.FindByIdAsync(h.ChangedByUserId);
                historyVms.Add(new StatusHistoryItemViewModel
                {
                    OldStatus   = h.OldStatus,
                    NewStatus   = h.NewStatus,
                    ChangedAt   = h.ChangedAt,
                    ChangedByName = changedBy != null
                        ? $"{changedBy.FirstName} {changedBy.LastName}"
                        : "System"
                });
            }

            var vm = new AppointmentDetailViewModel
            {
                Id                  = appointment.Id,
                PatientName         = $"{appointment.Patient?.User?.FirstName} {appointment.Patient?.User?.LastName}",
                PatientUserId       = appointment.Patient?.UserId ?? "",
                DoctorName          = $"Dr. {appointment.Doctor?.User?.FirstName} {appointment.Doctor?.User?.LastName}",
                DoctorUserId        = appointment.Doctor?.UserId ?? "",
                SpecializationName  = appointment.Specialization?.Name ?? "",
                AppointmentDateTime = appointment.AppointmentDateTime,
                Status              = appointment.Status,
                Notes               = appointment.Notes,
                CancellationReason  = appointment.CancellationReason,
                StatusHistory       = historyVms,
                AllowedTransitions  = GetAllowedTransitions(appointment.Status, User),
                HasVisitRecord      = appointment.VisitRecord != null,
                VisitRecordId       = appointment.VisitRecord?.Id
            };

            return View(vm);
        }

        // POST: Update appointment status (Receptionist, Doctor)
        [Authorize(Roles = "Receptionist,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus newStatus, string? reason)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Specialization)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var allowed = GetAllowedTransitions(appointment.Status, User);

            if (!allowed.Contains(newStatus))
            {
                TempData["Error"] = $"Transition from {appointment.Status} to {newStatus} is not permitted.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var oldStatus = appointment.Status;
            appointment.Status = newStatus;

            if (!string.IsNullOrWhiteSpace(reason))
                appointment.CancellationReason = reason;

            _db.AppointmentStatusHistories.Add(new AppointmentStatusHistory
            {
                AppointmentId   = appointment.Id,
                OldStatus       = oldStatus,
                NewStatus       = newStatus,
                ChangedByUserId = user!.Id,
                ChangedAt       = DateTime.Now
            });

            // Notify relevant parties
            await SendStatusNotificationAsync(appointment, oldStatus, newStatus, user);

            await _db.SaveChangesAsync();
            await BroadcastWaitingRoomUpdateAsync(appointment);

            TempData["Success"] = $"Appointment status updated to {newStatus}.";

            // If completed, redirect doctor to create visit record
            if (newStatus == AppointmentStatus.Completed && User.IsInRole("Doctor"))
                return RedirectToAction("CreateVisitRecord", "Doctor", new { appointmentId = appointment.Id });

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Patient cancels their own appointment
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == user!.Id);

            var appointment = await _db.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Specialization)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();
            if (patient == null || appointment.PatientId != patient.Id) return Forbid();

            if (appointment.Status != AppointmentStatus.Requested &&
                appointment.Status != AppointmentStatus.Confirmed)
            {
                TempData["Error"] = "Only Requested or Confirmed appointments can be cancelled.";
                return RedirectToAction(nameof(MyAppointments));
            }

            var oldStatus = appointment.Status;
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = reason ?? "Cancelled by patient";

            _db.AppointmentStatusHistories.Add(new AppointmentStatusHistory
            {
                AppointmentId   = appointment.Id,
                OldStatus       = oldStatus,
                NewStatus       = AppointmentStatus.Cancelled,
                ChangedByUserId = user!.Id,
                ChangedAt       = DateTime.Now
            });

            // Notify doctor
            if (appointment.Doctor?.UserId != null)
            {
                _db.Notifications.Add(new Notification
                {
                    RecipientUserId    = appointment.Doctor.UserId,
                    Message            = $"Appointment on {appointment.AppointmentDateTime:dd MMM yyyy HH:mm} was cancelled by the patient.",
                    RelatedAppointmentId = appointment.Id,
                    CreatedAt          = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
            await BroadcastWaitingRoomUpdateAsync(appointment);

            TempData["Success"] = "Your appointment has been cancelled.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private static List<AppointmentListItemViewModel> MapToListItems(List<Appointment> appointments)
        {
            return appointments.Select(a => new AppointmentListItemViewModel
            {
                Id                 = a.Id,
                PatientName        = $"{a.Patient?.User?.FirstName} {a.Patient?.User?.LastName}",
                DoctorName         = $"Dr. {a.Doctor?.User?.FirstName} {a.Doctor?.User?.LastName}",
                SpecializationName = a.Specialization?.Name ?? "",
                AppointmentDateTime = a.AppointmentDateTime,
                Status             = a.Status,
                Notes              = a.Notes
            }).ToList();
        }

        /// <summary>
        /// Returns which statuses the current user may transition THIS appointment to.
        /// </summary>
        private static List<AppointmentStatus> GetAllowedTransitions(
            AppointmentStatus current, System.Security.Claims.ClaimsPrincipal user)
        {
            var transitions = new List<AppointmentStatus>();

            if (user.IsInRole("Receptionist"))
            {
                if (current == AppointmentStatus.Requested)
                    transitions.AddRange(new[] { AppointmentStatus.Confirmed, AppointmentStatus.Cancelled });
                else if (current == AppointmentStatus.Confirmed)
                    transitions.AddRange(new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled });
            }
            else if (user.IsInRole("Doctor"))
            {
                if (current == AppointmentStatus.Requested)
                    transitions.Add(AppointmentStatus.InProgress);
                else if (current == AppointmentStatus.Confirmed)
                    transitions.Add(AppointmentStatus.InProgress);
                else if (current == AppointmentStatus.CheckedIn)
                    transitions.Add(AppointmentStatus.InProgress);
                else if (current == AppointmentStatus.InProgress)
                    transitions.AddRange(new[] { AppointmentStatus.Completed, AppointmentStatus.Missed });
            }

            return transitions;
        }

        private async Task BroadcastWaitingRoomUpdateAsync(Appointment appointment)
        {
            await _appointmentHub.Clients.Group("WaitingRoom").SendAsync("AppointmentStatusChanged", new
            {
                appointment.Id,
                Status = appointment.Status.ToString(),
                appointment.AppointmentDateTime,
                PatientName = $"{appointment.Patient?.User?.FirstName} {appointment.Patient?.User?.LastName}",
                DoctorName = $"Dr. {appointment.Doctor?.User?.FirstName} {appointment.Doctor?.User?.LastName}",
                SpecializationName = appointment.Specialization?.Name ?? string.Empty
            });
        }

        private async Task SendStatusNotificationAsync(
            Appointment appointment,
            AppointmentStatus oldStatus,
            AppointmentStatus newStatus,
            ApplicationUser changedBy)
        {
            var date = appointment.AppointmentDateTime.ToString("dd MMM yyyy");
            var time = appointment.AppointmentDateTime.ToString("hh:mm tt");
            var patientName = $"{appointment.Patient?.User?.FirstName} {appointment.Patient?.User?.LastName}";
            var patientUserId = appointment.Patient?.UserId;
            var doctorUserId  = appointment.Doctor?.UserId;

            switch (newStatus)
            {
                case AppointmentStatus.Confirmed:
                    if (patientUserId != null)
                        await _notifications.SendNotificationAsync(patientUserId,
                            $"Your appointment on {date} at {time} has been confirmed.", appointment.Id);
                    break;

                case AppointmentStatus.CheckedIn:
                    if (doctorUserId != null && doctorUserId != changedBy.Id)
                        await _notifications.SendNotificationAsync(doctorUserId,
                            $"Patient {patientName} has checked in for their {date} appointment.", appointment.Id);
                    break;

                case AppointmentStatus.InProgress:
                    if (patientUserId != null && patientUserId != changedBy.Id)
                        await _notifications.SendNotificationAsync(patientUserId,
                            $"Your appointment on {date} at {time} is now in progress.", appointment.Id);
                    break;

                case AppointmentStatus.Completed:
                    if (patientUserId != null)
                        await _notifications.SendNotificationAsync(patientUserId,
                            $"Your appointment on {date} is complete. Your visit record is now available.", appointment.Id);
                    break;

                case AppointmentStatus.Cancelled:
                    if (patientUserId != null && changedBy.Id != patientUserId)
                        await _notifications.SendNotificationAsync(patientUserId,
                            $"Your appointment on {date} at {time} has been cancelled.", appointment.Id);
                    if (doctorUserId != null && changedBy.Id != doctorUserId)
                        await _notifications.SendNotificationAsync(doctorUserId,
                            $"Appointment with {patientName} on {date} at {time} has been cancelled.", appointment.Id);
                    break;

                case AppointmentStatus.Missed:
                    if (patientUserId != null)
                        await _notifications.SendNotificationAsync(patientUserId,
                            $"Your appointment on {date} at {time} was marked as missed.", appointment.Id);
                    break;
            }
        }
    }
}
