using Checkout.com.BankSimulatorClient;
using CreditCardValidator;

namespace Checkout.com.Factories
{
    public class BankClientFactory : IBankClientFactory
    {
        private readonly IBankClient _client;

        public BankClientFactory(IBankClient client)
        {
            _client = client;
        }

        public IBankClient GetClient(string cardNumber)
        {
            var detector = new CreditCardDetector(cardNumber);

            switch (detector.Brand)
            {
                case CardIssuer.AmericanExpress:
                case CardIssuer.ChinaUnionPay:
                case CardIssuer.Dankort:
                case CardIssuer.DinersClub:
                case CardIssuer.Discover:
                case CardIssuer.Hipercard:
                case CardIssuer.JCB:
                case CardIssuer.Laser:
                case CardIssuer.Maestro:
                case CardIssuer.MasterCard:
                case CardIssuer.RuPay:
                case CardIssuer.Switch:
                case CardIssuer.Visa:
                case CardIssuer.Unknown:
                    return _client;
                default:
                    return _client;
            }
        }
    }
}
