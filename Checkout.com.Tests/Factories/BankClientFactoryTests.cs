using Checkout.com.BankSimulatorClient;
using Checkout.com.Factories;
using Moq;
using Xunit;

namespace Checkout.com.Tests.Factories
{
    public class BankClientFactoryTests
    {
        [Fact]
        public void GetClient_WhenCalled_ReturnsClient()
        {
            var mockClient = new Mock<IBankClient>();
            var sut = new BankClientFactory(mockClient.Object);
            
            Assert.Equal(mockClient.Object, sut.GetClient("4444333322221111"));
        }
    }
}
