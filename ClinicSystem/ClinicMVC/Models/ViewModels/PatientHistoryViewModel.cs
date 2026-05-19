namespace ClinicMVC.Models.ViewModels
{
    public class PatientHistoryViewModel
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public List<VisitRecordViewModel> VisitRecords { get; set; } = new();
        public List<AppointmentHistoryItemViewModel> Appointments { get; set; } = new();
        public List<PrescriptionViewModel> Prescriptions { get; set; } = new();
    }

    public class VisitRecordViewModel
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
        public bool HasPrescription { get; set; }
        public int? PrescriptionId { get; set; }
    }

    public class AppointmentHistoryItemViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool HasVisitRecord { get; set; }
        public int? VisitRecordId { get; set; }
    }
}
