using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaytentlyGateway.Services;
using System.Security.Claims;

namespace PaytentlyTestGateway.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiKeyAuthenticationService _apiKeyService;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyAuthenticationService apiKeyService)
            : base(options, logger, encoder, clock)
        {
            _apiKeyService = apiKeyService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeaderValues))
            {
                return AuthenticateResult.Fail("API Key header is missing");
            }

            var apiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
            {
                return AuthenticateResult.Fail("API Key is empty");
            }

            var apiKeyInfo = await _apiKeyService.ValidateApiKey(apiKey);
            if (apiKeyInfo == null)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, apiKeyInfo.MerchantName),
                new Claim(ClaimTypes.NameIdentifier, apiKeyInfo.MerchantId),
                new Claim("MerchantId", apiKeyInfo.MerchantId),
                new Claim(ClaimTypes.Role, "Merchant")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
} 