﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using OrganicOption.Models;

namespace OnlineShop.Models
{
    public class Order
    {
        public Order()
        {
            OrderDetails = new List<OrderDetails>();
        }

        public int Id { get; set; }
        [Display(Name = "Order No")]
        public string OrderNo { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Phone No")]
        public string PhoneNo { get; set; }
        [Required]
        [EmailAddress]
        public double Latitude { get; set; } // Latitude of the Customer location
        public double Longitude { get; set; } // Longitude of the Customer location
        public string Email { get; set; }
        [Required]
        public string Address { get; set; }
        public bool FreeDelevary { get; set; }
        public decimal DelivaryCharge { get; set; }


        public DateTime OrderDate { get; set; }
        public string UserId { get; set; } // Representing the user who placed the order
        public virtual ApplicationUser User { get; set; }
        public virtual List<OrderDetails> OrderDetails { get; set; }
        public virtual List<InventoryItem> InventoryItems { get; set; }
    }
}
