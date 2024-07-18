using System;

namespace OrganicOption.Models
{
    public class ShopReviewViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public int Rating { get; set; }
    }
}
