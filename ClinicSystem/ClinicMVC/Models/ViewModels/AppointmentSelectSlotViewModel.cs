/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : View model for the third booking step, exposing available 30-minute time slots for the selected doctor and date.
 */
using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class AppointmentSelectSlotViewModel
    {
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        // Passed through (receptionist booking)
        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please select a time slot.")]
        public string SelectedSlot { get; set; } = string.Empty; // "HH:mm"

        public List<string> AvailableSlots { get; set; } = new();

        public string? Notes { get; set; }
    }
}
