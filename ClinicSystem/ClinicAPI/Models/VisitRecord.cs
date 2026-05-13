using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class VisitRecord
    {
        [Key]
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public string DoctorNotes { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string Treatment { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Prescription? Prescription { get; set; }
    }
}
