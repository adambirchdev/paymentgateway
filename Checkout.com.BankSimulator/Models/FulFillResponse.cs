using System;

namespace Checkout.com.BankSimulator.Models
{
    public class FulFillResponse
    {
        public Guid TransactionId { get; set; }

        public Status Status { get; set; }
    }
}
