namespace ClinicAPI.DTOs
{
    /// <summary>
    /// DTO for appointment lookup response (public, no auth required)
    /// </summary>
    public class AppointmentLookupDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for visit record summary
    /// </summary>
    public class VisitSummaryDto
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for patient lookup response (public endpoint)
    /// </summary>
    public class PatientLookupResponseDto
    {
        public bool Found { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public List<AppointmentLookupDto> UpcomingAppointments { get; set; } = new();
        public List<VisitSummaryDto> LastThreeVisits { get; set; } = new();
    }
}
