using System.Collections.Generic;

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
        public string FullAddress()
        {
            var addressParts = new List<string>();

            if (!string.IsNullOrEmpty(House))
                addressParts.Add("House: " + House);

            if (!string.IsNullOrEmpty(StreetNo))
                addressParts.Add("Street No: " + StreetNo);

            if (!string.IsNullOrEmpty(WardNo))
                addressParts.Add("Ward No: " + WardNo);

            if (!string.IsNullOrEmpty(Thana))
                addressParts.Add("Thana: " + Thana);

            if (!string.IsNullOrEmpty(District))
                addressParts.Add("District: " + District);

            if (!string.IsNullOrEmpty(Division))
                addressParts.Add("Division: " + Division);

            return string.Join(", ", addressParts);
        }
    }
}
