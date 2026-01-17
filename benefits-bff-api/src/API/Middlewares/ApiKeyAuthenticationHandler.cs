using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Schema;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Sunny.Benefits.Bff.Api.Middlewares
{
    [ExcludeFromCodeCoverage]
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ITenantClient _tenantClient;
        private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ITenantClient tenantClient)
            : base(options, loggerFactory, encoder)
        {
            _tenantClient = tenantClient;
            _logger = loggerFactory.CreateLogger<ApiKeyAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Only process if X_API_KEY header is present
            if (!Request.Headers.TryGetValue(HttpHeaderNames.XAPIKey, out var apiKeyHeader))
            {
                _logger.LogError("Missing X_API_KEY header.");
                return AuthenticateResult.NoResult();
            }

            var allowedPaths = new[]
                {
                    CommonConstants.ConsumerSummaryAPIUrl,
                    CommonConstants.WalletsAPIUrl,
                    CommonConstants.CardOperationUrl,
                    CommonConstants.VerifyMemberInfoUrl,
                    CommonConstants.CardOperationReissueUrl,
                    CommonConstants.CardStatusUrl,
                    CommonConstants.SubscriptionAPIUrl
                };

            var requestPath = Request.Path.Value;

            // Skip authentication if path is not allowed
            if (!allowedPaths.Contains(requestPath, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogError("Path not authenticated via XAPI");
                return AuthenticateResult.NoResult(); 
            }

            bool isValid = await IsValidApiKey(apiKeyHeader!);

            if (!isValid)
            {
                _logger.LogWarning("Invalid API key provided.");
                return AuthenticateResult.Fail("Xapi key not matched");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
            var identity = new ClaimsIdentity(claims, nameof(ApiKeyAuthenticationHandler));
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private async Task<bool> IsValidApiKey(string userApiKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userApiKey))
                    return false;
                bool apiKey = await _tenantClient.Post<dynamic>("tenant/validate-api-key", userApiKey);
                if (!apiKey)
                {
                    _logger.LogWarning("API key validation failed in tenant service.");
                    return false;
                }

                _logger.LogInformation("API key validation succeeded in tenant service.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating API key.");
                return false;
            }
        }
    }

}
