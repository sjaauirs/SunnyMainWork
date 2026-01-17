using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sunny.Benefits.Bff.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    /// <summary>
    /// Caches JWT validation results in Redis to reduce Auth0 API calls.
    /// </summary>
    public interface IJwtValidationCacheService
    {
        Task<JwtValidationResult?> GetCachedValidationAsync(string token);
        Task CacheValidationResultAsync(string token, bool isValid, string email);
        string ComputeTokenHash(string token);
    }

    public class JwtValidationResult
    {
        public bool IsValid { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime CachedAt { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class JwtValidationCacheService : IJwtValidationCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<JwtValidationCacheService> _logger;
        private readonly RedisConfiguration _redisConfig;
        private const string CacheKeyPrefix = "jwt:";
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(55);

        public JwtValidationCacheService(
            IDistributedCache cache,
            ILogger<JwtValidationCacheService> logger,
            IOptions<RedisConfiguration> redisConfig)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redisConfig = redisConfig?.Value ?? throw new ArgumentNullException(nameof(redisConfig));
        }

        public async Task<JwtValidationResult?> GetCachedValidationAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var hash = ComputeTokenHash(token);
                var key = $"{CacheKeyPrefix}{hash}";

                _logger.LogInformation("JwtValidationCacheService - Checking cache for key: {Key}", key);
                var cachedBytes = await _cache.GetAsync(key);
                if (cachedBytes == null || cachedBytes.Length == 0)
                {
                    _logger.LogInformation("JwtValidationCacheService - Cache MISS for token hash: {Hash}", hash.Substring(0, 8));
                    return null;
                }

                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                var result = JsonSerializer.Deserialize<JwtValidationResult>(cachedJson);

                if (result != null)
                {
                    _logger.LogInformation("JwtValidationCacheService - Cache HIT for token hash: {Hash}, IsValid: {IsValid}, CachedAt: {CachedAt}",
                        hash.Substring(0, 8), result.IsValid, result.CachedAt);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JwtValidationCacheService - Error reading from cache, will proceed with Auth0 validation");
                return null;
            }
        }

        public async Task CacheValidationResultAsync(string token, bool isValid, string email)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            try
            {
                var hash = ComputeTokenHash(token);
                var key = $"{CacheKeyPrefix}{hash}";

                var result = new JwtValidationResult
                {
                    IsValid = isValid,
                    Email = email ?? string.Empty,
                    CachedAt = DateTime.UtcNow
                };

                var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _defaultCacheDuration
                };

                await _cache.SetAsync(key, jsonBytes, options);

                _logger.LogInformation("JwtValidationCacheService - Cached validation result for token hash: {Hash}, IsValid: {IsValid}, TTL: {TTL}min",
                    hash.Substring(0, 8), isValid, _defaultCacheDuration.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JwtValidationCacheService - Error caching validation result, validation was successful but won't be cached");
                // Don't throw - caching failure shouldn't fail the request
            }
        }

        public string ComputeTokenHash(string token)
        {
            // Clean the token (remove "Bearer " prefix if present)
            var cleanToken = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? token.Substring(7)
                : token;

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cleanToken));
            return Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-");
        }
    }
}

