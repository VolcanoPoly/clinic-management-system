namespace ClinicMVC.Models.ViewModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RelatedAppointmentId { get; set; }
    }
}
