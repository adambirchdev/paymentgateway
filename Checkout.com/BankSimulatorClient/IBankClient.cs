using System.Threading.Tasks;

namespace Checkout.com.BankSimulatorClient
{
    public interface IBankClient
    {
        Task<BankFulfillResponse> FulfillAsync(BankFulfillRequest request);
    }
}
