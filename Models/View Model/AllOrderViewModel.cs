using OnlineShop.Models;
using System;

namespace OrganicOption.Models.View_Model
{
    public class AllOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderCondition OrderCondition { get; set; }
        public double DeliveryCharge { get; set; }
        public double TotalPrice { get; set; }
        public CustomerInfoViewModel Customer { get; set; }
        public RiderInfoViewModel Rider { get; set; }
        public FarmerShopInfoViewModel FarmerShop { get; set; }
        public PaymentInfoViewModel Payment { get; set; }
    }
}
