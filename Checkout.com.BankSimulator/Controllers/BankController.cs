using Checkout.com.BankSimulator.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Checkout.com.BankSimulator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankController : ControllerBase
    {
        [HttpPost]
        public FulFillResponse Fulfill(FulfillRequest request)
        {
            return new FulFillResponse
            {
                TransactionId = Guid.NewGuid(),
                Status = request.Amount % 5 == 0 ? Status.Failure : Status.Success
            };
        }
    }
}
