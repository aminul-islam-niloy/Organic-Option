using OnlineShop.Models;
using System;

namespace OrganicOption.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // Foreign key to the product
        public virtual Products Product { get; set; } // Navigation property to the product
        public int Quantity { get; set; }
        public DateTime LastSoldDate { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPrice { get; set; }
        public int FarmerShopId { get; set; } // Foreign key to the farmer shop
        public virtual FarmerShop FarmerShop { get; set; } // Navigation property to the farmer shop





    }
}
