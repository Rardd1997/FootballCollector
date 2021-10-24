using Football.Collector.Common.Models;
using Football.Collector.Data.Context;
using Football.Collector.Data.Models;
using Football.Collector.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Football.Collector.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly FootballCollectorDbContext dbContext;
        private readonly IEncryptionService encryptionService;
        private readonly ITokenService tokenService;

        public AuthController(FootballCollectorDbContext dbContext, IEncryptionService encryptionService, ITokenService tokenService)
        {
            this.dbContext = dbContext;
            this.encryptionService = encryptionService;
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request?.Username) || string.IsNullOrEmpty(request?.Password))
            {
                return BadRequest("username and password must be specified");
            }

            var passHash = encryptionService.GetPasswordHash(request.Password);
            var validUser = await GetValidUserAsync(request.Username, request.Password);
            if (validUser == null)
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, validUser.Id),
                new Claim(ClaimTypes.Name, request.Username)
            };

            var accessToken = tokenService.GenerateAccessToken(claims);

            return Ok(new LoginResponse { AccessToken = accessToken });
        }
        protected async Task<ServiceUser> GetValidUserAsync(string username, string clearPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(clearPassword))
            {
                return null;
            }

            var user = await dbContext.ServiceUsers.Where(p => p.Username == username).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            var hashPassword = user.Password;

            try
            {
                var isValidPassword = encryptionService.ValidatePassword(clearPassword, hashPassword);
                return isValidPassword ? user : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
