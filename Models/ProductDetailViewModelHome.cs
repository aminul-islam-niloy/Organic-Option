using System.Collections.Generic;

namespace OnlineShop.Models
{
    public class ProductDetailViewModelHome
    {
        public Products SpecificProduct { get; set; }
        public List<Products> RelatedProducts { get; set; }
        public string ShopName { get; set; }
        public bool IsShopOpen { get; set; }
        public double OverallRatting { get; set; }  
    }
}
