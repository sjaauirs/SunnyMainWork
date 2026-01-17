using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.Configuration;
using Sunny.Benefits.Bff.Infrastructure.Services;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class JwtValidationCacheServiceTests
    {
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<ILogger<JwtValidationCacheService>> _mockLogger;
        private readonly Mock<IOptions<RedisConfiguration>> _mockRedisConfig;
        private readonly JwtValidationCacheService _service;

        public JwtValidationCacheServiceTests()
        {
            _mockCache = new Mock<IDistributedCache>();
            _mockLogger = new Mock<ILogger<JwtValidationCacheService>>();
            _mockRedisConfig = new Mock<IOptions<RedisConfiguration>>();
            _mockRedisConfig.Setup(x => x.Value).Returns(new RedisConfiguration { CacheEnabledForAuth0 = true });
            
            _service = new JwtValidationCacheService(_mockCache.Object, _mockLogger.Object, _mockRedisConfig.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullCache_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new JwtValidationCacheService(null!, _mockLogger.Object, _mockRedisConfig.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new JwtValidationCacheService(_mockCache.Object, null!, _mockRedisConfig.Object));
        }

        [Fact]
        public void Constructor_WithNullRedisConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new JwtValidationCacheService(_mockCache.Object, _mockLogger.Object, null!));
        }

        #endregion

        #region ComputeTokenHash Tests

        [Fact]
        public void ComputeTokenHash_WithBearerPrefix_RemovesPrefixAndComputesHash()
        {
            var token = "Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test";
            var hash = _service.ComputeTokenHash(token);
            
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.DoesNotContain("/", hash);
            Assert.DoesNotContain("+", hash);
        }

        [Fact]
        public void ComputeTokenHash_WithoutBearerPrefix_ComputesHash()
        {
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test";
            var hash = _service.ComputeTokenHash(token);
            
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void ComputeTokenHash_SameToken_ReturnsSameHash()
        {
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test";
            var hash1 = _service.ComputeTokenHash(token);
            var hash2 = _service.ComputeTokenHash(token);
            
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void ComputeTokenHash_DifferentTokens_ReturnDifferentHashes()
        {
            var token1 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test1";
            var token2 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test2";
            var hash1 = _service.ComputeTokenHash(token1);
            var hash2 = _service.ComputeTokenHash(token2);
            
            Assert.NotEqual(hash1, hash2);
        }

        #endregion

        #region GetCachedValidationAsync Tests

        [Fact]
        public async Task GetCachedValidationAsync_WithNullToken_ReturnsNull()
        {
            var result = await _service.GetCachedValidationAsync(null!);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCachedValidationAsync_WithEmptyToken_ReturnsNull()
        {
            var result = await _service.GetCachedValidationAsync("");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCachedValidationAsync_CacheMiss_ReturnsNull()
        {
            _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var result = await _service.GetCachedValidationAsync("test-token");
            
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCachedValidationAsync_CacheHit_ReturnsResult()
        {
            var cachedResult = new JwtValidationResult
            {
                IsValid = true,
                Email = "test@example.com",
                CachedAt = DateTime.UtcNow
            };
            var cachedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cachedResult));
            
            _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedBytes);

            var result = await _service.GetCachedValidationAsync("test-token");
            
            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetCachedValidationAsync_CacheException_ReturnsNull()
        {
            _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Redis connection failed"));

            var result = await _service.GetCachedValidationAsync("test-token");
            
            Assert.Null(result);
        }

        #endregion

        #region CacheValidationResultAsync Tests

        [Fact]
        public async Task CacheValidationResultAsync_WithValidData_CachesResult()
        {
            _mockCache.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.CacheValidationResultAsync("test-token", true, "test@example.com");

            _mockCache.Verify(c => c.SetAsync(
                It.Is<string>(k => k.StartsWith("jwt:")),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CacheValidationResultAsync_WithNullToken_DoesNotCache()
        {
            await _service.CacheValidationResultAsync(null!, true, "test@example.com");

            _mockCache.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CacheValidationResultAsync_CacheException_DoesNotThrow()
        {
            _mockCache.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Redis connection failed"));

            // Should not throw
            await _service.CacheValidationResultAsync("test-token", true, "test@example.com");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CacheAndRetrieve_RoundTrip_PreservesData()
        {
            byte[]? storedBytes = null;
            
            _mockCache.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, bytes, _, _) => storedBytes = bytes)
                .Returns(Task.CompletedTask);

            await _service.CacheValidationResultAsync("test-token", true, "test@example.com");

            _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedBytes);

            var result = await _service.GetCachedValidationAsync("test-token");

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal("test@example.com", result.Email);
        }

        #endregion
    }
}

