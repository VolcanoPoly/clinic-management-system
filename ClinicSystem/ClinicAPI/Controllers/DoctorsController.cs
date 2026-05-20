using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.DTOs;

namespace ClinicAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(ApplicationDbContext context, ILogger<DoctorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/doctors
        /// JWT required - returns list of doctors with their specializations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DoctorDto>>> GetDoctors()
        {
            try
            {
                var doctors = await _context.Doctors
                    .AsNoTracking()
                    .Include(d => d.User)
                    .Include(d => d.Specializations)
                    .ThenInclude(ds => ds.Specialization)
                    .Select(d => new DoctorDto
                    {
                        Id = d.Id,
                        Name = $"{d.User!.FirstName} {d.User.LastName}",
                        Email = d.User.Email ?? string.Empty,
                        LicenseNumber = d.LicenseNumber,
                        Bio = d.Bio,
                        Specializations = d.Specializations
                            .Select(ds => ds.Specialization!.Name)
                            .ToList()
                    })
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// GET /api/doctors/{id}/availability?date={date}
        /// JWT required - returns available 30-min time slots for the doctor on that date
        /// </summary>
        [HttpGet("{id}/availability")]
        public async Task<ActionResult<DoctorAvailabilityDto>> GetDoctorAvailability(int id, [FromQuery] DateTime date)
        {
            try
            {
                var doctor = await _context.Doctors
                    .AsNoTracking()
                    .Include(d => d.User)
                    .Include(d => d.Schedules)
                    .Include(d => d.Leaves)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (doctor == null)
                {
                    return NotFound();
                }

                // Check if doctor has leave on this date
                var dateOnly = date.Date;
                var isOnLeave = doctor.Leaves.Any(l =>
                    l.StartDate.Date <= dateOnly && dateOnly <= l.EndDate.Date);

                if (isOnLeave)
                {
                    return Ok(new DoctorAvailabilityDto
                    {
                        DoctorId = doctor.Id,
                        DoctorName = $"{doctor.User!.FirstName} {doctor.User.LastName}",
                        Date = date.Date,
                        AvailableSlots = new()
                    });
                }

                // Get schedule for the day of week
                var dayOfWeek = date.DayOfWeek;
                var schedule = doctor.Schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

                if (schedule == null)
                {
                    return Ok(new DoctorAvailabilityDto
                    {
                        DoctorId = doctor.Id,
                        DoctorName = $"{doctor.User!.FirstName} {doctor.User.LastName}",
                        Date = date.Date,
                        AvailableSlots = new()
                    });
                }

                // Get existing appointments for this doctor on this date
                var existingAppointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == id &&
                                a.AppointmentDateTime.Date == dateOnly)
                    .Select(a => new { a.AppointmentDateTime })
                    .ToListAsync();

                var availableSlots = new List<TimeSlotDto>();

                // Generate 30-minute slots
                var startTime = dateOnly.Add(schedule.StartTime);
                var endTime = dateOnly.Add(schedule.EndTime);
                var currentSlotStart = startTime;

                while (currentSlotStart.AddMinutes(30) <= endTime)
                {
                    var currentSlotEnd = currentSlotStart.AddMinutes(30);

                    // Check if this slot overlaps with any existing appointment
                    var isBooked = existingAppointments.Any(apt =>
                        apt.AppointmentDateTime < currentSlotEnd && apt.AppointmentDateTime.AddMinutes(30) > currentSlotStart);

                    availableSlots.Add(new TimeSlotDto
                    {
                        StartTime = currentSlotStart,
                        EndTime = currentSlotEnd,
                        Available = !isBooked
                    });

                    currentSlotStart = currentSlotEnd;
                }

                var response = new DoctorAvailabilityDto
                {
                    DoctorId = doctor.Id,
                    DoctorName = $"{doctor.User!.FirstName} {doctor.User.LastName}",
                    Date = date.Date,
                    AvailableSlots = availableSlots
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor availability");
                return StatusCode(500);
            }
        }
    }
}
