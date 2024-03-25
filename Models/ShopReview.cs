using OnlineShop.Models;
using System.ComponentModel.DataAnnotations;
using System;

namespace OrganicOption.Models
{
    public class ShopReview
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your review")]
        public string Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReviewDate { get; set; }

        // Navigation property for the user who wrote the review
        public ApplicationUser User { get; set; }

        // Foreign key for the farmer being reviewed
        public string FarmerShopId { get; set; }
        public FarmerShop FarmerShop { get; set; }

    }
}
