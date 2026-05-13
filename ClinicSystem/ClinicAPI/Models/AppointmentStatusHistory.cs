using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class AppointmentStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required]
        public AppointmentStatus OldStatus { get; set; }

        [Required]
        public AppointmentStatus NewStatus { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        [Required]
        public string ChangedByUserId { get; set; } = string.Empty;
    }
}
