namespace ClinicMVC.Models.ViewModels
{
    public class DoctorFormViewModel
    {
        public int? Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
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
