namespace ClinicMVC.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message, int? relatedAppointmentId = null);
    }
}
