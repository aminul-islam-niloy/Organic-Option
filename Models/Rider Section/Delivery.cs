﻿
using OnlineShop.Models;
using System.Collections.Generic;

namespace OrganicOption.Models.Rider_Section
{
    public class Delivery
    {
        public int Id { get; set; }
        public string ProductDetails { get; set; }
        public string CustomerAddress { get; set; }
        public string DelivyAddress { get; set; }

        public OrderCondition OrderCondition { get; set; }
        public int RiderId { get; set; }
        public Rider Rider { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int FarmerShopId { get; set; }
        public FarmerShop FarmerShop { get; set; }
    }
}
