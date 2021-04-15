using Checkout.com.Data;
using Checkout.com.Data.Models;
using Checkout.com.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Checkout.com.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentService> _logger;
        public PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> InitialiseAsync(FulfillRequest request, string merchantId)
        {
            var existing = await _context.Payments.FirstOrDefaultAsync(x => x.CorrelationId == request.CorrelationId);
            if (existing is null)
            {
                var payment = new Payment
                {
                    Amount = request.Amount,
                    CardNumber = request.CardNumber,
                    CreatedDate = DateTime.Now,
                    CorrelationId = request.CorrelationId,
                    CurrencyCode = request.CurrencyCode,
                    CVV = request.CVV,
                    ExpiryMonthYear = request.ExpiryMonthYear,
                    MerchantId = merchantId,
                    Status = PaymentStatus.Pending
                };
                _context.Add(payment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Payment Initilised | Correlation Id: {request.CorrelationId}");

                return payment.Id;
            }
            else if (existing.Status != PaymentStatus.Pending)
            {
                _logger.LogCritical($"Payment has already been completed | Correlation Id: {request.CorrelationId}");
                throw new Exception("Payment has already been completed");
            }

            return existing.Id;
        }

        public async Task FulfillAsync(int id, PaymentStatus status, Guid bankTransactionId)
        {
            var existing = await _context.Payments.FirstOrDefaultAsync(x => x.Id == id);
            if (existing is null)
            {
                _logger.LogCritical($"Payment does not exist | Transaction Id: {bankTransactionId}");
                throw new Exception("Payment does not exist");
            }

            if (existing.Status != PaymentStatus.Pending)
            {
                _logger.LogCritical($"Payment already processed | Transaction Id: {bankTransactionId}");
                throw new Exception("Payment already processed");
            }

            existing.Status = status;
            existing.BankTransactionId = bankTransactionId;
            _context.Update(existing);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Payment Completed | Transaction Id: {bankTransactionId}");
        }

        public Task<Payment> GetAsync(Guid transactionId, string merchantId)
        {
            return _context.Payments.FirstOrDefaultAsync(x => x.BankTransactionId == transactionId && x.MerchantId == merchantId);
        }
    }
}
