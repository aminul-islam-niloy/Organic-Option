using OnlineShop.Models;
using System;

namespace OrganicOption.Models
{
    public class WithdrawalHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }
        public string UserType { get; set; } 
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsConfirmed { get; set; }
        public string AdminId { get; set; } 
        public ApplicationUser Admin { get; set; }
       
    }
}
