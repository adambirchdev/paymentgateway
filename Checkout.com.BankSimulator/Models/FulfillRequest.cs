namespace Checkout.com.BankSimulator.Models
{
    public class FulfillRequest
    {
        public string MerchantId { get; set; }

        public string ExpiryMonthYear { get; set; }

        public string CurrencyCode { get; set; }

        public string CardNumber { get; set; }

        public string CVV { get; set; }

        public decimal Amount { get; set; }
    }
}
