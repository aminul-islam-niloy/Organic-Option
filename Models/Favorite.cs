using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OrganicOption.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public int FarmerShopId { get; set; }


        [ForeignKey("FarmerShopId")]
        public FarmerShop FarmerShop { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
