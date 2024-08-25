using OrganicOption.Models;
using System;
using System.Collections.Generic;

namespace OnlineShop.Models
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public string  OrderNo { get; set; }
        public string CustomerName { get; set; } 
        public string CustomerAddress { get; set; }
        public Address CuAddress { get; set; }
        public Address SpAddress { get; set; }
        public Address RpAddress { get; set; }
        public string CustomerPhone { get; set; } 
        public string CustomerEmail { get; set; } 
        public DateTime OrderDate { get; set; }
        public DateTime OrderAccept {  get; set; }  
        public List<ProductViewModel> Products { get; set; }
        public string UserId { get; set; } 
        public string UserName { get; set; }
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public int RiderId { get; set; }  
        public string RiderName { get; set; }   
        public string RiderPhone { get; set; }  
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string ProductColor { get; set; }
        public int Quantity { get; set; }
        public string CardInfo { get; set; }    
        public PaymentMethods PaymentMethods { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PayableMoney { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal DelivaryCharge { get; set; }
        public decimal TotalDelivaryCharge { get; set; }
        public OrderCondition OrderCondition { get; set; }  

    }
}
