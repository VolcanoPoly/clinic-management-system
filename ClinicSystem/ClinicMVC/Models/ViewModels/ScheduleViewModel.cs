namespace ClinicMVC.Models.ViewModels
{
    public class ScheduleViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public List<DayScheduleViewModel> DaySchedules { get; set; } = new List<DayScheduleViewModel>();
    }

    public class DayScheduleViewModel
    {
        public int? Id { get; set; }
        public int DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string StartTimeString { get; set; } = string.Empty;
        public string EndTimeString { get; set; } = string.Empty;
    }
}
