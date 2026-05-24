using System.ComponentModel.DataAnnotations;
using ClinicAPI.DTOs;

namespace ClinicMVC.Models.ViewModels
{
    public class PublicLookupViewModel
    {
        [Required(ErrorMessage = "CPR is required")]
        [Display(Name = "CPR Number")]
        public string Cpr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reference number is required")]
        [Display(Name = "Reference Number")]
        public string Reference { get; set; } = string.Empty;

        public bool HasSubmitted { get; set; }
        public bool LookupSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public PatientLookupResponseDto? LookupResult { get; set; }
    }
}
