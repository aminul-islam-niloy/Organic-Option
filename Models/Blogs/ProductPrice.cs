using OnlineShop.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrganicOption.Models.Blogs
{
    public class ProductPrice
    {
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int ProductTypeId { get; set; }
        public ProductTypes ProductType { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
