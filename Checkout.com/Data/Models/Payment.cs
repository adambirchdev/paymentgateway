using Checkout.com.Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Checkout.com.Data.Models
{
    public class Payment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public Guid BankTransactionId { get; set; }

        public string MerchantId { get; set; }

        public DateTime CreatedDate { get; set; }

        [Encrypted]
        public string ExpiryMonthYear { get; set; }

        public string CurrencyCode { get; set; }

        [Encrypted]
        public string CardNumber { get; set; }
        
        [Encrypted]
        public string CVV { get; set; }

        public decimal Amount { get; set; }
        
        public string CorrelationId { get; set; }

        public PaymentStatus Status { get; set; }
    }
}
