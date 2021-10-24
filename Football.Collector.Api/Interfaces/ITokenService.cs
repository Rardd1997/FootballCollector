using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Football.Collector.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims, DateTime? expires = null);
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}
