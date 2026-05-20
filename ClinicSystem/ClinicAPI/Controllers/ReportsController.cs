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
    [Authorize(Roles = "ClinicManager")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ApplicationDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/reports/appointment-stats?from={date}&to={date}
        /// JWT required, ClinicManager role only - returns appointment statistics
        /// </summary>
        [HttpGet("appointment-stats")]
        public async Task<ActionResult<AppointmentStatsDto>> GetAppointmentStats(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                if (from > to)
                {
                    return BadRequest("'from' date must be before 'to' date");
                }

                var appointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.AppointmentDateTime >= from && a.AppointmentDateTime <= to)
                    .Include(a => a.Specialization)
                    .ToListAsync();

                var stats = new AppointmentStatsDto
                {
                    TotalAppointments = appointments.Count,
                    ByStatus = appointments
                        .GroupBy(a => a.Status.ToString())
                        .ToDictionary(g => g.Key, g => g.Count()),
                    BySpecialization = appointments
                        .GroupBy(a => a.Specialization!.Name)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment statistics");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// GET /api/reports/doctor-utilization?from={date}&to={date}
        /// JWT required, ClinicManager role only - returns doctor utilization metrics
        /// </summary>
        [HttpGet("doctor-utilization")]
        public async Task<ActionResult<DoctorUtilizationReportDto>> GetDoctorUtilization(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                if (from > to)
                {
                    return BadRequest("'from' date must be before 'to' date");
                }

                var doctors = await _context.Doctors
                    .AsNoTracking()
                    .Include(d => d.User)
                    .Include(d => d.Specializations)
                    .ThenInclude(ds => ds.Specialization)
                    .Include(d => d.Appointments)
                    .ToListAsync();

                var metrics = new List<DoctorUtilizationDto>();

                foreach (var doctor in doctors)
                {
                    var doctorAppointments = doctor.Appointments
                        .Where(a => a.AppointmentDateTime >= from && a.AppointmentDateTime <= to)
                        .ToList();

                    var completedAppointments = doctorAppointments
                        .Count(a => a.Status == AppointmentStatus.Completed);

                    var appointmentCount = doctorAppointments.Count;
                    var completionRate = appointmentCount > 0
                        ? (decimal)completedAppointments / appointmentCount * 100
                        : 0;

                    var specialization = doctor.Specializations.FirstOrDefault()?.Specialization?.Name ?? "N/A";

                    metrics.Add(new DoctorUtilizationDto
                    {
                        DoctorId = doctor.Id,
                        DoctorName = $"{doctor.User!.FirstName} {doctor.User.LastName}",
                        Specialization = specialization,
                        AppointmentCount = appointmentCount,
                        CompletionRate = Math.Round(completionRate, 2)
                    });
                }

                var report = new DoctorUtilizationReportDto
                {
                    DoctorMetrics = metrics.OrderBy(m => m.DoctorName).ToList()
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor utilization report");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// GET /api/reports/cancellation-rates?from={date}&to={date}
        /// JWT required, ClinicManager role only - returns cancellation and missed rate statistics
        /// </summary>
        [HttpGet("cancellation-rates")]
        public async Task<ActionResult<CancellationRatesReportDto>> GetCancellationRates(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                if (from > to)
                {
                    return BadRequest("'from' date must be before 'to' date");
                }

                var appointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.AppointmentDateTime >= from && a.AppointmentDateTime <= to)
                    .ToListAsync();

                var cancelledCount = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
                var missedCount = appointments.Count(a => a.Status == AppointmentStatus.Missed);
                var totalAppointments = appointments.Count;

                var cancellationRate = totalAppointments > 0
                    ? (decimal)cancelledCount / totalAppointments * 100
                    : 0;

                var missedRate = totalAppointments > 0
                    ? (decimal)missedCount / totalAppointments * 100
                    : 0;

                // Group by date to get daily data
                var dailyData = appointments
                    .GroupBy(a => a.AppointmentDateTime.Date)
                    .Select(g =>
                    {
                        var dailyCancelled = g.Count(a => a.Status == AppointmentStatus.Cancelled);
                        var dailyMissed = g.Count(a => a.Status == AppointmentStatus.Missed);
                        var dailyTotal = g.Count();

                        return new CancellationRateDataDto
                        {
                            Date = g.Key,
                            CancellationCount = dailyCancelled,
                            MissedCount = dailyMissed,
                            CancellationRate = dailyTotal > 0
                                ? Math.Round((decimal)(dailyCancelled + dailyMissed) / dailyTotal * 100, 2)
                                : 0
                        };
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var report = new CancellationRatesReportDto
                {
                    TotalCancellations = cancelledCount,
                    TotalMissed = missedCount,
                    OverallCancellationRate = Math.Round(cancellationRate, 2),
                    OverallMissedRate = Math.Round(missedRate, 2),
                    DailyData = dailyData
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cancellation rates report");
                return StatusCode(500);
            }
        }
    }
}
