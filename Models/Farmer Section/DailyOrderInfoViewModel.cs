using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;

namespace OrganicOption.Models.Farmer_Section
{
    public class DailyOrderInfoViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public List<ProductInfo> Products { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderTme { get; set; }
    }
}
