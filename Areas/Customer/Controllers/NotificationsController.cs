using Microsoft.AspNetCore.Mvc;
using OrganicOption.Service;
using System.Linq;

namespace OrganicOption.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            var notifications = _notificationService.GetAllNotifications();

            if (notifications == null || notifications.Count() == 0)
            {
                ViewBag.Message = "No notifications available.";
            }

            return View(notifications);
        }


        public IActionResult Latest()
        {
            var notifications = _notificationService.GetUnreadNotifications();
            return View(notifications);
        }

        public IActionResult MarkAsRead(int id)
        {
            _notificationService.MarkAsRead(id);
            return RedirectToAction(nameof(Latest));
        }

        [HttpGet]
        public IActionResult GetUnreadNotificationCount()
        {
            var count = _notificationService.GetUnreadNotifications().Count();
            return Json(count);
        }

        [HttpPost]
        public IActionResult ClearAllNotifications()
        {
            _notificationService.ClearAllNotifications();
            return RedirectToAction("Index");
        }
    }
}
