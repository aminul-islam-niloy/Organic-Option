using System;

namespace OrganicOption.Models.Notifications
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsRead { get; set; }

        public string UserId { get; set; } 
        public int? ProductId { get; set; }
        public int OrderId { get; set; }
    }
}
