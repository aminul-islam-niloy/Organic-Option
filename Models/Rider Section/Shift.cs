using System;

namespace OrganicOption.Models.Rider_Section
{
    public class Shift
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RiderId { get; set; }
        public Rider Rider { get; set; }
    }
}
