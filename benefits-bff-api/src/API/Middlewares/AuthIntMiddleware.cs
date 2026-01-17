using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Sunny.Benefits.Bff.Api.Middlewares
{
    [ExcludeFromCodeCoverage]
    public class AuthIntMiddleware
    {
        private readonly IVault _vault;
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthIntMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string className = nameof(AuthIntMiddleware);

        public AuthIntMiddleware(IVault vault, RequestDelegate next, ILogger<AuthIntMiddleware> logger, IServiceProvider serviceProvider)
        {
            _vault = vault;
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async System.Threading.Tasks.Task InvokeAsync(HttpContext httpContext)
        {
            const string methodName = nameof(InvokeAsync);
            string requestUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - API triggered: {RequestUrl}", className, methodName, requestUrl);

                // Check if the environment is production and skip this middleware

                string secretEnv = await _vault.GetSecret(CommonConstants.Env);
                if (!string.IsNullOrEmpty(secretEnv) && secretEnv.Equals(EnvironmentConstants.Production, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Skipping AuthIntMiddleware in production. Request URL: {RequestUrl}",
                        className, methodName, requestUrl);

                    await _next(httpContext); // Pass to next middleware
                    return;
                }

                // Skip validation for certain API paths
                if (IsSkippedPath(httpContext.Request.Path))
                {
                    httpContext.Items[HttpContextKeys.IsAuthorized] = true;
                    await _next(httpContext);
                    return;
                }

                if (!httpContext.Request.Headers.TryGetValue(HttpHeaderNames.Authint, out var authintToken) || string.IsNullOrWhiteSpace(authintToken))
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - '{Authint}' header missing. Passing request to Authorization middleware. Request URL: {RequestUrl}",
                        className, methodName, HttpHeaderNames.Authint, requestUrl);

                    await _next(httpContext);
                    return;
                }


                // Enable buffering to read request body multiple times
                var request = httpContext.Request;
                request.EnableBuffering();

                BaseRequestDto? baseRequest = await GetRequestBodyAsync(request);

                JwtSecurityToken? tokenS = ReadToken(authintToken);
                if (tokenS == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid or malformed JWT token. Request URL: {RequestUrl}",
                        className, methodName, requestUrl);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }

                string? jwtConsumerCode = tokenS.Payload?.GetValueOrDefault("consumer_code")?.ToString();
                string? jwtTenantCode = tokenS.Payload?.GetValueOrDefault("tenant_code")?.ToString();
                string? tokenEnv = tokenS.Payload?.GetValueOrDefault("env")?.ToString();
                string? jwtEmail = tokenS.Payload?.GetValueOrDefault("email")?.ToString();
                string? personUniqueIdentifier = tokenS.Payload?.GetValueOrDefault("person_unique_identifier")?.ToString();
                var isSSOUserValue = tokenS.Payload?.GetValueOrDefault("is_sso_user")?.ToString();

                bool isSSOUser = bool.TryParse(isSSOUserValue, out var parsedBool) && parsedBool;
                httpContext.Items[HttpContextKeys.IsSsoUser] = isSSOUser;
                httpContext.Items[HttpContextKeys.JwtConsumerCode] = jwtConsumerCode;
                httpContext.Items[HttpContextKeys.TenantCode] = jwtTenantCode;
                httpContext.Items[HttpContextKeys.Email] = jwtEmail;
                httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = personUniqueIdentifier;
                using var scope = _serviceProvider.CreateScope();
                var _auth0Helper = scope.ServiceProvider.GetRequiredService<IAuth0Helper>();
                bool success = await _auth0Helper.SetAuthConfigToContext(httpContext);
                if (!success)
                {
                    return;
                }
                if (tokenEnv == secretEnv)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Token environment matches. Request authorized. Request URL: {RequestUrl}", className, methodName, requestUrl);
                    httpContext.Items[HttpContextKeys.IsAuthorized] = true;
                    await _next(httpContext);
                    return;
                }

                if (baseRequest?.consumerCode != null && (tokenEnv != secretEnv || jwtConsumerCode != baseRequest.consumerCode))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Unauthorized request. Consumer code mismatch. Request URL: {RequestUrl}", className, methodName, requestUrl);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Unexpected error occurred while processing request. Request URL: {RequestUrl}",
                    className, methodName, requestUrl);
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            await _next(httpContext);
        }

        private JwtSecurityToken? ReadToken(string? authintToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authintToken))
                {
                    return null;
                }

                authintToken = authintToken.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                var handler = new JwtSecurityTokenHandler();
                return handler.ReadToken(authintToken) as JwtSecurityToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error parsing JWT token. Token: {AuthintToken}",
        className, nameof(ReadToken), authintToken?.Substring(0, Math.Min(authintToken.Length, 10))); // Log only first 10 chars for safety
                return null;
            }
        }

        private static bool IsSkippedPath(string path)
        {
            return path.Contains("internal-login");
        }

        private async Task<BaseRequestDto?> GetRequestBodyAsync(HttpRequest request)
        {
            if (request.Method != HttpMethods.Post && request.Method != HttpMethods.Put && request.Method != HttpMethods.Patch)
                return null;

            if (request.ContentLength == null || request.ContentLength <= 0)
                return null;

            try
            {
                using var reader = new StreamReader(request.Body, encoding: System.Text.Encoding.UTF8, leaveOpen: true);
                string body = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset the stream for further processing

                if (request.ContentType != null && request.ContentType.StartsWith("multipart/form-data"))
                {
                    var formCollection = await request.ReadFormAsync();
                    return new BaseRequestDto { consumerCode = formCollection["ConsumerCode"].ToString() };
                }

                return JsonConvert.DeserializeObject<BaseRequestDto>(body);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed to deserialize request body. Request URL: {RequestUrl}",
                    className, nameof(GetRequestBodyAsync), request.Path);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Unexpected error occurred while reading request body. Request URL: {RequestUrl}",
                    className, nameof(GetRequestBodyAsync), request.Path);
                return null;
            }
        }
    }
}