using AutoFixture;
using Checkout.com.BankSimulatorClient;
using Checkout.com.Controllers;
using Checkout.com.Data.Models;
using Checkout.com.Factories;
using Checkout.com.Services;
using Checkout.com.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Checkout.com.Tests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IBankClientFactory> _mockClientFactory;
        private readonly Mock<ILogger<PaymentController>> _mockLogger;
        private readonly PaymentController _sut;
        public PaymentControllerTests()
        {
            _fixture = new Fixture();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockClientFactory = new Mock<IBankClientFactory>();
            _mockLogger = new Mock<ILogger<PaymentController>>();
            _sut = new PaymentController(_mockPaymentService.Object, _mockClientFactory.Object, _mockLogger.Object);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _sut.ControllerContext = context;
        }

        [Fact]
        public async Task Fulfill_WhenCalledWithInvalidCardNumber_ThenReturnsBadRequest()
        {
            var request = GetRequest("123");
            
            var response = await _sut.Fulfill(request);

            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task Fulfill_WhenCalledHappyPath_ThenCallsInitialise()
        {
            var request = GetRequest();

            var mockBank = new Mock<IBankClient>();

            mockBank.Setup(x => x.FulfillAsync(It.IsAny<BankFulfillRequest>()))
                .ReturnsAsync(_fixture.Create<BankFulfillResponse>());

            _mockClientFactory.Setup(x => x.GetClient(It.IsAny<string>()))
                .Returns(mockBank.Object);

            await _sut.Fulfill(request);

            _mockPaymentService.Verify(x => x.InitialiseAsync(request, "testuser"), Times.Once);
        }

        [Fact]
        public async Task Fulfill_WhenCalledHappyPath_ThenCallsClientAndFactory()
        {
            var request = GetRequest();

            var mockBank = new Mock<IBankClient>();

            mockBank.Setup(x => x.FulfillAsync(It.IsAny<BankFulfillRequest>()))
                .ReturnsAsync(_fixture.Create<BankFulfillResponse>());

            _mockClientFactory.Setup(x => x.GetClient(It.IsAny<string>()))
                .Returns(mockBank.Object);

            await _sut.Fulfill(request);

            _mockClientFactory.Verify(x => x.GetClient(request.CardNumber), Times.Once);
            mockBank.Verify(x => x.FulfillAsync(It.IsAny<BankFulfillRequest>()), Times.Once);
        }


        [Fact]
        public async Task Fulfill_WhenCalledHappyPath_ThenCompletesWithFulFill()
        {
            var request = GetRequest();

            var mockBank = new Mock<IBankClient>();

            mockBank.Setup(x => x.FulfillAsync(It.IsAny<BankFulfillRequest>()))
                .ReturnsAsync(_fixture.Create<BankFulfillResponse>());

            _mockClientFactory.Setup(x => x.GetClient(It.IsAny<string>()))
                .Returns(mockBank.Object);

            await _sut.Fulfill(request);

            _mockPaymentService.Verify(x => x.FulfillAsync(It.IsAny<int>(),
                It.IsAny<PaymentStatus>(),
                It.IsAny<Guid>()));
        }

        [Fact]
        public async Task Fulfill_WhenCalledHappyPath_ThenReturnsTransactionId()
        {
            var request = GetRequest();
            var bankResponse = _fixture.Create<BankFulfillResponse>();

            var mockBank = new Mock<IBankClient>();

            mockBank.Setup(x => x.FulfillAsync(It.IsAny<BankFulfillRequest>()))
                .ReturnsAsync(bankResponse);

            _mockClientFactory.Setup(x => x.GetClient(It.IsAny<string>()))
                .Returns(mockBank.Object);


            var response = (await _sut.Fulfill(request)).Result as OkObjectResult;

            var result = response.Value as FulfillResponse;
            Assert.Equal(bankResponse.TransactionId, result.BankTransactionId);
            Assert.Equal(bankResponse.Status == BankStatus.Success, result.Status == PaymentStatus.Success);
        }

        [Fact]
        public async Task GetDetails_WhenInvalidTransactionId_ThenReturnsBadRequest() 
        {
            var request = "not a guid";

            var response = await _sut.GetDetails(request);

            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task GetDetails_WhenHappyPath_ThenCallsGet()
        {
            var request = _fixture.Create<Guid>();

            _mockPaymentService.Setup(x => x.GetAsync(request, It.IsAny<string>()))
                .ReturnsAsync(_fixture.Create<Payment>());

            await _sut.GetDetails(request.ToString());

            _mockPaymentService.Verify(x => x.GetAsync(request, "testuser"), Times.Once);
        }

        [Fact]
        public async Task GetDetails_WhenNoTransaction_ThenReturnsNotFound()
        {
            var request = _fixture.Create<Guid>();
            var payment = _fixture.Create<Payment>();

            _mockPaymentService.Setup(x => x.GetAsync(request, It.IsAny<string>()))
                .ReturnsAsync((Payment)null);

            var response = await _sut.GetDetails(request.ToString());

            Assert.IsAssignableFrom<NotFoundObjectResult>(response.Result);
        }

        [Fact]
        public async Task GetDetails_WhenHappyPath_ThenReturnsMaskedResponse()
        {
            var request = _fixture.Create<Guid>();
            var payment = _fixture.Create<Payment>();

            _mockPaymentService.Setup(x => x.GetAsync(request, It.IsAny<string>()))
                .ReturnsAsync(payment);

            var response = (await _sut.GetDetails(request.ToString())).Result as OkObjectResult;

            var result = response.Value as PaymentDetailsResponse;

            var maskedCardNumber = new string('*', payment.CardNumber.Length - 4) + 
                payment.CardNumber.Substring(payment.CardNumber.Length - 4);

            Assert.Equal(payment.Status, result.Status);
            Assert.Equal(maskedCardNumber, result.CardNumber);
            Assert.Equal(payment.ExpiryMonthYear, result.ExpiryMonthYear);
            Assert.Equal(payment.Amount, result.Amount);
            Assert.Equal(payment.CurrencyCode, result.CurrencyCode);
        }

        private FulfillRequest GetRequest(string cardNumber = "4444333322221111")
        {
            return new FulfillRequest
            {
                Amount = 5,
                CorrelationId = _fixture.Create<string>(),
                CurrencyCode = "GBP",
                CVV = "123",
                ExpiryMonthYear = "10/30",
                CardNumber = cardNumber
            };
        }
    }
}
