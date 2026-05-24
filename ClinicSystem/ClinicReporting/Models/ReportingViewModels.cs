using System.ComponentModel.DataAnnotations;

namespace ClinicReporting.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class LoginResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserInfoModel? User { get; set; }
    }

    public class UserInfoModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class AppointmentStatsViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalAppointments { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public Dictionary<string, int> BySpecialization { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class DoctorUtilizationViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<DoctorUtilizationMetricViewModel> DoctorMetrics { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class DoctorUtilizationMetricViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class CancellationRatesViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalCancellations { get; set; }
        public int TotalMissed { get; set; }
        public decimal OverallCancellationRate { get; set; }
        public decimal OverallMissedRate { get; set; }
        public List<CancellationRatePointViewModel> DailyData { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class CancellationRatePointViewModel
    {
        public DateTime Date { get; set; }
        public int CancellationCount { get; set; }
        public int MissedCount { get; set; }
        public decimal CancellationRate { get; set; }
    }
}
