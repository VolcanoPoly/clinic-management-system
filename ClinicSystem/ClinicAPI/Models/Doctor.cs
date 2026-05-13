using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAPI.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public string LicenseNumber { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public ICollection<DoctorSpecialization> Specializations { get; set; } = new List<DoctorSpecialization>();
        public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
        public ICollection<DoctorLeave> Leaves { get; set; } = new List<DoctorLeave>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
