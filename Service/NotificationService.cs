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

        public IEnumerable<Notification> GetAllNotifications(string userId)
        {
            return _context.Notifications
                           .Where(n => n.UserId == userId)
                           .OrderByDescending(n => n.DateCreated)
                           .ToList();
        }

        public IEnumerable<Notification> GetUnreadNotifications(string userId)
        {
            return _context.Notifications
                           .Where(n => n.UserId == userId && !n.IsRead)
                           .OrderByDescending(n => n.DateCreated)
                           .ToList();
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

        public void AddNotification(string userId, string message, int? productId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                DateCreated = DateTime.Now,
                IsRead = false,
                ProductId = productId
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public void ClearAllNotifications(string userId)
        {
            var notifications = _context.Notifications
                                        .Where(n => n.UserId == userId)
                                        .ToList();
            _context.Notifications.RemoveRange(notifications);
            _context.SaveChanges();
        }
    }

}
