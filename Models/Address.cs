namespace OrganicOption.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Division { get; set; }
        public string District { get; set; }
        public string Thana { get; set; }
        public string WardNo { get; set; }
        public string StreetNo{ get; set; }
        public string House { get; set; }
        public double Latitude { get; set; } // Latitude coordinate

        public double Longitude { get; set; } // Longitude coordinate
    }
}
