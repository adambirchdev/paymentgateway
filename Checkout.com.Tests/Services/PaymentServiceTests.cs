using AutoFixture;
using Checkout.com.Data;
using Checkout.com.Data.Models;
using Checkout.com.Services;
using Checkout.com.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Checkout.com.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly Mock<ILogger<PaymentService>> _mockLogger;
        private PaymentService _sut;

        public PaymentServiceTests()
        {
            _fixture = new Fixture();

            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseInMemoryDatabase(databaseName: "Pay")
              .Options;

            _mockLogger = new Mock<ILogger<PaymentService>>();
        }

        [Fact]
        public async Task InitialiseAsync_WhenPaymentExistsAndNotPending_ThenThrowsException()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var request = GetRequest();

                SetData(context, new List<Payment> { new Payment { CorrelationId = request.CorrelationId, Status = PaymentStatus.Success } });

                await Assert.ThrowsAsync<Exception>(async () => await _sut.InitialiseAsync(request, _fixture.Create<string>()));
            }
        }

        [Fact]
        public async Task InitialiseAsync_WhenPaymentExistsAndPending_ThenReturnsId()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var request = GetRequest();
                var id = _fixture.Create<int>();

                SetData(context, new List<Payment> 
                { 
                    new Payment 
                    { 
                        Id = id,
                        CorrelationId = request.CorrelationId, 
                        Status = PaymentStatus.Pending 
                    }
                });

                var result = await _sut.InitialiseAsync(request, _fixture.Create<string>());
                Assert.Equal(id, result);
            }
        }

        [Fact]
        public async Task InitialiseAsync_WhenHappyPath_ThenCreatesPendingPayment()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var request = GetRequest();

                SetData(context, new List<Payment>());

                var merchantId = _fixture.Create<string>();

                await _sut.InitialiseAsync(request, merchantId);

                context.Payments.Any(x => x.Amount == request.Amount &&
                        x.CardNumber == request.CardNumber &&
                        x.CreatedDate == DateTime.Now &&
                        x.CorrelationId == request.CorrelationId &&
                        x.CurrencyCode == request.CurrencyCode &&
                        x.CVV == request.CVV &&
                        x.ExpiryMonthYear == request.ExpiryMonthYear &&
                        x.MerchantId == merchantId &&
                        x.Status == PaymentStatus.Pending);
            }
        }

        [Fact]
        public async Task InitialiseAsync_WhenHappyPath_ThenReturnsId()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var request = GetRequest();

                SetData(context, new List<Payment>());

                var merchantId = _fixture.Create<string>();

                var result = await _sut.InitialiseAsync(request, merchantId);

                Assert.True(result > 0);
            }
        }

        [Fact]
        public async Task FulfillAsync_WhenDoesNotExists_ThenThrowsException()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var request = GetRequest();

                SetData(context, new List<Payment>());

                await Assert.ThrowsAsync<Exception>(async () => await _sut.FulfillAsync(
                    _fixture.Create<int>(), _fixture.Create<PaymentStatus>(), _fixture.Create<Guid>()));
            }
        }

        [Fact]
        public async Task FulfillAsync_WhenPaymentNotPending_ThenReturnsId()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var payment = new Payment
                {
                    Id = _fixture.Create<int>(),
                    Status = PaymentStatus.Success
                };

                SetData(context, new List<Payment> { payment });

                await Assert.ThrowsAsync<Exception>(async () => await _sut.FulfillAsync(
                    payment.Id, _fixture.Create<PaymentStatus>(), _fixture.Create<Guid>()));
            }
        }

        [Fact]
        public async Task FulfillAsync_WhenHappyPath_ThenUpdatesStatusAndTransactionId()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var payment = new Payment
                {
                    Id = _fixture.Create<int>(),
                    Status = PaymentStatus.Pending
                };
                var transactionId = _fixture.Create<Guid>();

                SetData(context, new List<Payment> { payment });

                await _sut.FulfillAsync(payment.Id, PaymentStatus.Success, transactionId);

                context.Payments.Any(x => x.Id == payment.Id &&
                    x.BankTransactionId == transactionId &&
                    x.Status == PaymentStatus.Success);
            }
        }

        [Fact]
        public async Task GetAsync_WhenExists_ThenReturnsPayment()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                _sut = new PaymentService(context, _mockLogger.Object);

                var payment = _fixture.Create<Payment>();
            
                SetData(context, new List<Payment> { payment });

                var result = await _sut.GetAsync(payment.BankTransactionId, payment.MerchantId);

                Assert.Equal(payment, result);
            }
        }

        private void SetData(ApplicationDbContext context, List<Payment> payments)
        {
            context.AddRange(payments);
            context.SaveChanges();
        }

        private FulfillRequest GetRequest()
        {
            return new FulfillRequest
            {
                Amount = 5,
                CorrelationId = _fixture.Create<string>(),
                CurrencyCode = "GBP",
                CVV = "123",
                ExpiryMonthYear = "10/30",
                CardNumber = "4444333322221111"
            };
        }
    }
}
