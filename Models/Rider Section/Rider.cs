using Stripe;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OrganicOption.Models.Rider_Section
{
    public class Rider
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Rider Name")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Rider Age")]
        public int Age { get; set; }
        public string DrivingLicense { get; set; }
        [Required]
        [DisplayName("Rider NID")]
        public string NID { get; set; }
        public int RiderAddressId { get; set; } // Foreign key to RiderAddress
        public Address RiderAddress { get; set; } // Rider's address
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
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
        Car,
        Bicycle,
        Van,
        Truck,
        Others
    }

    public enum BagType
    {
        MediumCarry,
        Large
    }

}
