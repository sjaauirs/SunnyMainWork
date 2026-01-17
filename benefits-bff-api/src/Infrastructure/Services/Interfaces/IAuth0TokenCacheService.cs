using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Service interface for caching and managing Auth0 client_credentials tokens.
    /// Tokens are cached per-tenant to support multi-tenant environments.
    /// </summary>
    public interface IAuth0TokenCacheService
    {
        /// <summary>
        /// Gets a valid Auth0 client_credentials token for the current tenant.
        /// Uses cache if available and not expired. Automatically refreshes on expiration.
        /// </summary>
        Task<TokenResponse> GetTokenAsync();

        /// <summary>
        /// Clears all cached tokens for all tenants.
        /// </summary>
        Task ClearCacheAsync();

        /// <summary>
        /// Clears the cached token for the current tenant only.
        /// </summary>
        Task ClearTenantCacheAsync();
    }
}

