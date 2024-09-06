using OrganicOption.Models.Rider_Section;
using System;
using System.Collections.Generic;

namespace OnlineShop.Models
{
    public class UserOrdersViewModel
    {
  
    public string UserName { get; set; }
        public string UserPhone { get; set; }

        // Order information
        public int OrderId { get; set; }
        public string  OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }

        public ApplicationUser User { get; set; }
        public List<Order> Orders { get; set; }
        public PaymentMethods PaymentMethod { get; set; }
        public OrderCondition OrderCondition { get; set; }
        public double DeliveryCharge { get; set; }

        public int ShopId { get; set; }
        // Order details including product information
        public List<OrderDetailsViewModel> OrderDetails { get; set; }

    }
}
