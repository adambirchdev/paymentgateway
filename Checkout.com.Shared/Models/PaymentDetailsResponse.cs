namespace Checkout.com.Shared.Models
{
    public class PaymentDetailsResponse
    {
        private string _cardNumber { get; set; }

        public string ExpiryMonthYear { get; set; }

        public string CardNumber
        { 
            get 
            {
                var end = _cardNumber.Substring(_cardNumber.Length - 4);
                return new string('*', _cardNumber.Length - 4) + end;
            } 
            set { _cardNumber = value; } 
        }

        public string CurrencyCode { get; set; }

        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }
    }
}
