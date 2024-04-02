using OnlineShop.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public int SoldQuantity { get; set; }  
        public Decimal ShopRevenue { get; set; }
        public DateTime LastSoldDate { get; set; }  

        //public  Address  Address {get;set;}

        public double Latitude { get; set; } // Latitude of the shop's location
        public double Longitude { get; set; } // Longitude of the shop's location

        // Navigation property to link this shop to a farmer

        public ICollection<Products> Products { get; set; } // Products associated with this farmer
        public ICollection<ShopReview> Reviews { get; set; } // Reviews of this farmer's shop

        public string FarmerUserId { get; set; } // Foreign key to the ApplicationUser
        public ApplicationUser FarmerUser { get; set; } // Navigation property

        public ICollection<InventoryItem> Inventory { get; set; }

        // Method to get unsold products
      

        // Method to calculate remaining time in product (assuming ExpirationTime property exists in Product)
        public TimeSpan GetRemainingTimeInProduct(Products product)
        {
            return product.ExpirationTime - DateTime.Now;
        }

        // Method to calculate total revenue from products
        public decimal CalculateTotalRevenue()
        {
            return Products.Sum(p => p.Quantity * p.Price);
        }

        // Method to calculate daily, weekly, monthly, and total sold products
   

        // Method to find the most sold product
        public Products FindMostSoldProduct()
        {
            return Products.OrderByDescending(p => p.Quantity).FirstOrDefault();
        }

    

        public decimal CalculateAndCashoutRevenue(CashoutInterval interval)
        {
            // Calculate revenue for the specified interval
            decimal revenue = CalculateRevenueForInterval(interval);

            // Deduct platform fee (3%)
            decimal platformFee = revenue * 0.03m;
            decimal remainingRevenue = revenue - platformFee;

            // Perform cashout (not implemented here)

            // Return remaining revenue after platform fee deduction
            return remainingRevenue;
        }

        // Method to calculate revenue for the specified interval
        private decimal CalculateRevenueForInterval(CashoutInterval interval)
        {
            DateTime startDate;
            switch (interval)
            {
                case CashoutInterval.Weekly:
                    startDate = DateTime.Today.AddDays(-7);
                    break;
                case CashoutInterval.Biweekly:
                    startDate = DateTime.Today.AddDays(-15);
                    break;
                case CashoutInterval.Monthly:
                    startDate = DateTime.Today.AddMonths(-1);
                    break;
                default:
                    throw new ArgumentException("Invalid cashout interval");
            }

            // Calculate revenue based on the specified interval
            decimal revenue = Products
                .Where(p => p.LastSoldDate >= startDate)
                .Sum(p => p.Quantity * p.Price);

            return revenue;
        }

        // Method to determine best-selling shops and award bonuses
        public void AwardBonusesForBestSellingShops()
        {
            // Group products by farmer shop
            var shopSales = Products
                .GroupBy(p => p.FarmerShopId)
                .Select(group => new
                {
                    ShopId = group.Key,
                    TotalSales = group.Sum(p => p.Quantity * p.Price)
                });

            // Determine the shop with the highest sales
            var bestSellingShop = shopSales.OrderByDescending(shop => shop.TotalSales).FirstOrDefault();

            if (bestSellingShop != null)
            {
                // Award bonus to the best selling shop (for demonstration, add 10% bonus)
                decimal bonus = bestSellingShop.TotalSales * 0.1m; // 10% bonus
                // Perform bonus awarding (not implemented here)
            }
        }
    }

    // Enum to represent cashout intervals
    public enum CashoutInterval
    {
        Weekly,
        Biweekly,
        Monthly
    }


}
