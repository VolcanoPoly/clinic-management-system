namespace ClinicMVC.Models.ViewModels
{
    public class LeaveViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public List<DoctorLeaveItem> Leaves { get; set; } = new List<DoctorLeaveItem>();
        public DoctorLeaveItem NewLeave { get; set; } = new DoctorLeaveItem();
    }

    public class DoctorLeaveItem
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string StartDateString { get; set; } = string.Empty;
        public string EndDateString { get; set; } = string.Empty;
    }
}
