namespace Sunny.Benefits.Bff.Infrastructure.Configuration
{
    /// <summary>
    /// Redis configuration options
    /// </summary>
    public class RedisConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = string.Empty;
        public int DefaultExpirationMinutes { get; set; } = 30;
        
        /// <summary>
        /// Enables/disables Redis caching for Auth0 token validation.
        /// This flag is read from SSM Parameter Store at runtime.
        /// </summary>
        public bool CacheEnabledForAuth0 { get; set; } = false;
        
        /// <summary>
        /// Enables/disables Redis caching for API responses (future implementation).
        /// This flag is read from SSM Parameter Store at runtime.
        /// </summary>
        public bool CacheEnabledForApi { get; set; } = false;
    }
}

