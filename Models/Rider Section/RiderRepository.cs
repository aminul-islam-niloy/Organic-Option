using OnlineShop.Data;
using System.Collections.Generic;
using System.Linq;
using OnlineShop.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace OrganicOption.Models.Rider_Section
{
    public class RiderRepository
    {
        private readonly ApplicationDbContext _context;

        public RiderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Dictionary<int, decimal> GetMonthlyRevenue(int riderId)
        {
      
            var deliveries = _context.Deliveries
                .Where(d => d.RiderId == riderId && d.OrderCondition == OrderCondition.Delivered)
                .Select(d => new
                {
                    d.OrderAcceptTime.Month,
                    d.Order.DelivaryCharge 
                })
                .ToList();

        
            var monthlyRevenue = deliveries
                .GroupBy(d => d.Month)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(d => d.DelivaryCharge) 
                );

            return monthlyRevenue;
        }

        public decimal GetTotalRevenue(int riderId)
        {
            var totalRevenue = _context.Deliveries
                .Where(d => d.RiderId == riderId && d.OrderCondition == OrderCondition.Delivered)
                .Sum(d => d.Order.DelivaryCharge);

            return totalRevenue;
        }

        public List<DeliveryPerformance> GetRiderPerformance(int riderId)
        {
            return _context.Deliveries
                .Where(d => d.RiderId == riderId && d.OrderCondition == OrderCondition.Delivered)
                .GroupBy(d => new { d.OrderAcceptTime.Year, d.OrderAcceptTime.Month })
                .Select(g => new DeliveryPerformance
                {
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                    CompletedDeliveries = g.Count(),
                    EarnedRevenue = g.Sum(d => d.Order.DelivaryCharge)
                })
                .OrderBy(r => r.Month)
                .ToList();
        }

    }
}
