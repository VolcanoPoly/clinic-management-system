using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public int SpecializationId { get; set; }
        public Specialization? Specialization { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;

        public string Notes { get; set; } = string.Empty;

        public string CancellationReason { get; set; } = string.Empty;

        public ICollection<AppointmentStatusHistory> StatusHistory { get; set; } = new List<AppointmentStatusHistory>();
        public VisitRecord? VisitRecord { get; set; }
    }
}
