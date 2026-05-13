using System.ComponentModel.DataAnnotations;

namespace ClinicAPI.Models
{
    public class DoctorSchedule
    {
        [Key]
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
