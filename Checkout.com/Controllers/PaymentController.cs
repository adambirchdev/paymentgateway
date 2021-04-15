using Checkout.com.BankSimulatorClient;
using Checkout.com.Factories;
using Checkout.com.Services;
using Checkout.com.Shared.Models;
using CreditCardValidator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Checkout.com.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBankClientFactory _clientFactory;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, IBankClientFactory clientFactory, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<FulfillResponse>> Fulfill(FulfillRequest request)
        {
            var detector = new CreditCardDetector(request.CardNumber);

            if (!detector.IsValid())
            {
                _logger.LogError($"Invalid Card Number | Correlation Id: {request.CorrelationId}");
                return BadRequest("Invalid card number");
            }

            var merchantId = HttpContext.User.Identity.Name;
            var id = await _paymentService.InitialiseAsync(request, merchantId);

            var bankRequest = new BankFulfillRequest
            {
                ExpiryMonthYear = request.ExpiryMonthYear,
                CardNumber = request.CardNumber,
                Amount = request.Amount,
                CurrencyCode = request.CurrencyCode,
                CVV = request.CVV,
                MerchantId = merchantId
            };

            var result = await _clientFactory.GetClient(request.CardNumber).FulfillAsync(bankRequest);

            var status = GetStatus(result.Status);
            await _paymentService.FulfillAsync(id, status, result.TransactionId);

            return Ok(new FulfillResponse
            {
                BankTransactionId = result.TransactionId,
                Status = status,
                Reason = status.ToString()
            });
        }
        
        [HttpGet]
        public async Task<ActionResult<PaymentDetailsResponse>> GetDetails(string transactionId)
        {
            var isValidId = Guid.TryParse(transactionId, out var id);

            if (!isValidId)
            {
                _logger.LogError($"Invalid Transaction Id | Transaction Id: {transactionId}");
                return BadRequest("Invalid Transaction Id");
            }
            var merchantId = HttpContext.User.Identity.Name;
            var payment = await _paymentService.GetAsync(id, merchantId);

            if (payment is null)
            {
                _logger.LogError($"Transaction not found | Transaction Id: {transactionId}");
                return NotFound("Transaction not found");
            }

            return Ok(new PaymentDetailsResponse
            {
                Status = payment.Status,
                CardNumber = payment.CardNumber,
                ExpiryMonthYear = payment.ExpiryMonthYear,
                Amount = payment.Amount,
                CurrencyCode = payment.CurrencyCode
            });
        }

        private PaymentStatus GetStatus(BankStatus status)
        {
            switch (status)
            {
                case BankStatus.Success:
                    return PaymentStatus.Success;
                default:
                    return PaymentStatus.Failure;
            }
        }
    }
}
