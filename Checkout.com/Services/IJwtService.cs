namespace Checkout.com.Services
{
    public interface IJwtService
    {
        string GenerateSecurityToken(string name);
    }
}
