/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 6 - Appointment Lifecycle Management
 * Description : Full appointment detail view model including status history timeline and the list of allowed next-state transitions for the current user role.
 */
using ClinicAPI.Models;

namespace ClinicMVC.Models.ViewModels
{
    public class AppointmentDetailViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientUserId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorUserId { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
        public List<StatusHistoryItemViewModel> StatusHistory { get; set; } = new();

        // Actions available to the current user for this appointment
        public List<AppointmentStatus> AllowedTransitions { get; set; } = new();

        // Visit record info
        public bool HasVisitRecord { get; set; }
        public int? VisitRecordId { get; set; }
    }

    public class StatusHistoryItemViewModel
    {
        public AppointmentStatus OldStatus { get; set; }
        public AppointmentStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
    }
}
