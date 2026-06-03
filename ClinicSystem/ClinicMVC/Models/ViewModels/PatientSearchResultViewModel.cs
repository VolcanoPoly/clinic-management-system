using ClinicAPI.Models;

namespace ClinicMVC.Models.ViewModels
{
    public class PatientSearchResultViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CPRNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
