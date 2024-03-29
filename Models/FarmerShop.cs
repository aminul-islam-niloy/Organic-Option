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

        public double Latitude { get; set; } // Latitude of the shop's location
        public double Longitude { get; set; } // Longitude of the shop's location

        // Navigation property to link this shop to a farmer

        public ICollection<Products> Products { get; set; } // Products associated with this farmer
        public ICollection<ShopReview> Reviews { get; set; } // Reviews of this farmer's shop

        public string FarmerUserId { get; set; } // Foreign key to the ApplicationUser
        public ApplicationUser FarmerUser { get; set; } // Navigation property

        public ICollection<InventoryItem> Inventory { get; set; }


        // Method to get all products in the shop
        public IEnumerable<Products> GetAllProductsInShop()
        {
            return Inventory.Select(i => i.Product);
        }

        // Method to get sold products
        public IEnumerable<Products> GetSoldProducts()
        {
            return Inventory.Where(i => i.Quantity == 0).Select(i => i.Product);
        }

        // Method to get the last sold product
        public Products GetLastSoldProduct()
        {
            var lastSoldItem = Inventory.OrderByDescending(i => i.LastSoldDate).FirstOrDefault();
            return lastSoldItem?.Product;
        }

        // Method to get products prepared for delivery (sold products)
        public IEnumerable<Products> GetProductsPreparedForDelivery()
        {
            return GetSoldProducts();
        }

        // Method to get unsold products
        public IEnumerable<Products> GetUnsoldProducts()
        {
            return Inventory.Where(i => i.Quantity > 0).Select(i => i.Product);
        }

        // Method to calculate remaining time in product (assuming ExpirationTime property exists in Product)
        public TimeSpan GetRemainingTimeInProduct(Products product)
        {
            return product.ExpirationTime - DateTime.Now;
        }

        // Method to calculate total revenue from products
        public decimal CalculateTotalRevenue()
        {
            return Inventory.Sum(i => i.Quantity * i.Product.Price);
        }

        // Method to calculate daily, weekly, monthly, and total sold products
        public IDictionary<string, int> CalculateSalesStatistics()
        {
            var salesStatistics = new Dictionary<string, int>();

            // Sample implementation, you may need to adjust based on your requirements
            var today = DateTime.Today;
            var weeklyStartDate = today.AddDays(-7);
            var monthlyStartDate = today.AddDays(-30);

            salesStatistics.Add("Daily", Inventory.Count(i => i.LastSoldDate.Date == today));
            salesStatistics.Add("Weekly", Inventory.Count(i => i.LastSoldDate >= weeklyStartDate));
            salesStatistics.Add("Monthly", Inventory.Count(i => i.LastSoldDate >= monthlyStartDate));
            salesStatistics.Add("Total", Inventory.Sum(i => i.Quantity));

            return salesStatistics;
        }

        // Method to find the most sold product
        public Products FindMostSoldProduct()
        {
            var mostSoldItem = Inventory.OrderByDescending(i => i.Quantity).FirstOrDefault();
            return mostSoldItem?.Product;
        }

        // Method to calculate daily sales
        public int GetDailySales()
        {
            var today = DateTime.Today;
            return Inventory.Count(i => i.LastSoldDate.Date == today);
        }

        // Helper method to calculate weekly sales
        private int GetWeeklySales()
        {
            var weeklyStartDate = DateTime.Today.AddDays(-7);
            return Inventory.Count(i => i.LastSoldDate >= weeklyStartDate);
        }

        // Helper method to calculate monthly sales
        private int GetMonthlySales()
        {
            var monthlyStartDate = DateTime.Today.AddDays(-30);
            return Inventory.Count(i => i.LastSoldDate >= monthlyStartDate);
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
            decimal revenue = Inventory
                .Where(item => item.LastSoldDate >= startDate)
                .Sum(item => item.Quantity * item.Product.Price);

            return revenue;
        }

        // Method to determine best-selling shops and award bonuses
        public void AwardBonusesForBestSellingShops()
        {
            // Group inventory items by farmer shop
            var shopSales = Inventory
                .GroupBy(item => item.FarmerShopId)
                .Select(group => new
                {
                    ShopId = group.Key,
                    TotalSales = group.Sum(item => item.Quantity * item.Product.Price)
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
        // A


    }
}
