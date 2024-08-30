using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganicOption.Service;
using System.Linq;
using System.Security.Claims;

namespace OrganicOption.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = _notificationService.GetAllNotifications(userId);

            if (notifications == null || !notifications.Any())
            {
                ViewBag.Message = "No notifications available.";
            }

            return View(notifications);
        }

        public IActionResult Latest()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = _notificationService.GetUnreadNotifications(userId);
            return View(notifications);
        }

        [HttpGet]
        public IActionResult GetLatest()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = _notificationService.GetUnreadNotifications(userId);

            // Return the notifications as a partial view
            return PartialView("_LatestNotifications", notifications);
        }

        public IActionResult MarkAsRead(int id)
        {
            _notificationService.MarkAsRead(id);
            return RedirectToAction(nameof(Latest));
        }

        [HttpGet]
        public IActionResult GetUnreadNotificationCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = _notificationService.GetUnreadNotifications(userId).Count();
            return Json(count);
        }

        public IActionResult ClearAllNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _notificationService.ClearAllNotifications(userId);
            return RedirectToAction("Index");
        }
        //[HttpGet]
        //public IActionResult GetPopLatest()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var notifications = _notificationService.GetUnreadNotifications(userId);

        //    // Mark the notifications as read
        //    foreach (var notification in notifications)
        //    {
        //        _notificationService.MarkAsRead(notification.Id);
        //    }

        //    var notificationData = notifications.Select(n => new {
        //        n.Id,
        //        n.Message,  
        //        n.DateCreated
        //    }).ToList();

        //    return Json(notificationData);
        //}


    }
}
