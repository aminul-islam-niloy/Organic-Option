using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Stripe;
using OnlineShop.Models;

namespace OnlineShop.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }
        [Display(Name = "Order")]
        public int OrderId { get; set; }
        [Display(Name = "Product")]
        public int PorductId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        [ForeignKey("PorductId")]
        public Products Product { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal TotalDelivaryCharge { get; set; }

        public bool RequiresPreservation { get; set; }
        public string StripeSessionId { get; set; }

        public PaymentMethods PaymentMethods { get; set; }

        public PaymentCondition PaymentCondition { get; set; }

        public OrderCondition OrderCondition { get; set; }
    }
      public enum MobileBanking
    {
        BKash,
        Roket,
        Nagad,
        UPay,
        Selfin
    }

    public enum PaymentMethods
    {
        CashOnDelivery,
        Card,
        MobileBanking
    }

    public enum PaymentCondition
    {
        UnPaid,
        Paid
    
    }

    public enum OrderCondition
    {
        Onlist,
        OrderTaken,
        Processing,
        OnDelivary,
        Delivered,

    }
}



