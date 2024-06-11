using OnlineShop.Models;
using OrganicOption.Models.Rider_Section;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OrganicOption.Models
{
    public class FarmerShop
    {
        public int Id { get; set; } 
        [Required]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; } 
        [Required]
        [Display(Name = "Cover Photo")]
        public byte[] CoverPhoto { get; set; } 
        public bool IsShopOpen { get; set; } 
        [Required]
        [Display(Name = "Contract")]
        public string ContractInfo { get; set; }

        public Address ShopAddress { get; set; }
        public string NID { get; set; }

        public int SoldQuantity { get; set; }  
        public Decimal ShopRevenue { get; set; }
        public DateTime LastSoldDate { get; set; }

        public double Latitude { get; set; } 
        public double Longitude { get; set; } 

        public ICollection<Products> Products { get; set; } 
        public ICollection<ShopReview> Reviews { get; set; } 
        public string FarmerUserId { get; set; } // Foreign key to the ApplicationUser
        public ApplicationUser FarmerUser { get; set; } 

        public ICollection<InventoryItem> Inventory { get; set; }

        public ICollection<Order> Orders { get; set; }

        public ICollection<Delivery> Deliveries { get; set; }

    }



}
