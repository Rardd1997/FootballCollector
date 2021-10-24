using Football.Collector.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Football.Collector.Services
{
    public class TokenService : ITokenService
    {
        private readonly string appSecret;
        private readonly TokenValidationParameters tokenValidationParameters;

        public TokenService(string appSecret, TokenValidationParameters tokenValidationParameters)
        {
            this.appSecret = appSecret;
            this.tokenValidationParameters = tokenValidationParameters;
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims, DateTime? expires = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSecret);

            var claimsDictionary = new Dictionary<string, object>();

            foreach (var claim in claims)
            {
                claimsDictionary.Add(claim.Type, claim.Value);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = tokenValidationParameters.ValidAudience,
                Issuer = tokenValidationParameters.ValidIssuer,
                Subject = new ClaimsIdentity(claims),
                Claims = claimsDictionary,
                Expires = expires ?? DateTime.UtcNow.AddDays(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var newtokenValidationParameters = tokenValidationParameters.Clone();
            newtokenValidationParameters.ValidateLifetime = false;

            if (!tokenHandler.CanReadToken(token))
            {
                throw new SecurityTokenException("Invalid token");
            }

            var principal = tokenHandler.ValidateToken(token, newtokenValidationParameters, out SecurityToken securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
