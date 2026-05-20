/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : Carries the final booking summary (doctor, date/time, specialization) shown to the patient before they confirm.
 */
namespace ClinicMVC.Models.ViewModels
{
    public class AppointmentConfirmViewModel
    {
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public int? PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty; // for receptionist display

        public DateTime AppointmentDateTime { get; set; }
        public string? Notes { get; set; }
    }
}
