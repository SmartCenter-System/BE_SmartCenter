using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SmartCenter.Service.JwtService;

public interface IJwtService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    ClaimsPrincipal ValidateToken(string token);
}