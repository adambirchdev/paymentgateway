using System;

namespace Checkout.com.Shared.Models
{
    public class FulfillResponse
    {
        public Guid BankTransactionId { get; set; }

        public PaymentStatus Status { get; set; }

        public string Reason { get; set; }
    }
}
