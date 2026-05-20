namespace ClinicAPI.DTOs
{
    /// <summary>
    /// DTO for doctor response
    /// </summary>
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public List<string> Specializations { get; set; } = new();
    }

    /// <summary>
    /// DTO for available time slots
    /// </summary>
    public class TimeSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Available { get; set; }
    }

    /// <summary>
    /// DTO for doctor availability response
    /// </summary>
    public class DoctorAvailabilityDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    }
}
