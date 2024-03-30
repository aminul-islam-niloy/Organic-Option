using OnlineShop.Models;
using System;

namespace OrganicOption.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime LastSoldDate { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPrice { get; set; }
        public int FarmerShopId { get; set; } // Foreign key to the farmer shop
        public virtual FarmerShop FarmerShop { get; set; } // Navigation property to the farmer shop
        public int ProductId { get; set; }
        public virtual Products Products { get; set; }
        public decimal Price { get; set; }
        public int? OrderId { get; set; } // Nullable reference to the order ID
        public virtual Order Order { get; set; } // Navigation property to the order


    }
}
