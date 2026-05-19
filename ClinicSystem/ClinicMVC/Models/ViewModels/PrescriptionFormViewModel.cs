using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class PrescriptionFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int VisitRecordId { get; set; }

        public List<PrescriptionItemFormViewModel> Items { get; set; } = new();
    }

    public class PrescriptionItemFormViewModel
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
