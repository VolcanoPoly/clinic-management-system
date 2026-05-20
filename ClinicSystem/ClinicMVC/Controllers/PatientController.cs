/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 7 - Visit Records & Prescriptions
 * Description : Patient-facing features: viewing personal medical history, visit records, and prescriptions issued by doctors.
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Models.ViewModels;

namespace ClinicMVC.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // ??????????????????????????????????????????????????????????
        // Patient Medical History
        // ??????????????????????????????????????????????????????????

        public async Task<IActionResult> MyMedicalHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var patient = await _dbContext.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Id == user.Id);

            if (patient == null)
                return NotFound();

            var appointments = await _dbContext.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Specialization)
                .Include(a => a.VisitRecord)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            var visitRecords = await _dbContext.VisitRecords
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Where(v => v.Appointment.PatientId == patient.Id)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var prescriptions = await _dbContext.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Where(p => p.VisitRecord.Appointment.PatientId == patient.Id)
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
                    DoctorName = $"{a.Doctor?.User?.FirstName} {a.Doctor?.User?.LastName}",
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

            return View(viewModel);
        }

        public async Task<IActionResult> ViewVisitRecord(int id)
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

            return View(viewModel);
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

            return View(viewModel);
        }
    }
}
