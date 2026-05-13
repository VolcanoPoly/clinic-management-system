using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class DoctorLeave
    {
        [Key]
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
