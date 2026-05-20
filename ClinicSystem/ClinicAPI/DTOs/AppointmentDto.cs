namespace ClinicAPI.DTOs
{
    /// <summary>
    /// DTO for appointment response (authenticated endpoint)
    /// </summary>
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for appointment status history
    /// </summary>
    public class AppointmentStatusHistoryDto
    {
        public int Id { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string ChangedByUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for appointment detail with full history
    /// </summary>
    public class AppointmentDetailDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
        public List<AppointmentStatusHistoryDto> StatusHistory { get; set; } = new();
    }
}
