
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using OrganicOption.Models;
using OrganicOption.Models.Blogs;
using OrganicOption.Models.Notifications;
using OrganicOption.Models.Rider_Section;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineShop.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ProductTypes> ProductTypes { get; set; }
        public DbSet<SpecialTag> SpecialTag { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
  
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<FarmerShop> FarmerShop { get; set; }
        public DbSet<ShopReview> ShopReview { get; set; }
        public DbSet<Favorite> Favorite { get; set; }

        public DbSet<InventoryItem> InventoryItem { get; set; }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<RiderModel> RiderModel { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<WithdrawalHistory> withdrawalHistories { get; set; }

        public DbSet<FeedBack> FeedBack { get; set; }   
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }

    }
}
