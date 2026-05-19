using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class PrescriptionViewModel
    {
        public int Id { get; set; }
        public int VisitRecordId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public List<PrescriptionItemViewModel> Items { get; set; } = new();
    }

    public class PrescriptionCreateViewModel
    {
        public int VisitRecordId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public List<PrescriptionItemViewModel> Items { get; set; } = new();
    }

    public class PrescriptionItemViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string MedicationName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Duration { get; set; } = string.Empty;

        [StringLength(500)]
        public string Instructions { get; set; } = string.Empty;
    }
}
