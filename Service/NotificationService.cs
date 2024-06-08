using OnlineShop.Data;
using OrganicOption.Models.Notifications;
using System.Collections.Generic;
using System;
using System.Linq;

namespace OrganicOption.Service
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Notification> GetAllNotifications()
        {
            return _context.Notifications.OrderByDescending(n => n.DateCreated).ToList();
        }

        public IEnumerable<Notification> GetUnreadNotifications()
        {
            return _context.Notifications.Where(n => !n.IsRead).OrderByDescending(n => n.DateCreated).ToList();
        }

        public void MarkAsRead(int id)
        {
            var notification = _context.Notifications.Find(id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }
        }

        public void AddNotification(string message)
        {
            var notification = new Notification
            {
                Message = message,
                DateCreated = DateTime.Now,
                IsRead = false
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public void ClearAllNotifications()
        {
            var notifications = _context.Notifications.ToList();
            _context.Notifications.RemoveRange(notifications);
            _context.SaveChanges();
        }
    }
}
