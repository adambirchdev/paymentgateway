using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkout.com.BankSimulatorClient
{
    public class BankClient : IBankClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<BankClient> _logger;

        public BankClient(HttpClient client, ILogger<BankClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<BankFulfillResponse> FulfillAsync(BankFulfillRequest request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _logger.LogDebug($"Sending fulfill to bank");

            var response = await _client.PostAsync("Bank", content);

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();

            _logger.LogDebug($"Successful fulfill with bank");

            return await JsonSerializer.DeserializeAsync<BankFulfillResponse>(responseStream);
        }
    }
}
