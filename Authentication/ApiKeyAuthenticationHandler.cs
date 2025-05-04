using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaytentlyGateway.Services;
using System.Security.Claims;

namespace PaytentlyGateway.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = "X-API-Key";
        private readonly IApiKeyAuthenticationService _authenticationService;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyAuthenticationService authenticationService)
            : base(options, logger, encoder, clock)
        {
            _authenticationService = authenticationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.Fail("API Key was not provided");
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrEmpty(providedApiKey))
            {
                return AuthenticateResult.Fail("API Key was not provided");
            }

            var isValid = await _authenticationService.ValidateApiKey(providedApiKey);

            if (!isValid)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var apiKeyDetails = await _authenticationService.GetApiKeyDetails(providedApiKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, apiKeyDetails.MerchantId),
                new Claim(ClaimTypes.Name, apiKeyDetails.MerchantName),
                new Claim(ClaimTypes.Role, "Merchant")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
} 