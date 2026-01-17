namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Represents a service for interacting with Auth0-related functionality.
    /// </summary>
    public interface IAuth0Service
    {
        /// <summary>
        /// Retrieves the consumer code associated with the provided Auth0 token.
        /// </summary>
        /// <param name="auth0Token">The Auth0 token for authentication and authorization.</param>
        /// <returns>
        /// A string representing the consumer code if the token is valid; 
        /// otherwise, null if the token is invalid or no associated consumer code exists.
        /// </returns>
        string? GetConsumerCode(string auth0Token);
    }

}
