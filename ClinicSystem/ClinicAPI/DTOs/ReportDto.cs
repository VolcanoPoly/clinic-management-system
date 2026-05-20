namespace ClinicAPI.DTOs
{
    /// <summary>
    /// DTO for appointment statistics report
    /// </summary>
    public class AppointmentStatsDto
    {
        public int TotalAppointments { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public Dictionary<string, int> BySpecialization { get; set; } = new();
    }

    /// <summary>
    /// DTO for doctor utilization metrics
    /// </summary>
    public class DoctorUtilizationDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public decimal CompletionRate { get; set; }
    }

    /// <summary>
    /// DTO for doctor utilization report
    /// </summary>
    public class DoctorUtilizationReportDto
    {
        public List<DoctorUtilizationDto> DoctorMetrics { get; set; } = new();
    }

    /// <summary>
    /// DTO for cancellation rates data point
    /// </summary>
    public class CancellationRateDataDto
    {
        public DateTime Date { get; set; }
        public int CancellationCount { get; set; }
        public int MissedCount { get; set; }
        public decimal CancellationRate { get; set; }
    }

    /// <summary>
    /// DTO for cancellation rates report
    /// </summary>
    public class CancellationRatesReportDto
    {
        public int TotalCancellations { get; set; }
        public int TotalMissed { get; set; }
        public decimal OverallCancellationRate { get; set; }
        public decimal OverallMissedRate { get; set; }
        public List<CancellationRateDataDto> DailyData { get; set; } = new();
    }
}
