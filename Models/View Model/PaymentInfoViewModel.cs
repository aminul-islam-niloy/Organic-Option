using OnlineShop.Models;

namespace OrganicOption.Models.View_Model
{
    public class PaymentInfoViewModel
    {
        public string PaymentId { get; set; }
        public PaymentMethods PaymentMethods { get; set; }
        public double Amount { get; set; }

        public PaymentCondition PaymentCondition { get; set; }
    }
}
