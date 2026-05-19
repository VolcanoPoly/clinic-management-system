using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class VisitRecordFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        [StringLength(2000)]
        public string DoctorNotes { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Treatment { get; set; } = string.Empty;
    }
}
