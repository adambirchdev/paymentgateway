using Checkout.com.Shared.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkout.com.Client
{
    public class CheckoutClient : ICheckoutClient
    {
        private readonly HttpClient _client;

        public CheckoutClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<FulfillResponse> Fulfill(FulfillRequest request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("Payment", content);

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<FulfillResponse>(responseStream);
        }

        public async Task<PaymentDetailsResponse> GetDetails(string transactionId)
        {
            var response = await _client.GetAsync($"Payment/{transactionId}");

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<PaymentDetailsResponse>(responseStream);
        }
    }
}
