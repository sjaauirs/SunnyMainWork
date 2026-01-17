using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Configuration;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    /// <summary>
    /// Thread-safe service for caching and managing Auth0 client_credentials tokens.
    /// Tokens are cached in Redis per-tenant to support multi-tenant environments.
    /// </summary>
    public class Auth0TokenCacheService : IAuth0TokenCacheService
    {
        private readonly ILogger<Auth0TokenCacheService> _logger;
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;
        private readonly RedisConfiguration _redisConfig;
        private const string className = nameof(Auth0TokenCacheService);
        private const string CacheKeyPrefix = "m2m:token:";

        // Refresh token 5 minutes before expiration
        private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(5);
        
        // JWT handler for parsing and validating tokens
        private static readonly JwtSecurityTokenHandler _jwtHandler = new JwtSecurityTokenHandler();

        // In-memory locks per tenant to prevent thundering herd
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _tenantLocks = new();

        public Auth0TokenCacheService(
            ILogger<Auth0TokenCacheService> logger,
            IVault vault,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache,
            IOptions<RedisConfiguration> redisConfig)
        {
            _logger = logger;
            _vault = vault;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _redisConfig = redisConfig?.Value ?? new RedisConfiguration();
        }

        /// <summary>
        /// Gets a valid Auth0 client_credentials token, using Redis cache if available and not expired.
        /// Automatically refreshes the token if it's close to expiring.
        /// Tokens are cached per-tenant to support multi-tenant Auth0 configurations.
        /// </summary>
        public async Task<TokenResponse> GetTokenAsync()
        {
            const string methodName = nameof(GetTokenAsync);

            var cacheKey = GetTenantCacheKey();
            
            _logger.LogDebug("{ClassName}.{MethodName} - Getting token for cache key: {CacheKey}",
                className, methodName, cacheKey);

            // Try to get from Redis cache
            var cached = await GetCachedTokenAsync(cacheKey);
            if (cached != null)
            {
                var jwtExpiration = GetJwtExpiration(cached.access_token);
                if (jwtExpiration.HasValue && DateTime.UtcNow < jwtExpiration.Value - RefreshBuffer)
                {
                    _logger.LogDebug("{ClassName}.{MethodName} - Using cached token from Redis for {CacheKey} (JWT exp: {JwtExp})",
                        className, methodName, cacheKey, jwtExpiration);
                    return cached;
                }
                
                _logger.LogInformation("{ClassName}.{MethodName} - Cached token for {CacheKey} expired or close to expiration (JWT exp: {JwtExp}), will refresh",
                    className, methodName, cacheKey, jwtExpiration);
            }

            // Get or create tenant-specific lock to prevent thundering herd
            var tenantLock = _tenantLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

            await tenantLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                var cachedAfterLock = await GetCachedTokenAsync(cacheKey);
                if (cachedAfterLock != null)
                {
                    var jwtExpiration = GetJwtExpiration(cachedAfterLock.access_token);
                    if (jwtExpiration.HasValue && DateTime.UtcNow < jwtExpiration.Value - RefreshBuffer)
                    {
                        _logger.LogDebug("{ClassName}.{MethodName} - Using cached token for {CacheKey} (refreshed by another thread)",
                            className, methodName, cacheKey);
                        return cachedAfterLock;
                    }
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Fetching new Auth0 client_credentials token for {CacheKey}",
                    className, methodName, cacheKey);

                var tokenResponse = await FetchTokenFromAuth0Async();

                if (tokenResponse?.access_token != null)
                {
                    var jwtExpiration = GetJwtExpiration(tokenResponse.access_token);
                    
                    if (jwtExpiration.HasValue)
                    {
                        var ttl = jwtExpiration.Value - DateTime.UtcNow - RefreshBuffer;
                        if (ttl > TimeSpan.Zero)
                        {
                            await CacheTokenAsync(cacheKey, tokenResponse, ttl);
                            _logger.LogInformation("{ClassName}.{MethodName} - Cached new token in Redis for {CacheKey} (TTL: {TTL}min, JWT exp: {JwtExp})",
                                className, methodName, cacheKey, ttl.TotalMinutes, jwtExpiration);
                        }
                    }
                    else if (!string.IsNullOrEmpty(tokenResponse.expires_in) &&
                             int.TryParse(tokenResponse.expires_in, out var expiresInSeconds) &&
                             expiresInSeconds > 0)
                    {
                        var ttl = TimeSpan.FromSeconds(expiresInSeconds) - RefreshBuffer;
                        if (ttl > TimeSpan.Zero)
                        {
                            await CacheTokenAsync(cacheKey, tokenResponse, ttl);
                            _logger.LogInformation("{ClassName}.{MethodName} - Cached new token in Redis for {CacheKey} (TTL: {TTL}min, expires_in: {ExpiresIn}s)",
                                className, methodName, cacheKey, ttl.TotalMinutes, expiresInSeconds);
                        }
                    }
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName} - Failed to obtain valid token from Auth0 for {CacheKey}. ErrorCode: {ErrorCode}",
                        className, methodName, cacheKey, tokenResponse?.ErrorCode);
                }

                return tokenResponse ?? new TokenResponse { ErrorCode = (int)HttpStatusCode.InternalServerError };
            }
            finally
            {
                tenantLock.Release();
            }
        }

        /// <summary>
        /// Gets the cache key for the current tenant.
        /// </summary>
        private string GetTenantCacheKey()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            
            var tenantCode = httpContext?.Items[HttpContextKeys.TenantCode]?.ToString();
            if (!string.IsNullOrWhiteSpace(tenantCode))
            {
                return $"{CacheKeyPrefix}{tenantCode}";
            }

            if (httpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.Auth0TokenUrl))
            {
                try
                {
                    var uri = new Uri(authConfig.Auth0.Auth0TokenUrl);
                    return $"{CacheKeyPrefix}domain:{uri.Host}";
                }
                catch
                {
                    // Ignore URI parsing errors
                }
            }

            var configTokenUrl = _configuration.GetSection("Auth0:Auth0TokenUrl").Value;
            if (!string.IsNullOrWhiteSpace(configTokenUrl))
            {
                try
                {
                    var uri = new Uri(configTokenUrl);
                    return $"{CacheKeyPrefix}config:{uri.Host}";
                }
                catch
                {
                    // Ignore URI parsing errors
                }
            }

            return $"{CacheKeyPrefix}default";
        }

        /// <summary>
        /// Gets cached token from Redis.
        /// </summary>
        private async Task<TokenResponse?> GetCachedTokenAsync(string cacheKey)
        {
            try
            {
                var cachedBytes = await _cache.GetAsync(cacheKey);
                if (cachedBytes == null || cachedBytes.Length == 0)
                {
                    return null;
                }

                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                return JsonConvert.DeserializeObject<TokenResponse>(cachedJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{ClassName}.GetCachedTokenAsync - Error reading from Redis cache for {CacheKey}",
                    className, cacheKey);
                return null;
            }
        }

        /// <summary>
        /// Caches token in Redis.
        /// </summary>
        private async Task CacheTokenAsync(string cacheKey, TokenResponse token, TimeSpan ttl)
        {
            try
            {
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(token));
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                };

                await _cache.SetAsync(cacheKey, jsonBytes, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{ClassName}.CacheTokenAsync - Error caching token in Redis for {CacheKey}",
                    className, cacheKey);
            }
        }

        /// <summary>
        /// Fetches a new client_credentials token from Auth0 with retry logic for transient failures.
        /// </summary>
        private async Task<TokenResponse> FetchTokenFromAuth0Async()
        {
            const string methodName = nameof(FetchTokenFromAuth0Async);
            const int maxRetries = 3;
            const int baseDelaySeconds = 1;
            
            var fetchStopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    string auth0TokenUrl = GetAuth0TokenUrl();
                    string[] audiences = GetAuth0Audiences();
                    string grantType = GetAuth0GrantType();
                    
                    var vaultStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var clientSecretTask = GetTenantSecretAsync(CommonConstants.Auth0ClientSecretKeyName);
                    var clientIdTask = GetTenantClientIdAsync(CommonConstants.Auth0ClienIdKeyName);
                    
                    await Task.WhenAll(clientSecretTask, clientIdTask);
                    vaultStopwatch.Stop();
                    
                    var clientSecret = await clientSecretTask;
                    var clientId = await clientIdTask;
                    
                    if (attempt == 0)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Vault calls completed in {VaultMs}ms",
                            className, methodName, vaultStopwatch.ElapsedMilliseconds);
                    }
                    
                    string combinedAudience = string.Join(",", audiences);

                    var requestContent = new StringContent(
                        $"grant_type={grantType}&client_id={clientId}&client_secret={clientSecret}&audience={combinedAudience}",
                        Encoding.UTF8,
                        "application/x-www-form-urlencoded"
                    );

                    var auth0TokenAPIUrl = new Uri(auth0TokenUrl);
                    
                    var auth0CallStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var httpResponse = await client.PostAsync(auth0TokenAPIUrl, requestContent);
                    auth0CallStopwatch.Stop();
                    
                    if (attempt == 0)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Auth0 API call completed in {Auth0Ms}ms",
                            className, methodName, auth0CallStopwatch.ElapsedMilliseconds);
                    }

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                        
                        fetchStopwatch.Stop();
                        _logger.LogInformation("{ClassName}.{MethodName} - Successfully obtained token from Auth0 (Total time: {TotalMs}ms)",
                            className, methodName, fetchStopwatch.ElapsedMilliseconds);
                        
                        return tokenResponse ?? new TokenResponse { ErrorCode = (int)HttpStatusCode.InternalServerError };
                    }
                    else
                    {
                        var errorContent = await httpResponse.Content.ReadAsStringAsync();
                        var statusCode = (int)httpResponse.StatusCode;
                        
                        if (statusCode >= 400 && statusCode < 500)
                        {
                            fetchStopwatch.Stop();
                            _logger.LogError("{ClassName}.{MethodName} - Failed to obtain token. Status Code: {StatusCode}, Response: {Response}",
                                className, methodName, httpResponse.StatusCode, errorContent);
                            return new TokenResponse { ErrorCode = statusCode };
                        }
                        
                        if (attempt < maxRetries - 1)
                        {
                            var delaySeconds = baseDelaySeconds * (int)Math.Pow(2, attempt);
                            _logger.LogWarning("{ClassName}.{MethodName} - Failed (attempt {Attempt}/{MaxRetries}). Status Code: {StatusCode}. Retrying in {DelaySeconds}s",
                                className, methodName, attempt + 1, maxRetries, httpResponse.StatusCode, delaySeconds);
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                            continue;
                        }
                        
                        fetchStopwatch.Stop();
                        _logger.LogError("{ClassName}.{MethodName} - Failed after {MaxRetries} attempts. Status Code: {StatusCode}, Response: {Response}",
                            className, methodName, maxRetries, httpResponse.StatusCode, errorContent);
                        return new TokenResponse { ErrorCode = statusCode };
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    if (attempt < maxRetries - 1)
                    {
                        var delaySeconds = baseDelaySeconds * (int)Math.Pow(2, attempt);
                        _logger.LogWarning(ex, "{ClassName}.{MethodName} - Timeout (attempt {Attempt}/{MaxRetries}). Retrying in {DelaySeconds}s",
                            className, methodName, attempt + 1, maxRetries, delaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        continue;
                    }
                    
                    fetchStopwatch.Stop();
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Timeout after {MaxRetries} attempts",
                        className, methodName, maxRetries);
                    throw;
                }
                catch (HttpRequestException ex)
                {
                    if (attempt < maxRetries - 1)
                    {
                        var delaySeconds = baseDelaySeconds * (int)Math.Pow(2, attempt);
                        _logger.LogWarning(ex, "{ClassName}.{MethodName} - Network error (attempt {Attempt}/{MaxRetries}). Retrying in {DelaySeconds}s",
                            className, methodName, attempt + 1, maxRetries, delaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        continue;
                    }
                    
                    fetchStopwatch.Stop();
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Network error after {MaxRetries} attempts",
                        className, methodName, maxRetries);
                    throw;
                }
                catch (Exception ex)
                {
                    fetchStopwatch.Stop();
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred while fetching token",
                        className, methodName);
                    throw;
                }
            }
            
            return new TokenResponse { ErrorCode = (int)HttpStatusCode.InternalServerError };
        }

        /// <summary>
        /// Clears all cached M2M tokens (removes by pattern is not supported in IDistributedCache).
        /// </summary>
        public Task ClearCacheAsync()
        {
            _logger.LogInformation("{ClassName}.ClearCacheAsync - Clear all not supported with IDistributedCache, use ClearTenantCacheAsync instead",
                className);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears the cached token for the current tenant.
        /// </summary>
        public async Task ClearTenantCacheAsync()
        {
            var cacheKey = GetTenantCacheKey();
            try
            {
                await _cache.RemoveAsync(cacheKey);
                _logger.LogInformation("{ClassName}.ClearTenantCacheAsync - Removed cache for {CacheKey}",
                    className, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{ClassName}.ClearTenantCacheAsync - Error removing cache for {CacheKey}",
                    className, cacheKey);
            }
        }

        #region Configuration Helpers

        private string GetAuth0TokenUrl()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.Auth0TokenUrl))
            {
                return authConfig.Auth0.Auth0TokenUrl;
            }

            return _configuration.GetSection("Auth0:Auth0TokenUrl").Value ?? string.Empty;
        }

        private string GetAuth0GrantType()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.GrantType))
            {
                return authConfig.Auth0.GrantType;
            }

            return _configuration.GetSection("Auth0:grant_type").Value ?? string.Empty;
        }

        private string[] GetAuth0Audiences()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var configObj) == true &&
                configObj is AuthConfig { Auth0.Audience: { Length: > 0 } } authConfig)
            {
                return authConfig.Auth0.Audience;
            }

            return _configuration.GetSection("Auth0:Audiences").Get<string[]>() ?? Array.Empty<string>();
        }

        private async Task<string> GetTenantSecretAsync(string secretKey)
        {
            const string methodName = nameof(GetTenantSecretAsync);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var tenantCode = _httpContextAccessor?.HttpContext?.Items[HttpContextKeys.TenantCode]?.ToString() ?? string.Empty;

            var secret = await _vault.GetTenantSecret(tenantCode, secretKey);
            stopwatch.Stop();

            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                var fallbackStopwatch = System.Diagnostics.Stopwatch.StartNew();
                secret = await _vault.GetSecret(secretKey);
                fallbackStopwatch.Stop();
                _logger.LogInformation("{ClassName}.{MethodName} - Tenant secret not found, fallback used for secretKey: {SecretKey}",
                    className, methodName, secretKey);
                return secret;
            }

            _logger.LogDebug("{ClassName}.{MethodName} - Tenant secret retrieved for secretKey: {SecretKey}",
                className, methodName, secretKey);
            return secret;
        }

        private async Task<string> GetTenantClientIdAsync(string clientIdKey)
        {
            const string methodName = nameof(GetTenantClientIdAsync);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var tenantCode = _httpContextAccessor?.HttpContext?.Items[HttpContextKeys.TenantCode]?.ToString() ?? string.Empty;

            var clientId = await _vault.GetTenantSecret(tenantCode, clientIdKey);
            stopwatch.Stop();

            if (string.IsNullOrEmpty(clientId) || clientId == _vault.InvalidSecret)
            {
                var fallbackStopwatch = System.Diagnostics.Stopwatch.StartNew();
                clientId = await _vault.GetSecret(clientIdKey);
                fallbackStopwatch.Stop();
                _logger.LogInformation("{ClassName}.{MethodName} - Tenant client ID not found, fallback used for clientIdKey: {ClientIdKey}",
                    className, methodName, clientIdKey);
                return clientId;
            }

            _logger.LogDebug("{ClassName}.{MethodName} - Tenant client ID retrieved for clientIdKey: {ClientIdKey}",
                className, methodName, clientIdKey);
            return clientId;
        }

        #endregion

        /// <summary>
        /// Parses the JWT token and extracts the expiration time from the 'exp' claim.
        /// </summary>
        private DateTime? GetJwtExpiration(string? accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            try
            {
                var jwtToken = _jwtHandler.ReadJwtToken(accessToken);
                
                if (jwtToken.Payload.TryGetValue("exp", out var expClaim) && expClaim != null)
                {
                    long expTimestamp = 0;
                    if (expClaim is long longValue)
                    {
                        expTimestamp = longValue;
                    }
                    else if (expClaim is int intValue)
                    {
                        expTimestamp = intValue;
                    }
                    else if (long.TryParse(expClaim.ToString(), out var parsed))
                    {
                        expTimestamp = parsed;
                    }

                    if (expTimestamp > 0)
                    {
                        return DateTimeOffset.FromUnixTimeSeconds(expTimestamp).UtcDateTime;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{ClassName}.GetJwtExpiration - Token is not a valid JWT or missing exp claim",
                    className);
                return null;
            }
        }
    }
}
