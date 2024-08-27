
using OnlineShop.Models;
using System;
using System.Collections.Generic;

namespace OrganicOption.Models.Rider_Section
{
    public class Delivery
    {
        public int Id { get; set; }
        public string ProductDetails { get; set; }
        public string CustomerAddress { get; set; }
        public string DelivyAddress { get; set; }
        public string CustomerPhone { get; set; }

        public OrderCondition OrderCondition { get; set; }
        public PaymentCondition PaymentCondition { get; set; }
        public decimal PayableMoney { get; set; }
        public int RiderId { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        //public string ShopAddress { get; set; }
        //public Address ShopAddress { get; set; }

        //public string CustomerPreAdd { get; set; }
        //public Address CustomerPreAdd { get; set; }

  
        public Address ShopCurrentAddress {get;set ;}
        public Address CustomerCurrentAddress {get; set; }
        

        public string ShopName { get; set; }    
        public string ShopContract { get; set; }
        public DateTime OrderDeliveredDate { get; set; }
        public DateTime OrderAcceptTime { get; set; }   
        public double ShopLat { get; set; }
        public double ShopLon { get; set; }
        public double DeliveryLat { get; set; }
        public double DeliveryLon { get; set; }

    }
}
