using System;
using System.Text.Json.Serialization;

namespace Checkout.com.BankSimulatorClient
{
    public class BankFulfillResponse
    {
        [JsonPropertyName("transactionId")]
        public Guid TransactionId { get; set; }

        [JsonPropertyName("status")]
        public BankStatus Status { get; set; }
    }
}
