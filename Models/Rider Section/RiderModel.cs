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
        public string RiderUserId { get; set; }
        public ApplicationUser RiderUser { get; set; }
        public int RiderAddressId { get; set; } 
        public Address RiderAddress { get; set; }
       
        [Required]
        [DisplayName("Rider Phone Number")]
        public string PhoneNumber { get; set; }
        public decimal Revenue { get; set; }
        public decimal RiderDue { get; set; }
        public VehicleType VehicleType { get; set; }
        public BagType BagType { get; set; }
        public ICollection<Shift> Shifts { get; set; }
        public ICollection<Delivery> Deliveries { get; set; }

        public ICollection<WithdrawalHistory> WithdrawalHistories { get; set; }
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
