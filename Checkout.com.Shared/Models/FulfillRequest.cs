using System.ComponentModel.DataAnnotations;

namespace Checkout.com.Shared.Models
{
    public class FulfillRequest
    {
        [Required]
        [RegularExpression("[0-1][0-9]/[2-9][0-9]")]
        public string ExpiryMonthYear { get; set; }

        [Required]
        [RegularExpression("[A-Z]{3}")]
        public string CurrencyCode { get; set; }
        
        [Required]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression("[0-9]{3}")]
        public string CVV { get; set; }

        [Required]
        [RegularExpression("[0-9]+(.[0-9][0-9]?)?")]
        public decimal Amount { get; set; }

        [Required]
        public string CorrelationId { get; set; }
    }
}
