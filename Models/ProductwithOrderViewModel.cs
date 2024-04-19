using System.Collections.Generic;

namespace OrganicOption.Models
{
    public class ProductwithOrderViewModel
    {
       
            public string ProductName { get; set; } // Product name
            public int Quantity { get; set; } // Product description
            public decimal Price { get; set; } // Product price
            public string ProductImage { get; set; } // Product image URL
            public string ShopName { get; set; } // Shop name
            public Address ShopAddress { get; set; } // Shop address
            public string ShopContact { get; set; } // Shop contact information

    }
}
