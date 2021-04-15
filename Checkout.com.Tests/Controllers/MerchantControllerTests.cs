using AutoFixture;
using Checkout.com.Controllers;
using Checkout.com.Services;
using Moq;
using Xunit;

namespace Checkout.com.Tests.Controllers
{
    public class MerchantControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly MerchantController _sut;
        public MerchantControllerTests()
        {
            _fixture = new Fixture();
            _mockJwtService = new Mock<IJwtService>();
            _sut = new MerchantController(_mockJwtService.Object);
        }

        [Fact]
        public void Get_WhenCalled_ThenCallsJwtServiceOnce()
        {
            var name = _fixture.Create<string>();
            _sut.Get(name);

            _mockJwtService.Verify(x => x.GenerateSecurityToken(name), Times.Once);
        }

        [Fact]
        public void Get_WhenCalled_ThenReturnsJwtServiceResponse()
        {
            var name = _fixture.Create<string>();
            var encrypted = _fixture.Create<string>();
            _mockJwtService.Setup(x => x.GenerateSecurityToken(name)).Returns(encrypted);
            var response = _sut.Get(name);

            Assert.Equal(encrypted, response);
        }
    }
}
