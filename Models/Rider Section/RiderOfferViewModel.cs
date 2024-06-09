using OnlineShop.Models;
using System;
using System.Collections.Generic;

namespace OrganicOption.Models.Rider_Section
{
    public class RiderOfferViewModel
    {

        public int OrderId { get; set; }
        public List<ProductwithOrderViewModel> ProductDetails { get; set; } // List of product details
        public Products Product { get; set; }
        public string CustomerAddress { get; set; }
        public Address ShopAddress { get; set; }
        public TimeSpan DeliveryTime { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public DateTime OfferStartTime { get; set; }
        public decimal Revenue { get; set; }  

        public string ShopName { get; set; } // Add shop name
        public string ShopContract { get; set; } // Add shop contract



    }
}
