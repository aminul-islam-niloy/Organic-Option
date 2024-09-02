using System.Collections.Generic;

namespace OrganicOption.Models.Rider_Section
{
    public class RiderDashboardViewModel
    {
        public Dictionary<int, decimal> MonthlyRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DeliveryPerformance> Performance { get; set; }
    }
}
