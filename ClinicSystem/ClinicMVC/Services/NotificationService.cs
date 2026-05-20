using ClinicAPI.Data;
using ClinicAPI.Models;

namespace ClinicMVC.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SendNotificationAsync(string userId, string message, int? relatedAppointmentId = null)
        {
            _db.Notifications.Add(new Notification
            {
                RecipientUserId      = userId,
                Message              = message,
                RelatedAppointmentId = relatedAppointmentId,
                IsRead               = false,
                CreatedAt            = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }
    }
}
