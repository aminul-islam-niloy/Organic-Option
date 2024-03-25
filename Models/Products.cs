﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OrganicOption.Models;

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


 
        [Display(Name = "Preservation Required")]
        public bool PreservationRequired { get; set; } // Indicates if preservation is required


        [Display(Name = "Quantity Type")]
        public QuantityType QuantityType { get; set; } // Quantity type: Item, Kg, Liter, etc.

       

        [Display(Name = "Sold Quantity")]
        public int SoldQuantity { get; set; } // Tracks the quantity sold


        [NotMapped] // Not mapped to database
        public decimal DiscountedPrice
        {
            get
            {
                decimal discountedPrice = Price; // Default to original price

                // Apply discounts based on conditions
                if (IsRegularCustomer)
                {
                    // Apply 5% discount for regular customers for at least 7 days
                    if (OrderDayCont >= 7)
                    {
                        discountedPrice -= 0.05M * Price; // 5% off
                    }
                }

                // Check if it's Ramadan and apply discount
                if (IsRamadan)
                {
                    discountedPrice -= 0.15M * Price; // 15% off during Ramadan
                }

                // Check if it's Eid and apply discount
                if (IsEid)
                {
                    discountedPrice -= 0.10M * Price; // 10% off during Eid
                }

                return discountedPrice;
            }
        }

        // Additional properties to track customer status
        [NotMapped] // Not mapped to database
        public bool IsRegularCustomer { get; set; }

        [NotMapped] // Not mapped to database
        public int OrderDayCont { get; set; }

        [NotMapped] // Not mapped to database
        public bool IsRamadan { get; set; }

        [NotMapped] // Not mapped to database
        public bool IsEid { get; set; }

        public int FarmerShopId { get; set; }
        [ForeignKey("FarmerShopId")]
        public virtual FarmerShop FarmerShop { get; set; }


    }
}
