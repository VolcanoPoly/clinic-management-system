using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class DoctorLeaveCreateViewModel
    {
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        public string StartDateString { get; set; } = string.Empty;

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        public string EndDateString { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Reason")]
        public string? Reason { get; set; }
    }
}
