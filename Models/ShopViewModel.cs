using OnlineShop.Models;
using System.Collections.Generic;

namespace OrganicOption.Models
{
    public class ShopViewModel
    {
        public List<Products> products { get; set; }
        public List<Products> Fruits { get; set; }
        public List<Products> Vegetable { get; set; }
        public List<Products> Dairy { get; set; }
        public List<Products> Pets { get; set; }
        public List<Products> Fish { get; set; }
        public List<Products> Meat { get; set; }
        public List<Products> Crops { get; set; }
        public List<Products> Cattle { get; set; }
        public List<Products> Bird { get; set; }
        public List<Products> Honey { get; set; }


        public List<ShopReview> TopRatedShop { get; set; }
        public List<Products> DiscountProduct { get; set; }
        public List<Products> TopSelling { get; set; }
        public List<FarmerShop> BestSeller { get; set; }
        public FarmerShop FarmerShop { get; set; }

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string UserName { get; set; }
        public List<ShopReview> Reviews { get; set; }
    }
}




