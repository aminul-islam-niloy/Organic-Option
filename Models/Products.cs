using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OrganicOption.Models;
using System;

namespace OnlineShop.Models
{
    public enum QuantityType
    {
        Item,
        Kg,
        Liter,
        // Add more as needed
    }
    public class Products
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string Image { get; set; }

        [Required]
        [Display(Name = "Available")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Type")]
        [Required]
        public int ProductTypeId { get; set; }
        [ForeignKey("ProductTypeId")]
        public virtual ProductTypes ProductTypes { get; set; }
        [Display(Name = "Tag")]
        [Required]
        public int SpecialTagId { get; set; }
        [ForeignKey("SpecialTagId")]
        public virtual SpecialTag SpecialTag { get; set; }


        [Required]
        [DisplayName("Quantity")]
        public int Quantity { get; set; }
        public int QuantityInCart { get; set; } // Quantity of the product in the cart
        public string Description { get; set; }

        public ICollection<ProductImage> ImagesSmall { get; set; }


        [Required]
    
        [Display(Name = "Preservation Required ?")]
        public bool PreservationRequired { get; set; } // Indicates if preservation is required

        [Required]
        [Display(Name = "Quantity Type")]
        public QuantityType QuantityType { get; set; } // Quantity type: Item, Kg, Liter, etc.

        [Display(Name = "Creation Time")]
        public DateTime CreationTime { get; set; }


        [Required]
        [Display(Name = "Expiration Time")]
        public DateTime ExpirationTime { get; set; }


        [Display(Name = "Sold Quantity")]
        public int SoldQuantity { get; set; } // Tracks the quantity sold

      public DateTime LastSoldDate { get; set; }    
        public decimal Discount { get; set;}
        public decimal DiscountPrice { get; set;}

        public bool IsRamadan { get; set; }
        public bool IsEid { get; set; }
        public bool isNewCusotmer { get; set; } 

        public int FarmerShopId { get; set; }
        [ForeignKey("FarmerShopId")]
        public virtual FarmerShop FarmerShop { get; set; }

        public virtual List<InventoryItem> InventoryItems { get; set; }

    }
}
