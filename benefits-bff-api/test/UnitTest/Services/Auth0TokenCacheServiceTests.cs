using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Configuration;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    /// <summary>
    /// Unit tests for Auth0TokenCacheService with Redis caching.
    /// Tests cover:
    /// - Redis cache operations
    /// - Tenant-aware cache keys
    /// - Vault fallback logic
    /// - Cache clearing
    /// </summary>
    public class Auth0TokenCacheServiceTests
    {
        private readonly Mock<ILogger<Auth0TokenCacheService>> _logger;
        private readonly Mock<IVault> _vault;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IDistributedCache> _cache;
        private readonly Mock<IOptions<RedisConfiguration>> _redisConfigOptions;
        private DefaultHttpContext _httpContext;
        private Auth0TokenCacheService _service;

        public Auth0TokenCacheServiceTests()
        {
            _logger = new Mock<ILogger<Auth0TokenCacheService>>();
            _vault = new Mock<IVault>();
            _configuration = new Mock<IConfiguration>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _cache = new Mock<IDistributedCache>();
            _redisConfigOptions = new Mock<IOptions<RedisConfiguration>>();

            _httpContext = new DefaultHttpContext();
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

            _redisConfigOptions.Setup(x => x.Value).Returns(new RedisConfiguration());

            SetupConfiguration();
            SetupVault();
        }

        private void SetupConfiguration()
        {
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://test.auth0.com/oauth/token");                                 
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");                                                    

            var audienceSection0 = new Mock<IConfigurationSection>();
            audienceSection0.Setup(x => x.Value).Returns("https://api.test.com/");                                                                              

            var audienceArraySection = new Mock<IConfigurationSection>();
            audienceArraySection.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> { audienceSection0.Object });                                                                          

            _configuration.Setup(x => x.GetSection("Auth0:Audiences"))
                .Returns(audienceArraySection.Object);
        }

        private void SetupVault()
        {
            _vault.Setup(v => v.GetTenantSecret(It.IsAny<string>(), CommonConstants.Auth0ClientSecretKeyName))                                                  
                .ReturnsAsync("test-client-secret");
            _vault.Setup(v => v.GetTenantSecret(It.IsAny<string>(), CommonConstants.Auth0ClienIdKeyName))                                                       
                .ReturnsAsync("test-client-id");
            _vault.Setup(v => v.GetSecret(CommonConstants.Auth0ClientSecretKeyName))                                                                            
                .ReturnsAsync("test-client-secret");
            _vault.Setup(v => v.GetSecret(CommonConstants.Auth0ClienIdKeyName))
                .ReturnsAsync("test-client-id");
            _vault.Setup(v => v.InvalidSecret).Returns("INVALID_SECRET");
        }

        private Auth0TokenCacheService CreateService()
        {
            return new Auth0TokenCacheService(
                _logger.Object,
                _vault.Object,
                _configuration.Object,
                _httpContextAccessor.Object,
                _cache.Object,
                _redisConfigOptions.Object);
        }

        [Fact]
        public async Task ClearTenantCacheAsync_CallsRedisRemove()
        {
            // Arrange
            _httpContext.Items[HttpContextKeys.TenantCode] = "tenant-test";
            _service = CreateService();

            // Act
            await _service.ClearTenantCacheAsync();

            // Assert
            _cache.Verify(c => c.RemoveAsync("m2m:token:tenant-test", default), Times.Once);
        }

        [Fact]
        public async Task GetTokenAsync_WhenCacheHit_ReturnsFromRedis()
        {
            // Arrange
            _httpContext.Items[HttpContextKeys.TenantCode] = "tenant-cached";
            
            var cachedToken = new TokenResponse
            {
                access_token = CreateMockJwtToken(DateTime.UtcNow.AddHours(1)),
                expires_in = "3600"
            };
            var cachedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cachedToken));
            
            _cache.Setup(c => c.GetAsync("m2m:token:tenant-cached", default))
                .ReturnsAsync(cachedBytes);

            _service = CreateService();

            // Act
            var result = await _service.GetTokenAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.access_token);
            _cache.Verify(c => c.GetAsync("m2m:token:tenant-cached", default), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetTokenAsync_WhenCacheMiss_CallsAuth0()
        {
            // Arrange
            _httpContext.Items[HttpContextKeys.TenantCode] = "tenant-miss";
            
            _cache.Setup(c => c.GetAsync("m2m:token:tenant-miss", default))
                .ReturnsAsync((byte[]?)null);

            _service = CreateService();

            // Act
            var result = await _service.GetTokenAsync();

            // Assert
            Assert.NotNull(result);
            _cache.Verify(c => c.GetAsync("m2m:token:tenant-miss", default), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetTokenAsync_WhenVaultReturnsInvalidSecret_FallsBackToDefaultSecret()                                                                
        {
            // Arrange
            _vault.Setup(v => v.GetTenantSecret(It.IsAny<string>(), CommonConstants.Auth0ClientSecretKeyName))                                                  
                .ReturnsAsync("INVALID_SECRET");
            _vault.Setup(v => v.GetTenantSecret(It.IsAny<string>(), CommonConstants.Auth0ClienIdKeyName))                                                       
                .ReturnsAsync("INVALID_SECRET");

            _cache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                .ReturnsAsync((byte[]?)null);

            _service = CreateService();

            // Act
            var result = await _service.GetTokenAsync();

            // Assert
            _vault.Verify(v => v.GetSecret(CommonConstants.Auth0ClientSecretKeyName), Times.AtLeastOnce);                                                       
            _vault.Verify(v => v.GetSecret(CommonConstants.Auth0ClienIdKeyName), Times.AtLeastOnce);                                                            
        }

        [Fact]
        public async Task GetTokenAsync_WithDifferentTenants_UsesDifferentCacheKeys()
        {
            // Arrange
            _service = CreateService();
            
            // Set up Tenant A
            _httpContext.Items[HttpContextKeys.TenantCode] = "tenant-A";
            _cache.Setup(c => c.GetAsync("m2m:token:tenant-A", default))
                .ReturnsAsync((byte[]?)null);
            
            // Act - Get token for Tenant A
            await _service.GetTokenAsync();
            
            // Switch to Tenant B
            _httpContext.Items[HttpContextKeys.TenantCode] = "tenant-B";
            _cache.Setup(c => c.GetAsync("m2m:token:tenant-B", default))
                .ReturnsAsync((byte[]?)null);
            
            // Act - Get token for Tenant B
            await _service.GetTokenAsync();

            // Assert - Different cache keys used
            _cache.Verify(c => c.GetAsync("m2m:token:tenant-A", default), Times.AtLeastOnce);
            _cache.Verify(c => c.GetAsync("m2m:token:tenant-B", default), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetTokenAsync_WithAuthConfigInHttpContext_UsesDomainBasedCacheKey()
        {
            // Arrange
            _service = CreateService();
            
            var authConfig = new AuthConfig
            {
                Auth0 = new Auth0Config
                {
                    Auth0TokenUrl = "https://tenant-domain.auth0.com/oauth/token"
                }
            };
            _httpContext.Items[HttpContextKeys.AuthConfig] = authConfig;
            
            _cache.Setup(c => c.GetAsync("m2m:token:domain:tenant-domain.auth0.com", default))
                .ReturnsAsync((byte[]?)null);
            
            // Act
            await _service.GetTokenAsync();

            // Assert
            _cache.Verify(c => c.GetAsync("m2m:token:domain:tenant-domain.auth0.com", default), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetTokenAsync_WithTenantCodeAndAuthConfig_PrefersTenantCode()
        {
            // Arrange
            _service = CreateService();
            
            _httpContext.Items[HttpContextKeys.TenantCode] = "tenant-explicit";
            var authConfig = new AuthConfig
            {
                Auth0 = new Auth0Config
                {
                    Auth0TokenUrl = "https://domain-based.auth0.com/oauth/token"
                }
            };
            _httpContext.Items[HttpContextKeys.AuthConfig] = authConfig;
            
            _cache.Setup(c => c.GetAsync("m2m:token:tenant-explicit", default))
                .ReturnsAsync((byte[]?)null);
            
            // Act
            await _service.GetTokenAsync();

            // Assert - TenantCode takes priority over AuthConfig domain
            _cache.Verify(c => c.GetAsync("m2m:token:tenant-explicit", default), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetTokenAsync_WithNoTenantContext_UsesConfigBasedCacheKey()
        {
            // Arrange
            _service = CreateService();
            _httpContext.Items.Clear();
            
            _cache.Setup(c => c.GetAsync("m2m:token:config:test.auth0.com", default))
                .ReturnsAsync((byte[]?)null);
            
            // Act
            await _service.GetTokenAsync();

            // Assert
            _cache.Verify(c => c.GetAsync("m2m:token:config:test.auth0.com", default), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetTokenAsync_WhenRedisThrows_StillFetchesFromAuth0()
        {
            // Arrange
            _httpContext.Items[HttpContextKeys.TenantCode] = "tenant-redis-error";
            
            _cache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                .ThrowsAsync(new Exception("Redis connection failed"));

            _service = CreateService();

            // Act
            var result = await _service.GetTokenAsync();

            // Assert - Should not throw, should attempt to fetch from Auth0
            Assert.NotNull(result);
        }

        /// <summary>
        /// Creates a mock JWT token with the specified expiration time.
        /// </summary>
        private string CreateMockJwtToken(DateTime expiration)
        {
            var exp = new DateTimeOffset(expiration).ToUnixTimeSeconds();
            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"exp\":{exp},\"iss\":\"https://test.auth0.com/\"}}"));
            var signature = "mock-signature";
            return $"{header}.{payload}.{signature}";
        }
    }
}
