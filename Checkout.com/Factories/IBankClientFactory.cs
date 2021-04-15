using Checkout.com.BankSimulatorClient;

namespace Checkout.com.Factories
{
    public interface IBankClientFactory
    {
        IBankClient GetClient(string cardNumber);
    }
}
