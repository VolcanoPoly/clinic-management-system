using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        public int VisitRecordId { get; set; }
        public VisitRecord? VisitRecord { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.Now;

        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    }
}
