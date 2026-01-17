using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    /// <summary>
    /// Represents a service for interacting with Auth0-related functionality.
    /// </summary>
    public class Auth0Service : IAuth0Service
    {
        private readonly ILogger<Auth0Service> _logger;

        private const string className = nameof(Auth0Service);

        public Auth0Service(ILogger<Auth0Service> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the consumer code associated with the provided Auth0 token.
        /// </summary>
        /// <param name="auth0Token">The Auth0 token for authentication and authorization.</param>
        /// <returns>
        /// A string representing the consumer code if the token is valid; 
        /// otherwise, null if the token is invalid or no associated consumer code exists.
        /// </returns>
        public string? GetConsumerCode(string auth0Token)
        {
            string? consumerCode = null;
            
            const string methodName = nameof(GetConsumerCode);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing for auth0Token: {auth0Token}", className, methodName, auth0Token);

                string jsonData = ReadToken(auth0Token);
                if (jsonData != "{}")
                {
                    consumerCode = JsonConvert.DeserializeObject<BaseRequestDto>(jsonData)?.ConsumerCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error processing for auth0Token: {auth0Token}", className, methodName, auth0Token);
            }
            return consumerCode;

        }

        /// <summary>
        /// Extracts and processes the payload of a JWT (JSON Web Token) from the provided Auth0 token.
        /// </summary>
        /// <param name="auth0Token">The Auth0 token, expected to be a Bearer token with a prefix ("Bearer ").</param>
        /// <returns>
        /// A JSON string representing the payload of the token if extraction and processing are successful.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the token length is less than or equal to 7 characters, indicating an invalid or improperly formatted token.
        /// </exception>
        private static string ReadToken(string auth0Token)
        {
            var jwtEncodedString = auth0Token.Substring(7);

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
    }
}
