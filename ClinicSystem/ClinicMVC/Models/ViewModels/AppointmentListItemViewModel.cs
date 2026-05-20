/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : Lightweight projection used to display appointments in list/table views across all role dashboards.
 */
using ClinicAPI.Models;

namespace ClinicMVC.Models.ViewModels
{
    public class AppointmentListItemViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
