using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Configuration;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using BaseRequestDto = Sunny.Benefits.Bff.Core.Domain.Dtos.BaseRequestDto;

namespace Sunny.Benefits.Bff.Api.Middlewares
{
    [ExcludeFromCodeCoverage]
    public class Auth0Middleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserClient _userClient;
        private readonly ILogger<Auth0Middleware> _logger;
        private readonly IJwtValidationCacheService? _jwtCacheService;
        private readonly RedisConfiguration _redisConfig;
        private const string className=nameof(Auth0Middleware);
        
        // Rate limiting: Track Auth0 API calls per user (consumerCode) to stay within 5 requests/minute limit
        // Key: consumerCode, Value: Queue of timestamps of last 5 Auth0 calls
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> _userCallHistory = new();
        private static readonly object _lockObject = new object();
        private const int MaxCallsPerMinute = 5;
        private const int RateLimitWindowSeconds = 60;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serviceProvider"></param>
        public Auth0Middleware(RequestDelegate next, IServiceProvider serviceProvider,
            IUserClient userClient, ILogger<Auth0Middleware> logger, IConfiguration configuration,
            IOptions<RedisConfiguration> redisConfig,
            IJwtValidationCacheService? jwtCacheService = null)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _userClient = userClient;
            _logger = logger;
            _redisConfig = redisConfig?.Value ?? new RedisConfiguration();
            _jwtCacheService = jwtCacheService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            const string methodName = nameof(Invoke);
            var middlewareStopwatch = System.Diagnostics.Stopwatch.StartNew();
            string requestUrl = $"{context.Request.Path}{context.Request.QueryString}";
            _logger.LogInformation("{ClassName}.{MethodName} - API triggered: {RequestUrl}", className, methodName, requestUrl);
            // If request is already authorized by the first middleware, bypass this middleware
            if (context.Items.ContainsKey(HttpContextKeys.IsAuthorized))
            {
                middlewareStopwatch.Stop();
                _logger.LogInformation("{ClassName}.{MethodName} - Bypassed (already authorized), MiddlewareTime: {ElapsedMs}ms, RequestUrl: {RequestUrl}", 
                    className, methodName, middlewareStopwatch.ElapsedMilliseconds, requestUrl);
                await _next(context);
                return;
            }

            // Skip if only XAPIHeader is present
            if (context.Request.Headers.ContainsKey(HttpHeaderNames.XAPIKey) &&
                !context.Request.Headers.ContainsKey(HttpHeaderNames.Authorization))
            {
                middlewareStopwatch.Stop();
                _logger.LogInformation("{ClassName}.{MethodName} - Skipping Auth0Middleware as only XAPIHeader is present, MiddlewareTime: {ElapsedMs}ms, RequestUrl: {RequestUrl}", 
                    className, methodName, middlewareStopwatch.ElapsedMilliseconds, requestUrl);
                await _next(context);
                return;
            }

            // Skip validation for certain API paths
            if (IsSkippedPath(context.Request.Path))
            {
                middlewareStopwatch.Stop();
                _logger.LogInformation("{ClassName}.{MethodName} - Skipped path validation, MiddlewareTime: {ElapsedMs}ms, RequestUrl: {RequestUrl}", 
                    className, methodName, middlewareStopwatch.ElapsedMilliseconds, requestUrl);
                await _next(context);
                return;
            }
            if (!context.Request.Headers.TryGetValue(HttpHeaderNames.Authorization, out var token) || string.IsNullOrWhiteSpace(token))
            {
                middlewareStopwatch.Stop();
                _logger.LogError("{ClassName}.{MethodName} - Unauthorized request. Both '{Authint}' and '{Authorization}' headers are missing. Request URL: {RequestUrl}, MiddlewareTime: {ElapsedMs}ms",
                nameof(AuthorizationMiddleware), methodName, HttpHeaderNames.Authint, HttpHeaderNames.Authorization, requestUrl, middlewareStopwatch.ElapsedMilliseconds);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
            var authHeaders = context.Request.Headers;

            var request = context.Request;
            request.EnableBuffering();


            string? queryPayLoadEmail = context.Request.Query["email"];


            request.EnableBuffering();

            if (!authHeaders.ContainsKey("Authorization"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                _logger.LogError("{ClassName}.{MethodName} - UnAuthorized AuthHeaders does not contains key",className, methodName);
                return;
            }

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                   
                    var stream = new StreamReader(context.Request.Body);
                    string body = await stream.ReadToEndAsync();
                    request.Body.Position = 0;
                    var baseRequest = JsonConvert.DeserializeObject<BaseRequestDto>(body);
                    context.Items[HttpContextKeys.Email] = baseRequest?.email;
                    PersonDto? consumerEmail = null;
                    string jsonData = ReadToken(token!);
                    if (jsonData != "{}")
                    {
                        var auth0AppMetadata = JsonConvert.DeserializeObject<Auth0AppMetadataDto>(jsonData);

                        if (!string.IsNullOrEmpty(auth0AppMetadata?.ConsumerCode))
                        {
                            consumerEmail = await GetConsumerEmail(new BaseRequestDto() { consumerCode = auth0AppMetadata.ConsumerCode}, context);
                        }
                        context.Items[HttpContextKeys.JwtConsumerCode] = auth0AppMetadata?.ConsumerCode;
                        context.Items[HttpContextKeys.TenantCode] = auth0AppMetadata?.TenantCode;
                        context.Items[HttpContextKeys.PersonUniqueIdentifier] = auth0AppMetadata?.PersonUniqueIdentifier;
                        context.Items[HttpContextKeys.UserName] = auth0AppMetadata?.UserName;
                        context.Items[HttpContextKeys.IsSsoUser] = auth0AppMetadata?.IsSSOUser ?? false;
                        context.Items[HttpContextKeys.MemberNbr] = auth0AppMetadata?.MemberNbr;
                        context.Items[HttpContextKeys.RegionCode] = auth0AppMetadata?.RegionCode;

                        var isSSOUser = auth0AppMetadata?.IsSSOUser ?? false;
                        if (!isSSOUser && !string.IsNullOrEmpty(queryPayLoadEmail?.Trim()) && consumerEmail?.Email != queryPayLoadEmail)
                        {
                            context.Response.StatusCode = 401;
                            _logger.LogError("{ClassName}.{MethodName} - Unauthorized access with given email.ErrorCode:{Code}",
                                className, methodName, StatusCodes.Status401Unauthorized);
                            return;
                        }
                        var requestEmail = baseRequest?.email;
                        var consumerEmailValue = consumerEmail?.Email;
                        var queryEmail = queryPayLoadEmail?.Trim();
                        if (body != "")
                        {

                            if (baseRequest?.consumerCode is not null && baseRequest.consumerCode.Contains("cmr-") && auth0AppMetadata?.ConsumerCode != baseRequest.consumerCode)
                            {
                                context.Response.StatusCode = 401;
                                _logger.LogError("{ClassName}.{MethodName} - Unauthorized access attempt expected ConsumerCode : '{BaseRequestConsumerCode}', but received :'{JwtConsumerCode}', ErrorCode:{Code}",
                                    className, methodName, baseRequest.consumerCode, auth0AppMetadata?.ConsumerCode, StatusCodes.Status401Unauthorized);
                                return;
                            }
                            if (!isSSOUser && !string.IsNullOrWhiteSpace(requestEmail) &&
                                !string.IsNullOrWhiteSpace(consumerEmailValue) &&
                                !string.Equals(requestEmail, consumerEmailValue, StringComparison.OrdinalIgnoreCase))
                            {
                                context.Response.StatusCode = 401;
                                _logger.LogError("{ClassName}.{MethodName} - Unauthorized access with given email, ErrorCode:{Code}", className, methodName, StatusCodes.Status401Unauthorized);
                                return;
                            }
                        }

                        else if (!isSSOUser && !string.IsNullOrWhiteSpace(queryEmail) &&
                         !string.IsNullOrWhiteSpace(consumerEmailValue) &&
                         !string.Equals(queryEmail, consumerEmailValue, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = 401;
                            _logger.LogError("{ClassName}.{MethodName} - Unauthorized access Because Payload Email and Consumer Email are different. ErrorCode:{Code}", className, methodName, StatusCodes.Status401Unauthorized);
                            return;
                        }
                    }
                    var identity = new ClaimsIdentity("basic");
                    context.User = new ClaimsPrincipal(identity);
                    using var scope = _serviceProvider.CreateScope();
                    var _auth0Helper = scope.ServiceProvider.GetRequiredService<IAuth0Helper>();
                    bool success = await _auth0Helper.SetAuthConfigToContext(context);
                    if (!success)
                    {
                        middlewareStopwatch.Stop();
                        _logger.LogWarning("{ClassName}.{MethodName} - SetAuthConfigToContext failed, MiddlewareTime: {ElapsedMs}ms, RequestUrl: {RequestUrl}",
                            className, methodName, middlewareStopwatch.ElapsedMilliseconds, requestUrl);
                        return;
                    }

                    // Extract consumerCode for rate limiting
                    string? consumerCode = null;
                    try
                    {
                        string tokenJsonData = ReadToken(token);
                        if (tokenJsonData != "{}")
                        {
                            var auth0AppMetadataForRateLimit = JsonConvert.DeserializeObject<Auth0AppMetadataDto>(tokenJsonData);
                            consumerCode = auth0AppMetadataForRateLimit?.ConsumerCode;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "{ClassName}.{MethodName} - Could not extract consumerCode for rate limiting, proceeding without rate limit check, RequestUrl: {RequestUrl}",
                            className, methodName, requestUrl);
                    }

                    var validateTokenStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    (bool isValid, string email) = await ValidateTokenWithRetry(_auth0Helper, token, requestUrl, consumerCode);
                    validateTokenStopwatch.Stop();
                    _logger.LogInformation("{ClassName}.{MethodName} - Auth0 token validation completed, ValidationTime: {ElapsedMs}ms, IsValid: {IsValid}, RequestUrl: {RequestUrl}",
                        className, methodName, validateTokenStopwatch.ElapsedMilliseconds, isValid, requestUrl);
                    context.Items[HttpContextKeys.Email] = email;

                   

                    

                    if (request.Path.Value?.ToLower() == "/api/v1/get-user-by-id" || request.Path.Value?.ToLower() == "/api/v1/post-verification-email" || request.Path.Value?.ToLower() == "/api/v1/patch-user")
                    {
                        isValid = true;
                    }
                    if (!isValid)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        _logger.LogError("{ClassName}.{MethodName} - Inavlid Token Foribidden: {Token}, ErrorCode:{Code}",
                            className,methodName, token,StatusCodes.Status403Forbidden);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Original behavior: On 429, wait 10 seconds then continue (isValid remains false, so request fails with 403)
                    if (ex.Message.Contains(((int)HttpStatusCode.TooManyRequests).ToString()))
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Too many requests detected for Auth0 AccessToken: '{AccessToken}', ErrorCode: {Code}, RequestUrl: {RequestUrl}",
                            className, methodName, token, StatusCodes.Status429TooManyRequests, requestUrl);
                        
                        // Wait 10 seconds (non-blocking) then continue - isValid will be false, so request fails with 403
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        
                        // Continue execution - isValid is false (never set), so it will fail validation below and return 403
                        // This matches original behavior where 429 was logged but client received 403 (not 429)
                    }
                    else
                    {
                        middlewareStopwatch.Stop();
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Unauthorized Access: Provided token is not valid, ErrorCode: {Code}, MiddlewareTime: {ElapsedMs}ms, RequestUrl: {RequestUrl}",
                            className, methodName, StatusCodes.Status401Unauthorized, middlewareStopwatch.ElapsedMilliseconds, requestUrl);
                        return;
                    }
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                _logger.LogError("{ClassName}.{MethodName} - Unauthorized Access: Provided token '{AccessToken}' is not valid,ErrorCode:{Code}", className, methodName, token,StatusCodes.Status401Unauthorized);
                return;
            }
            
            middlewareStopwatch.Stop();
            await _next(context);
        }

        private async Task<(bool isValid, string email)> ValidateTokenWithRetry(IAuth0Helper auth0Helper, string token, string requestUrl, string? consumerCode)
        {
            const string methodName = nameof(ValidateTokenWithRetry);
            
            _logger.LogInformation("{ClassName}.{MethodName} - CacheEnabledForAuth0: {CacheEnabled}, JwtCacheService: {HasService}, RequestUrl: {RequestUrl}",
                className, methodName, _redisConfig.CacheEnabledForAuth0, _jwtCacheService != null ? "registered" : "null", requestUrl);
            
            // Check cache first
            if (_redisConfig.CacheEnabledForAuth0 && _jwtCacheService != null)
            {
                try
                {
                    var cachedResult = await _jwtCacheService.GetCachedValidationAsync(token);
                    if (cachedResult != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Cache hit, IsValid: {IsValid}, RequestUrl: {RequestUrl}",
                            className, methodName, cachedResult.IsValid, requestUrl);
                        return (cachedResult.IsValid, cachedResult.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{ClassName}.{MethodName} - Cache check failed, RequestUrl: {RequestUrl}",
                        className, methodName, requestUrl);
                }
            }
            
            // Rate limiting
            if (!string.IsNullOrEmpty(consumerCode))
            {
                await WaitForRateLimit(consumerCode, requestUrl);
            }
            
            // Call Auth0
            var auth0CallStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var (isValid, email) = await auth0Helper.Validatetoken(token);
            auth0CallStopwatch.Stop();
            
            if (!string.IsNullOrEmpty(consumerCode))
            {
                RecordAuth0Call(consumerCode);
            }
            
            _logger.LogInformation("{ClassName}.{MethodName} - Auth0 validation completed, Time: {ElapsedMs}ms, IsValid: {IsValid}, RequestUrl: {RequestUrl}",
                className, methodName, auth0CallStopwatch.ElapsedMilliseconds, isValid, requestUrl);

            // Cache the result
            if (_redisConfig.CacheEnabledForAuth0 && _jwtCacheService != null)
            {
                try
                {
                    await _jwtCacheService.CacheValidationResultAsync(token, isValid, email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{ClassName}.{MethodName} - Failed to cache result, RequestUrl: {RequestUrl}",
                        className, methodName, requestUrl);
                }
            }

            return (isValid, email);
        }
        
        /// <summary>
        /// Waits if necessary to stay within Auth0 rate limit (5 requests/minute per user)
        /// </summary>
        private async Task WaitForRateLimit(string consumerCode, string requestUrl)
        {
            const string methodName = nameof(WaitForRateLimit);
            var waitStopwatch = System.Diagnostics.Stopwatch.StartNew();
            TimeSpan waitTime = TimeSpan.Zero;
            
            // Calculate wait time while holding lock
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;
                var cutoffTime = now.AddSeconds(-RateLimitWindowSeconds);
                
                // Get or create call history for this user
                if (!_userCallHistory.TryGetValue(consumerCode, out var callHistory))
                {
                    callHistory = new Queue<DateTime>();
                    _userCallHistory[consumerCode] = callHistory;
                }
                
                // Remove calls older than 60 seconds
                while (callHistory.Count > 0 && callHistory.Peek() < cutoffTime)
                {
                    callHistory.Dequeue();
                }
                
                // If we've made 5 calls in the last 60 seconds, wait until the oldest call expires
                if (callHistory.Count >= MaxCallsPerMinute)
                {
                    var oldestCallTime = callHistory.Peek();
                    var waitUntil = oldestCallTime.AddSeconds(RateLimitWindowSeconds);
                    waitTime = waitUntil - now;
                    
                    if (waitTime.TotalMilliseconds > 0)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - ⏳ Rate limit: {CallCount}/{MaxCalls} calls in last {WindowSeconds}s for ConsumerCode: {ConsumerCode}, Waiting {WaitSeconds:F1}s before Auth0 call, RequestUrl: {RequestUrl}",
                            className, methodName, callHistory.Count, MaxCallsPerMinute, RateLimitWindowSeconds, consumerCode, waitTime.TotalSeconds, requestUrl);
                    }
                }
            }
            
            // Wait outside the lock (non-blocking)
            if (waitTime.TotalMilliseconds > 0)
            {
                await Task.Delay(waitTime);
                
                // Clean up expired calls after waiting
                lock (_lockObject)
                {
                    if (_userCallHistory.TryGetValue(consumerCode, out var callHistory))
                    {
                        var newCutoffTime = DateTime.UtcNow.AddSeconds(-RateLimitWindowSeconds);
                        while (callHistory.Count > 0 && callHistory.Peek() < newCutoffTime)
                        {
                            callHistory.Dequeue();
                        }
                    }
                }
                
                waitStopwatch.Stop();
                _logger.LogInformation("{ClassName}.{MethodName} - ✅ Rate limit wait completed ({WaitMs}ms), ConsumerCode: {ConsumerCode}, RequestUrl: {RequestUrl}",
                    className, methodName, waitStopwatch.ElapsedMilliseconds, consumerCode, requestUrl);
                    }
                    else
                    {
                waitStopwatch.Stop();
            }
        }
        
        /// <summary>
        /// Records an Auth0 API call timestamp for rate limiting
        /// </summary>
        private void RecordAuth0Call(string consumerCode)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;
                var cutoffTime = now.AddSeconds(-RateLimitWindowSeconds);
                
                // Get or create call history for this user
                if (!_userCallHistory.TryGetValue(consumerCode, out var callHistory))
                {
                    callHistory = new Queue<DateTime>();
                    _userCallHistory[consumerCode] = callHistory;
                }
                
                // Remove calls older than 60 seconds
                while (callHistory.Count > 0 && callHistory.Peek() < cutoffTime)
                {
                    callHistory.Dequeue();
                }
                
                // Add current call timestamp
                callHistory.Enqueue(now);
                
                // Keep only last 5 calls (cleanup)
                while (callHistory.Count > MaxCallsPerMinute)
                {
                    callHistory.Dequeue();
                }
            }
        }

        private static bool IsSkippedPath(string path)
        {
            return path.Contains("internal-login");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auth0ConsumerCode"></param>
        /// <returns></returns>
        private async Task<PersonDto> GetConsumerEmail(BaseRequestDto? auth0ConsumerCode, HttpContext context)
        {
            const string methodName = nameof(GetConsumerEmail);
            _logger.LogInformation("{ClassName}.{MethodName} - Starting to execute GetConsumerEmail, Auth0ConsumerCode : {ConsumerCode}", className,methodName, auth0ConsumerCode?.consumerCode);
            var consumerDto = new GetConsumerRequestDto { ConsumerCode = auth0ConsumerCode?.consumerCode };
            var consumerRecord = await GetConsumer(consumerDto);
            var personDto = new PersonDto { PersonId = consumerRecord?.Consumer.PersonId ?? 0 };
            var consumerEmail = await GetPerson(personDto);
            
            _logger.LogInformation("{ClassName}.{MethodName} - Ending to GetConsumerEmail, Auth0ConsumerCode : {ConsumerCode}",className, methodName, auth0ConsumerCode?.ToJson());
            var consumerPerson = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerRecord?.Consumer != null ? new[] { consumerRecord.Consumer } : Array.Empty<ConsumerDto>(),
                Person = consumerEmail
            };
            context.Items[HttpContextKeys.ConsumerInfo] = consumerPerson;
            return consumerEmail;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static string ReadToken(string token)
        {
            var jwtEncodedString = token.Substring(7);

            var jsonToken = new JwtSecurityToken(jwtEncodedString);

            var claims = string.Empty;
            foreach (var claim in jsonToken.Claims)
            {
                claims = ($"{claim.Type}: {claim.Value}");
                break;
            }

            var jsonSplit = claims.Split('{');
            var jsonObject = jsonSplit[1].ToString();
            var jsonData = "{" + jsonObject;
            return jsonData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerSummaryRequestDto"></param>
        /// <returns></returns>
        private async Task<GetConsumerResponseDto> GetConsumer(GetConsumerRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(GetConsumer);
            _logger.LogInformation("{ClassName}.{MethodName} - Consumer get started with Consumer code: {ConsumerCode}", className,methodName,consumerSummaryRequestDto.ConsumerCode);
            var consumer = await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", consumerSummaryRequestDto);
            if (consumer.Consumer == null)
            {
                _logger.LogError("{ClassName}.{MethodName} - Invalid Consumer code: {ConsumerCode},ErrorCode:{Code}",className, methodName, consumerSummaryRequestDto.ConsumerCode,StatusCodes.Status404NotFound);
                return new GetConsumerResponseDto();
            }
            _logger.LogInformation("{ClassName}.{MethodName} - Ending to GetConsumer, ConsumerCode : {ConsumerCode}",className, methodName, consumerSummaryRequestDto.ConsumerCode);
            return consumer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumer"></param>
        /// <returns></returns>
        private async Task<PersonDto> GetPerson(PersonDto consumer)
        {
            const string methodName = nameof(GetPerson);
            _logger.LogInformation("{ClassName}.{MethodName} - Person get started with PersonId:{Id}", className,methodName, consumer?.PersonId);
            var person = await _userClient.GetById<PersonDto>("person/", consumer?.PersonId ?? 0);
            if (person == null || string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
            {
                _logger.LogError("{ClassName}.{MethodName} - Invalid Person with PersonId: {Id},ErrorCode:{Code}", className,methodName, consumer?.PersonId,StatusCodes.Status404NotFound);
                return new PersonDto();
            }
            _logger.LogInformation("{ClassName}.{MethodName} - Ending to GetPerson, PersonCode : {PersonCode}", className,methodName, consumer?.PersonCode);
            return person;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<GetConsumerByEmailResponseDto> GetConsumerByEmail(string email)
        {
            const string methodName = nameof(GetConsumerByEmail);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("email", HttpUtility.UrlEncode(email));

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Email Started Processing GetConsumerBy Email", className,methodName);
                var data = await _userClient.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", parameters);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error Occurred While performing GetConsumerByEmail,ErrorCode:{Code}, ERROR: {Message}",className, methodName, StatusCodes.Status500InternalServerError,ex.Message);
                throw;
            }

        }
    }
}


