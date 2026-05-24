using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class DoctorFormViewModel
    {
        public int? Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "License number is required.")]
        [StringLength(50)]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        public List<int> SelectedSpecializationIds { get; set; } = new List<int>();
        public List<SpecializationOption> AvailableSpecializations { get; set; } = new List<SpecializationOption>();
    }

    public class SpecializationOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
