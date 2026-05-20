/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : View model for the second booking step, listing doctors that belong to the chosen specialization along with their available slot count.
 */
using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class AppointmentSelectDoctorViewModel
    {
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;

        // Passed through from step 1 (receptionist booking)
        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Please select a doctor.")]
        public int SelectedDoctorId { get; set; }

        public List<DoctorOption> Doctors { get; set; } = new();
    }

    public class DoctorOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public List<string> SpecializationNames { get; set; } = new();
        public DateTime? NextAvailableDate { get; set; }
    }
}
