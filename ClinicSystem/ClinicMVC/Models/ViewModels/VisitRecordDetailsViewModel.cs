using ClinicAPI.Models;

namespace ClinicMVC.Models.ViewModels
{
    public class VisitRecordDetailsViewModel
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public string DoctorNotes { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Prescription details if exists
        public PrescriptionViewModel? Prescription { get; set; }
    }

    public class VisitRecordListItemViewModel
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool HasPrescription { get; set; }
    }
}
