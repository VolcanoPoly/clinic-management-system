using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class Specialization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public ICollection<DoctorSpecialization> Doctors { get; set; } = new List<DoctorSpecialization>();
    }
}
