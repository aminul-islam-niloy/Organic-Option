using OnlineShop.Models;
using Stripe;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OrganicOption.Models.Rider_Section
{
    public class RiderModel
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Rider Name")]

        public string RiderUserId { get; set; } // Foreign key to the ApplicationUser
        public ApplicationUser RiderUser { get; set; } // Navigation property
        public string Name { get; set; }
        [Required]
        [DisplayName("Rider Age")]
        public int Age { get; set; }
        public bool RiderStatus { get; set; }   
        public bool OnDeliaryByOffer { get; set; }   
        public string DrivingLicense { get; set; }
        [Required]
        [DisplayName("Rider NID")]
        public string NID { get; set; }
        public int RiderAddressId { get; set; } // Foreign key to RiderAddress
        public Address RiderAddress { get; set; } // Rider's address
        public string Location { get; set; }
        [Required]
        [DisplayName("Rider Phone Number")]
        public string PhoneNumber { get; set; }
        public decimal Revenue { get; set; }
        public VehicleType VehicleType { get; set; }
        public BagType BagType { get; set; }
        public ICollection<Shift> Shifts { get; set; }
        public ICollection<Delivery> Deliveries { get; set; }
    }

    public enum VehicleType
    {
        Motorcycle,
        Bicycle,
        Van,
        Truck,
        Car,
        Boat,
        Ship,
        Others
    }

    public enum BagType
    {
        MediumCarry,
        Large,
        Container
    }

}
