namespace ClinicMVC.Models.ViewModels
{
    public class DoctorViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public List<string> SpecializationNames { get; set; } = new List<string>();
    }
}
