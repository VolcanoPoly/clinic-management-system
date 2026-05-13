using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class PrescriptionItem
    {
        [Key]
        public int Id { get; set; }

        public int PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }

        [Required]
        public string MedicationName { get; set; } = string.Empty;

        [Required]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        public string Duration { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;
    }
}
