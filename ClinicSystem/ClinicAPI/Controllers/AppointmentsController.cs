using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.DTOs;
using ClinicAPI.Models;

namespace ClinicAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ApplicationDbContext context, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/appointments/lookup?cpr={cpr}&ref={ref}
        /// PUBLIC endpoint - finds patient by CPR and reference number, returns upcoming appointments and last 3 visits
        /// </summary>
        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<ActionResult<PatientLookupResponseDto>> LookupPatientAppointments(
            [FromQuery] string cpr,
            [FromQuery] string @ref)
        {
            if (string.IsNullOrWhiteSpace(cpr) || string.IsNullOrWhiteSpace(@ref))
            {
                return BadRequest(new PatientLookupResponseDto
                {
                    Found = false,
                    Message = "CPR and reference number are required"
                });
            }

            try
            {
                var patient = await _context.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.CPRNumber == cpr && p.ReferenceNumber == @ref);

                if (patient == null)
                {
                    _logger.LogWarning($"Patient lookup failed for CPR: {cpr}");
                    return NotFound(new PatientLookupResponseDto
                    {
                        Found = false,
                        Message = "Patient not found with the provided CPR and reference number"
                    });
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == patient.UserId);

                if (user == null)
                {
                    return NotFound(new PatientLookupResponseDto
                    {
                        Found = false,
                        Message = "Patient record incomplete"
                    });
                }

                // Get upcoming appointments
                var upcomingAppointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.PatientId == patient.Id && a.AppointmentDateTime > DateTime.Now)
                    .OrderBy(a => a.AppointmentDateTime)
                    .Include(a => a.Doctor)
                    .Include(a => a.Doctor!.User)
                    .Include(a => a.Specialization)
                    .Select(a => new AppointmentLookupDto
                    {
                        Id = a.Id,
                        AppointmentDateTime = a.AppointmentDateTime,
                        Status = a.Status.ToString(),
                        DoctorName = $"{a.Doctor!.User!.FirstName} {a.Doctor.User.LastName}",
                        Specialization = a.Specialization!.Name,
                        Notes = a.Notes
                    })
                    .ToListAsync();

                // Get last 3 visit records (from completed appointments)
                var lastThreeVisits = await _context.VisitRecords
                    .AsNoTracking()
                    .Where(v => v.Appointment!.PatientId == patient.Id)
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(3)
                    .Include(v => v.Appointment)
                    .Include(v => v.Appointment!.Doctor)
                    .Include(v => v.Appointment!.Doctor!.User)
                    .Include(v => v.Appointment!.Specialization)
                    .Select(v => new VisitSummaryDto
                    {
                        Id = v.Id,
                        VisitDate = v.CreatedAt,
                        DoctorName = $"{v.Appointment!.Doctor!.User!.FirstName} {v.Appointment.Doctor.User.LastName}",
                        Specialization = v.Appointment.Specialization!.Name,
                        Diagnosis = v.Diagnosis,
                        Treatment = v.Treatment
                    })
                    .ToListAsync();

                var response = new PatientLookupResponseDto
                {
                    Found = true,
                    Message = "Patient found",
                    PatientName = $"{user.FirstName} {user.LastName}",
                    UpcomingAppointments = upcomingAppointments,
                    LastThreeVisits = lastThreeVisits
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient appointment lookup");
                return StatusCode(500, new PatientLookupResponseDto
                {
                    Found = false,
                    Message = "An error occurred while looking up appointments"
                });
            }
        }

        /// <summary>
        /// GET /api/appointments
        /// JWT required (Receptionist or ClinicManager) - returns list of appointments with optional filters
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Receptionist,ClinicManager")]
        public async Task<ActionResult<List<AppointmentDto>>> GetAppointments(
            [FromQuery] DateTime? date,
            [FromQuery] int? doctorId,
            [FromQuery] string? status)
        {
            try
            {
                var query = _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .Include(a => a.Patient!.User)
                    .Include(a => a.Doctor)
                    .Include(a => a.Doctor!.User)
                    .Include(a => a.Specialization)
                    .AsQueryable();

                // Filter by date (if provided, get appointments for that date)
                if (date.HasValue)
                {
                    var startOfDay = date.Value.Date;
                    var endOfDay = startOfDay.AddDays(1);
                    query = query.Where(a => a.AppointmentDateTime >= startOfDay && a.AppointmentDateTime < endOfDay);
                }

                // Filter by doctor
                if (doctorId.HasValue)
                {
                    query = query.Where(a => a.DoctorId == doctorId.Value);
                }

                // Filter by status
                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, ignoreCase: true, out var appointmentStatus))
                {
                    query = query.Where(a => a.Status == appointmentStatus);
                }

                var appointments = await query
                    .OrderBy(a => a.AppointmentDateTime)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        PatientId = a.PatientId,
                        PatientName = $"{a.Patient!.User!.FirstName} {a.Patient.User.LastName}",
                        DoctorId = a.DoctorId,
                        DoctorName = $"{a.Doctor!.User!.FirstName} {a.Doctor.User.LastName}",
                        Specialization = a.Specialization!.Name,
                        AppointmentDateTime = a.AppointmentDateTime,
                        Status = a.Status.ToString(),
                        Notes = a.Notes,
                        CancellationReason = a.CancellationReason
                    })
                    .ToListAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// GET /api/appointments/{id}
        /// JWT required - returns single appointment detail with status history
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Receptionist,ClinicManager")]
        public async Task<ActionResult<AppointmentDetailDto>> GetAppointmentDetail(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .Include(a => a.Patient!.User)
                    .Include(a => a.Doctor)
                    .Include(a => a.Doctor!.User)
                    .Include(a => a.Specialization)
                    .Include(a => a.StatusHistory)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                var detail = new AppointmentDetailDto
                {
                    Id = appointment.Id,
                    PatientId = appointment.PatientId,
                    PatientName = $"{appointment.Patient!.User!.FirstName} {appointment.Patient.User.LastName}",
                    DoctorId = appointment.DoctorId,
                    DoctorName = $"{appointment.Doctor!.User!.FirstName} {appointment.Doctor.User.LastName}",
                    Specialization = appointment.Specialization!.Name,
                    AppointmentDateTime = appointment.AppointmentDateTime,
                    Status = appointment.Status.ToString(),
                    Notes = appointment.Notes,
                    CancellationReason = appointment.CancellationReason,
                    StatusHistory = appointment.StatusHistory
                        .OrderBy(sh => sh.ChangedAt)
                        .Select(sh => new AppointmentStatusHistoryDto
                        {
                            Id = sh.Id,
                            PreviousStatus = sh.OldStatus.ToString(),
                            NewStatus = sh.NewStatus.ToString(),
                            ChangedAt = sh.ChangedAt,
                            ChangedByUserId = sh.ChangedByUserId
                        })
                        .ToList()
                };

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment detail");
                return StatusCode(500);
            }
        }
    }
}
