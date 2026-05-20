/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : View model for the first booking step, listing all available specializations for the patient to choose from.
 */
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class AppointmentBookViewModel
    {
        [Required(ErrorMessage = "Please select a specialization.")]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        public List<SelectListItem> Specializations { get; set; } = new();

        // Receptionist only — which patient to book for
        public int? PatientId { get; set; }
        public List<SelectListItem> Patients { get; set; } = new();
    }
}
