/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : Calculates available 30-minute time slots for a doctor on a given date, accounting for their weekly schedule, leave periods, and existing non-cancelled appointments.
 */
using ClinicAPI.Data;
using ClinicAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Services
{
    public class AvailabilityService
    {
        private readonly ApplicationDbContext _db;
        private const int SlotMinutes = 30;

        public AvailabilityService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns available 30-minute time slot strings ("HH:mm") for a doctor on a given date.
        /// Filters out: outside working hours, existing non-cancelled appointments, leave periods.
        /// </summary>
        public async Task<List<string>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            // Get doctor's schedule for this day of week
            var schedule = await _db.DoctorSchedules
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek);

            if (schedule == null)
                return new List<string>(); // Doctor doesn't work this day

            // Check if doctor is on leave
            var dateOnly = date.Date;
            var onLeave = await _db.DoctorLeaves.AnyAsync(l =>
                l.DoctorId == doctorId &&
                l.StartDate.Date <= dateOnly &&
                l.EndDate.Date >= dateOnly);

            if (onLeave)
                return new List<string>();

            // Get already booked slots for this doctor on this date
            var bookedTimes = await _db.Appointments
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDateTime.Date == dateOnly &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.Missed)
                .Select(a => a.AppointmentDateTime.TimeOfDay)
                .ToListAsync();

            // Generate all 30-min slots within working hours
            var slots = new List<string>();
            var current = schedule.StartTime;
            var end = schedule.EndTime.Subtract(TimeSpan.FromMinutes(SlotMinutes));

            while (current <= end)
            {
                if (!bookedTimes.Contains(current))
                    slots.Add(current.ToString(@"hh\:mm"));
                current = current.Add(TimeSpan.FromMinutes(SlotMinutes));
            }

            return slots;
        }

        /// <summary>
        /// Returns the next available date (from today) for a doctor.
        /// Looks up to 30 days ahead.
        /// </summary>
        public async Task<DateTime?> GetNextAvailableDateAsync(int doctorId)
        {
            for (int i = 1; i <= 30; i++)
            {
                var date = DateTime.Today.AddDays(i);
                var slots = await GetAvailableSlotsAsync(doctorId, date);
                if (slots.Count > 0)
                    return date;
            }
            return null;
        }
    }
}
