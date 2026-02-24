using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MyApp.API.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string AuthorizationHeaderName = "Authorization";
        private readonly string _expectedAuthValue;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            _expectedAuthValue = configuration["Authentication:ApiKey"]
                ?? throw new InvalidOperationException("Authentication:ApiKey configuration is required.");
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(AuthorizationHeaderName, out var authorizationHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            var authHeaderValue = authorizationHeader.ToString();

            if (!ConstantTimeEquals(authHeaderValue, _expectedAuthValue))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header Value"));
            }

            var claims = new[] {
                new Claim(ClaimTypes.Name, "AuthenticatedUser"),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            var bytesA = Encoding.UTF8.GetBytes(a);
            var bytesB = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
        }
    }
}
