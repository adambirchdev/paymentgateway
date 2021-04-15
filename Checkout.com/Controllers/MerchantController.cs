using Checkout.com.Services;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.com.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantController : ControllerBase
    {
        private readonly IJwtService _jwtService;

        public MerchantController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpGet]
        public string Get(string name)
        {
            return _jwtService.GenerateSecurityToken(name);
        }
    }
}
