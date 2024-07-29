using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using iLib.src.main.IDAO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace iLib.src.main.rest
{
    [method: Obsolete("ISystemClock is obsolete, use TimeProvider on AuthenticationSchemeOptions instead.")]
    public class CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserDao userDao) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
    {
        private readonly IUserDao _userDao = userDao;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));

            if (!JwtHelper.ValidateToken(token))
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));

            var email = JwtHelper.GetEmailFromToken(token);
            var user = _userDao.FindUserByEmail(email);
            if (user == null)
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("email", email ?? string.Empty),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
