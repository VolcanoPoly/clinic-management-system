/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 7 - Visit Records & Prescriptions
 * Description : Form view model for creating a new visit record tied to a completed appointment.
 */
using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class VisitRecordCreateViewModel
    {
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

        // Prescription items to be added
        public List<PrescriptionItemViewModel> PrescriptionItems { get; set; } = new();
    }
}
