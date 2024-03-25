using OnlineShop.Models;
using Stripe;
using System;
using System.Collections.Generic;

namespace OrganicOption.Models
{
    public class FarmerShop
    {
        public int Id { get; set; } // Primary key
        public string ShopName { get; set; } // Shop Name
        public byte[] CoverPhoto { get; set; } // Cover Photo
        public bool IsShopOpen { get; set; } // Shop Open or Close
        public string ContractInfo { get; set; }

        public double Latitude { get; set; } // Latitude of the shop's location
        public double Longitude { get; set; } // Longitude of the shop's location

        // Navigation property to link this shop to a farmer

        public ICollection<Products> Products { get; set; } // Products associated with this farmer
        public ICollection<ShopReview> Reviews { get; set; } // Reviews of this farmer's shop

        public string FarmerUserId { get; set; } // Foreign key to the ApplicationUser
        public ApplicationUser FarmerUser { get; set; } // Navigation property

    }
}
