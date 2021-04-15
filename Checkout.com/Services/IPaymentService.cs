using Checkout.com.Data.Models;
using Checkout.com.Shared.Models;
using System;
using System.Threading.Tasks;

namespace Checkout.com.Services
{
    public interface IPaymentService
    {
        Task<int> InitialiseAsync(FulfillRequest request, string merchantId);
        
        Task FulfillAsync(int id, PaymentStatus status, Guid bankTransactionId);

        Task<Payment> GetAsync(Guid transactionId, string merchantId);
    }
}
