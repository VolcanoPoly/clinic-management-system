using ClinicAPI.Data;
using ClinicAPI.Models;
using ClinicMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicMVC.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _db.Notifications
                .Where(n => n.RecipientUserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationViewModel
                {
                    Id                   = n.Id,
                    Message              = n.Message,
                    IsRead               = n.IsRead,
                    CreatedAt            = n.CreatedAt,
                    RelatedAppointmentId = n.RelatedAppointmentId
                })
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.RecipientUserId == user.Id);

            if (notification != null)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unread = await _db.Notifications
                .Where(n => n.RecipientUserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var count = await _db.Notifications
                .CountAsync(n => n.RecipientUserId == user.Id && !n.IsRead);

            return Json(new { count });
        }
    }
}
