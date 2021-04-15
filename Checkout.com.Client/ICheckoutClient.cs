using Checkout.com.Shared.Models;
using System.Threading.Tasks;

namespace Checkout.com.Client
{
    public interface ICheckoutClient
    {
        Task<FulfillResponse> Fulfill(FulfillRequest request);

        Task<PaymentDetailsResponse> GetDetails(string transactionId);
    }
}
