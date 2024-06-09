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
        public int Id { get; set; } // Primary key
        [Required]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; } // Shop Name
        [Required]
        [Display(Name = "Cover Photo")]
        public byte[] CoverPhoto { get; set; } // Cover Photo
        public bool IsShopOpen { get; set; } // Shop Open or Close
        [Required]
        [Display(Name = "Contract")]
        public string ContractInfo { get; set; }

        public Address ShopAddress { get; set; }
        public string NID { get; set; }

        public int SoldQuantity { get; set; }  
        public Decimal ShopRevenue { get; set; }
        public DateTime LastSoldDate { get; set; }

        public double Latitude { get; set; } // Latitude of the shop's location
        public double Longitude { get; set; } // Longitude of the shop's location

        // Navigation property to link this shop to a farmer

        public ICollection<Products> Products { get; set; } // Products associated with this farmer
        public ICollection<ShopReview> Reviews { get; set; } // Reviews of this farmer's shop

        public string FarmerUserId { get; set; } // Foreign key to the ApplicationUser
        public ApplicationUser FarmerUser { get; set; } // Navigation property

        public ICollection<InventoryItem> Inventory { get; set; }

        public ICollection<Order> Orders { get; set; }

        public ICollection<Delivery> Deliveries { get; set; }

        public virtual ProductTypes ShopCatagory { get; set; }

    }



}
