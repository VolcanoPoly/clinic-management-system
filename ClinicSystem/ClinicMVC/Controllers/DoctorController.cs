/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 7 - Visit Records & Prescriptions
 * Description : Doctor-facing features: recording visit notes, creating and printing prescriptions, and browsing patient medical history.
 */
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
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notifications;

        public DoctorController(
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
        // Visit Records & Patient History
        // ??????????????????????????????????????????????????????????

        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var doctor = await _dbContext.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
                return NotFound("Doctor profile not found");

            var appointments = await _dbContext.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .Include(a => a.Specialization)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            var viewModel = appointments.Select(a => new AppointmentListItemViewModel
            {
                Id = a.Id,
                PatientName = $"{a.Patient?.User?.FirstName} {a.Patient?.User?.LastName}",
                DoctorName = $"{a.Doctor?.User?.FirstName} {a.Doctor?.User?.LastName}",
                SpecializationName = a.Specialization?.Name ?? "",
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status,
                Notes = a.Notes ?? ""
            }).ToList();

            return View("~/Views/Appointment/DoctorAppointments.cshtml", viewModel);
        }

        public async Task<IActionResult> PatientHistory(int patientId)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null)
                return NotFound("Doctor profile not found");

            var hasTreatedPatient = await _dbContext.Appointments
                .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorId.Value);
            if (!hasTreatedPatient)
                return Forbid();

            var patient = await _dbContext.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                return NotFound();

            var visitRecords = await _dbContext.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Where(v => v.Appointment.PatientId == patientId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var appointments = await _dbContext.Appointments
                .Include(a => a.Specialization)
                .Include(a => a.VisitRecord)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            var prescriptions = await _dbContext.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Where(p => p.VisitRecord.Appointment.PatientId == patientId)
                .OrderByDescending(p => p.IssuedAt)
                .ToListAsync();

            var viewModel = new PatientHistoryViewModel
            {
                PatientId = patient.Id.ToString(),
                PatientName = $"{patient.User?.FirstName} {patient.User?.LastName}",
                PatientEmail = patient.User?.Email ?? "",
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.User?.PhoneNumber ?? "",
                VisitRecords = visitRecords.Select(v => new VisitRecordViewModel
                {
                    Id = v.Id,
                    AppointmentId = v.AppointmentId,
                    PatientName = $"{patient.User?.FirstName} {patient.User?.LastName}",
                    DoctorName = $"{v.Appointment?.Doctor?.User?.FirstName} {v.Appointment?.Doctor?.User?.LastName}",
                    AppointmentDateTime = v.Appointment?.AppointmentDateTime ?? DateTime.Now,
                    DoctorNotes = v.DoctorNotes,
                    Diagnosis = v.Diagnosis,
                    Treatment = v.Treatment,
                    CreatedAt = v.CreatedAt,
                    HasPrescription = v.Prescription != null,
                    PrescriptionId = v.Prescription?.Id
                }).ToList(),
                Appointments = appointments.Select(a => new AppointmentHistoryItemViewModel
                {
                    Id = a.Id,
                    AppointmentDateTime = a.AppointmentDateTime,
                    DoctorName = "",
                    SpecializationName = a.Specialization?.Name ?? "",
                    Status = a.Status.ToString(),
                    HasVisitRecord = a.VisitRecord != null,
                    VisitRecordId = a.VisitRecord?.Id
                }).ToList(),
                Prescriptions = prescriptions.Select(p => new PrescriptionViewModel
                {
                    Id = p.Id,
                    VisitRecordId = p.VisitRecordId,
                    DoctorName = $"{p.Doctor?.User?.FirstName} {p.Doctor?.User?.LastName}",
                    IssuedAt = p.IssuedAt,
                    Items = p.Items.Select(i => new PrescriptionItemViewModel
                    {
                        Id = i.Id,
                        MedicationName = i.MedicationName,
                        Dosage = i.Dosage,
                        Frequency = i.Frequency,
                        Duration = i.Duration,
                        Instructions = i.Instructions
                    }).ToList()
                }).ToList()
            };

            return View("~/Views/Doctor/VisitRecords/PatientHistory.cshtml", viewModel);
        }

        public async Task<IActionResult> VisitRecordDetail(int id)
        {
            var visitRecord = await _dbContext.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(v => v.Prescription)
                    .ThenInclude(p => p.Items)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitRecord == null)
                return NotFound();

            if (!await DoctorOwnsVisitRecordAsync(visitRecord))
                return Forbid();

            var viewModel = new VisitRecordViewModel
            {
                Id = visitRecord.Id,
                AppointmentId = visitRecord.AppointmentId,
                PatientName = $"{visitRecord.Appointment?.Patient?.User?.FirstName} {visitRecord.Appointment?.Patient?.User?.LastName}",
                DoctorName = $"{visitRecord.Appointment?.Doctor?.User?.FirstName} {visitRecord.Appointment?.Doctor?.User?.LastName}",
                AppointmentDateTime = visitRecord.Appointment?.AppointmentDateTime ?? DateTime.Now,
                DoctorNotes = visitRecord.DoctorNotes,
                Diagnosis = visitRecord.Diagnosis,
                Treatment = visitRecord.Treatment,
                CreatedAt = visitRecord.CreatedAt,
                HasPrescription = visitRecord.Prescription != null,
                PrescriptionId = visitRecord.Prescription?.Id
            };

            return View("~/Views/Doctor/VisitRecords/VisitRecordDetail.cshtml", viewModel);
        }

        public async Task<IActionResult> CreateVisitRecord(int appointmentId)
        {
            var appointment = await _dbContext.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.VisitRecord)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return NotFound();

            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null || appointment.DoctorId != doctorId.Value)
                return Forbid();

            if (appointment.Status != AppointmentStatus.Completed)
                return BadRequest("Visit records can only be created for completed appointments.");

            var existingVisitRecordId = await _dbContext.VisitRecords
                .Where(v => v.AppointmentId == appointmentId)
                .Select(v => (int?)v.Id)
                .FirstOrDefaultAsync();
            if (existingVisitRecordId.HasValue)
                return RedirectToAction(nameof(VisitRecordDetail), new { id = existingVisitRecordId.Value });

            var viewModel = new VisitRecordFormViewModel
            {
                AppointmentId = appointmentId,
                PatientName = $"{appointment.Patient?.User?.FirstName} {appointment.Patient?.User?.LastName}",
                DoctorName = $"{appointment.Doctor?.User?.FirstName} {appointment.Doctor?.User?.LastName}",
                AppointmentDateTime = appointment.AppointmentDateTime
            };

            return View("~/Views/Doctor/VisitRecords/CreateVisitRecord.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVisitRecord(VisitRecordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Doctor/VisitRecords/CreateVisitRecord.cshtml", model);

            try
            {
                var doctorId = await GetCurrentDoctorIdAsync();
                if (doctorId == null)
                    return Unauthorized();

                var appointment = await _dbContext.Appointments
                    .Include(a => a.VisitRecord)
                    .FirstOrDefaultAsync(a => a.Id == model.AppointmentId);

                if (appointment == null)
                    return NotFound();

                if (appointment.DoctorId != doctorId.Value)
                    return Forbid();

                if (appointment.Status != AppointmentStatus.Completed)
                {
                    ModelState.AddModelError(string.Empty, "Visit records can only be created for completed appointments.");
                    return View("~/Views/Doctor/VisitRecords/CreateVisitRecord.cshtml", model);
                }

                if (appointment.VisitRecord != null)
                    return RedirectToAction(nameof(VisitRecordDetail), new { id = appointment.VisitRecord.Id });

                var visitRecord = new VisitRecord
                {
                    AppointmentId = model.AppointmentId,
                    DoctorNotes = model.DoctorNotes,
                    Diagnosis = model.Diagnosis,
                    Treatment = model.Treatment,
                    CreatedAt = DateTime.Now
                };

                _dbContext.VisitRecords.Add(visitRecord);

                await _dbContext.SaveChangesAsync();

                // Notify patient that visit record is ready
                var appointmentData = await _dbContext.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(a => a.Id == model.AppointmentId);

                if (appointmentData?.Patient?.User != null)
                    await _notifications.SendNotificationAsync(
                        appointmentData.Patient.User.Id,
                        $"Your visit record from Dr. {model.DoctorName} is now available.",
                        model.AppointmentId);

                return RedirectToAction(nameof(VisitRecordDetail), new { id = visitRecord.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating visit record: {ex.Message}");
                return View("~/Views/Doctor/VisitRecords/CreateVisitRecord.cshtml", model);
            }
        }

        public async Task<IActionResult> EditVisitRecord(int id)
        {
            var visitRecord = await _dbContext.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitRecord == null)
                return NotFound();

            if (!await DoctorOwnsVisitRecordAsync(visitRecord))
                return Forbid();

            var viewModel = new VisitRecordFormViewModel
            {
                Id = id,
                AppointmentId = visitRecord.AppointmentId,
                PatientName = $"{visitRecord.Appointment?.Patient?.User?.FirstName} {visitRecord.Appointment?.Patient?.User?.LastName}",
                DoctorName = $"{visitRecord.Appointment?.Doctor?.User?.FirstName} {visitRecord.Appointment?.Doctor?.User?.LastName}",
                AppointmentDateTime = visitRecord.Appointment?.AppointmentDateTime ?? DateTime.Now,
                DoctorNotes = visitRecord.DoctorNotes,
                Diagnosis = visitRecord.Diagnosis,
                Treatment = visitRecord.Treatment
            };

            return View("~/Views/Doctor/VisitRecords/CreateVisitRecord.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVisitRecord(int id, VisitRecordFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View("~/Views/Doctor/VisitRecords/CreateVisitRecord.cshtml", model);

            try
            {
                var visitRecord = await _dbContext.VisitRecords.FirstOrDefaultAsync(v => v.Id == id);
                if (visitRecord == null)
                    return NotFound();

                if (!await DoctorOwnsVisitRecordAsync(visitRecord))
                    return Forbid();

                visitRecord.DoctorNotes = model.DoctorNotes;
                visitRecord.Diagnosis = model.Diagnosis;
                visitRecord.Treatment = model.Treatment;

                _dbContext.Update(visitRecord);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(VisitRecordDetail), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating visit record: {ex.Message}");
                return View("~/Views/Doctor/VisitRecords/CreateVisitRecord.cshtml", model);
            }
        }

        // ??????????????????????????????????????????????????????????
        // Prescriptions
        // ??????????????????????????????????????????????????????????

        public async Task<IActionResult> CreatePrescription(int visitRecordId)
        {
            var visitRecord = await _dbContext.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(v => v.Id == visitRecordId);

            if (visitRecord == null)
                return NotFound();

            if (!await DoctorOwnsVisitRecordAsync(visitRecord))
                return Forbid();

            if (visitRecord.Appointment?.Status != AppointmentStatus.Completed)
                return BadRequest("Prescriptions can only be created for completed appointments.");

            // Check if prescription already exists
            var existing = await _dbContext.Prescriptions
                .FirstOrDefaultAsync(p => p.VisitRecordId == visitRecordId);

            if (existing != null)
                return RedirectToAction(nameof(EditPrescription), new { id = existing.Id });

            var viewModel = new PrescriptionFormViewModel
            {
                VisitRecordId = visitRecordId,
                Items = new List<PrescriptionItemFormViewModel>
                {
                    new PrescriptionItemFormViewModel()
                }
            };

            return View("~/Views/Doctor/Prescriptions/CreatePrescription.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePrescription(PrescriptionFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Doctor/Prescriptions/CreatePrescription.cshtml", model);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

                if (doctor == null)
                    return Unauthorized();

                var visitRecord = await _dbContext.VisitRecords
                    .Include(v => v.Appointment)
                    .FirstOrDefaultAsync(v => v.Id == model.VisitRecordId);

                if (visitRecord == null)
                    return NotFound();

                if (visitRecord.Appointment?.DoctorId != doctor.Id)
                    return Forbid();

                if (visitRecord.Appointment.Status != AppointmentStatus.Completed)
                {
                    ModelState.AddModelError(string.Empty, "Prescriptions can only be created for completed appointments.");
                    return View("~/Views/Doctor/Prescriptions/CreatePrescription.cshtml", model);
                }

                var prescription = new Prescription
                {
                    VisitRecordId = model.VisitRecordId,
                    DoctorId = doctor.Id,
                    IssuedAt = DateTime.Now
                };

                if (model.Items != null)
                {
                    foreach (var item in model.Items.Where(i => !string.IsNullOrEmpty(i.MedicationName)))
                    {
                        prescription.Items.Add(new PrescriptionItem
                        {
                            MedicationName = item.MedicationName,
                            Dosage = item.Dosage,
                            Frequency = item.Frequency,
                            Duration = item.Duration,
                            Instructions = item.Instructions
                        });
                    }
                }

                _dbContext.Prescriptions.Add(prescription);
                await _dbContext.SaveChangesAsync();

                // Notify patient about new prescription
                var visitRecordWithPatient = await _dbContext.VisitRecords
                    .Include(v => v.Appointment)
                        .ThenInclude(a => a.Patient)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(v => v.Id == model.VisitRecordId);

                if (visitRecordWithPatient?.Appointment?.Patient?.User != null)
                    await _notifications.SendNotificationAsync(
                        visitRecordWithPatient.Appointment.Patient.User.Id,
                        "A new prescription has been issued for you. You can view it in your medical history.",
                        visitRecordWithPatient.AppointmentId);

                return RedirectToAction(nameof(ViewPrescription), new { id = prescription.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating prescription: {ex.Message}");
                return View("~/Views/Doctor/Prescriptions/CreatePrescription.cshtml", model);
            }
        }

        public async Task<IActionResult> ViewPrescription(int id)
        {
            var prescription = await _dbContext.Prescriptions
                .Include(p => p.VisitRecord)
                    .ThenInclude(v => v.Appointment)
                        .ThenInclude(a => a.Patient)
                            .ThenInclude(p => p.User)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound();

            if (!await DoctorOwnsPrescriptionAsync(prescription))
                return Forbid();

            var viewModel = new PrescriptionViewModel
            {
                Id = prescription.Id,
                VisitRecordId = prescription.VisitRecordId,
                DoctorName = $"{prescription.Doctor?.User?.FirstName} {prescription.Doctor?.User?.LastName}",
                IssuedAt = prescription.IssuedAt,
                Items = prescription.Items.Select(i => new PrescriptionItemViewModel
                {
                    Id = i.Id,
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                }).ToList()
            };

            return View("~/Views/Doctor/Prescriptions/ViewPrescription.cshtml", viewModel);
        }

        public async Task<IActionResult> PrintPrescription(int id)
        {
            var prescription = await _dbContext.Prescriptions
                .Include(p => p.VisitRecord)
                    .ThenInclude(v => v.Appointment)
                        .ThenInclude(a => a.Patient)
                            .ThenInclude(p => p.User)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound();

            if (!await DoctorOwnsPrescriptionAsync(prescription))
                return Forbid();

            var viewModel = new PrescriptionViewModel
            {
                Id            = prescription.Id,
                VisitRecordId = prescription.VisitRecordId,
                DoctorName    = $"{prescription.Doctor?.User?.FirstName} {prescription.Doctor?.User?.LastName}",
                IssuedAt      = prescription.IssuedAt,
                Items         = prescription.Items.Select(i => new PrescriptionItemViewModel
                {
                    Id             = i.Id,
                    MedicationName = i.MedicationName,
                    Dosage         = i.Dosage,
                    Frequency      = i.Frequency,
                    Duration       = i.Duration,
                    Instructions   = i.Instructions
                }).ToList()
            };

            return View("~/Views/Doctor/Prescriptions/PrintPrescription.cshtml", viewModel);
        }

        public async Task<IActionResult> EditPrescription(int id)
        {
            var prescription = await _dbContext.Prescriptions
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound();

            if (!await DoctorOwnsPrescriptionAsync(prescription))
                return Forbid();

            var viewModel = new PrescriptionFormViewModel
            {
                VisitRecordId = prescription.VisitRecordId,
                Items = prescription.Items.Select(i => new PrescriptionItemFormViewModel
                {
                    Id = i.Id,
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                }).ToList()
            };

            return View("~/Views/Doctor/Prescriptions/CreatePrescription.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPrescription(int id, PrescriptionFormViewModel model)
        {
            try
            {
                var prescription = await _dbContext.Prescriptions
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (prescription == null)
                    return NotFound();

                if (!await DoctorOwnsPrescriptionAsync(prescription))
                    return Forbid();

                // Remove existing items
                _dbContext.PrescriptionItems.RemoveRange(prescription.Items);

                // Add new items
                if (model.Items != null)
                {
                    foreach (var item in model.Items.Where(i => !string.IsNullOrEmpty(i.MedicationName)))
                    {
                        prescription.Items.Add(new PrescriptionItem
                        {
                            MedicationName = item.MedicationName,
                            Dosage = item.Dosage,
                            Frequency = item.Frequency,
                            Duration = item.Duration,
                            Instructions = item.Instructions
                        });
                    }
                }

                _dbContext.Update(prescription);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(ViewPrescription), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating prescription: {ex.Message}");
                return View("~/Views/Doctor/Prescriptions/CreatePrescription.cshtml", model);
            }
        }

        public async Task<IActionResult> MyVisitRecords()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound("Doctor profile not found");

            var visitRecords = await _dbContext.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Include(v => v.Prescription)
                .Where(v => v.Appointment.DoctorId == doctor.Id)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var viewModel = visitRecords.Select(v => new VisitRecordViewModel
            {
                Id = v.Id,
                AppointmentId = v.AppointmentId,
                PatientName = $"{v.Appointment?.Patient?.User?.FirstName} {v.Appointment?.Patient?.User?.LastName}",
                DoctorName = $"{user.FirstName} {user.LastName}",
                AppointmentDateTime = v.Appointment?.AppointmentDateTime ?? DateTime.Now,
                Diagnosis = v.Diagnosis,
                Treatment = v.Treatment,
                DoctorNotes = v.DoctorNotes,
                CreatedAt = v.CreatedAt,
                HasPrescription = v.Prescription != null,
                PrescriptionId = v.Prescription?.Id
            }).ToList();

            return View("~/Views/Doctor/VisitRecords/MyVisitRecords.cshtml", viewModel);
        }

        public async Task<IActionResult> MyPrescriptions()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound("Doctor profile not found");

            var prescriptions = await _dbContext.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.VisitRecord)
                    .ThenInclude(v => v.Appointment)
                        .ThenInclude(a => a.Patient)
                            .ThenInclude(p => p.User)
                .Where(p => p.DoctorId == doctor.Id)
                .OrderByDescending(p => p.IssuedAt)
                .ToListAsync();

            var viewModel = prescriptions.Select(p => new PrescriptionViewModel
            {
                Id = p.Id,
                VisitRecordId = p.VisitRecordId,
                DoctorName = $"{user.FirstName} {user.LastName}",
                IssuedAt = p.IssuedAt,
                Items = p.Items.Select(i => new PrescriptionItemViewModel
                {
                    Id = i.Id,
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                }).ToList()
            }).ToList();

            return View("~/Views/Doctor/Prescriptions/MyPrescriptions.cshtml", viewModel);
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

        private async Task<int?> GetCurrentDoctorIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return null;

            return await _dbContext.Doctors
                .Where(d => d.UserId == user.Id)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> DoctorOwnsVisitRecordAsync(VisitRecord visitRecord)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null)
                return false;

            if (visitRecord.Appointment != null)
                return visitRecord.Appointment.DoctorId == doctorId.Value;

            return await _dbContext.VisitRecords
                .AnyAsync(v => v.Id == visitRecord.Id && v.Appointment.DoctorId == doctorId.Value);
        }

        private async Task<bool> DoctorOwnsPrescriptionAsync(Prescription prescription)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null)
                return false;

            return prescription.DoctorId == doctorId.Value;
        }
    }
}
