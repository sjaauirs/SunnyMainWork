using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class PersonRoleService : IPersonRoleService
    {
        private readonly ILogger<PersonRoleService> _logger;
        private readonly IUserClient _userClient;
        private readonly IAuth0Service _auth0Service;
        public const string className = nameof(PersonRoleService);

        public PersonRoleService(ILogger<PersonRoleService> logger, IUserClient userClient, IAuth0Service auth0Service)
        {
            _logger = logger;
            _userClient = userClient;
            _auth0Service = auth0Service;
        }

        /// <summary>
        /// Retrieves all the PersonRoles available in database
        /// </summary>
        /// <param name="getPersonRolesRequestDto">request contains email and personCode to fetch personRoles</param>
        /// <returns>GetPersonRoles as List with base responses with statusCodes</returns>
        public async Task<GetPersonRolesResponseDto> GetPersonRoles(GetPersonRolesRequestDto getPersonRolesRequestDto)
        {
            return await _userClient.Post<GetPersonRolesResponseDto>(Constant.PersonRoles, getPersonRolesRequestDto);
        }

        /// <summary>
        /// Fetch the access control list for the specified auth0Token.
        /// </summary>
        /// <param name="auth0Token"></param>
        /// <returns></returns>
        public async Task<AccessControlListResponseDTO> GetAccessControlList(string? auth0Token)
        {
            const string methodName = nameof(GetAccessControlList);
            _logger.LogInformation("{ClassName}.{MethodName}: Started processing for auth0Token: {auth0Token}", className, methodName, auth0Token);

            var response = new AccessControlListResponseDTO();

            try
            {
                // Validate auth0Token
                if (string.IsNullOrWhiteSpace(auth0Token))
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: auth0Token is null or empty", className, methodName);
                    response.ErrorCode = StatusCodes.Status404NotFound;
                    response.ErrorMessage = "auth0Token is null or empty";
                    return response;
                }

                var consumerCode = _auth0Service.GetConsumerCode(auth0Token);

                // Validate consumerCode
                if (string.IsNullOrWhiteSpace(consumerCode))
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: ConsumerCode is null or empty", className, methodName);
                    response.ErrorCode = StatusCodes.Status404NotFound;
                    response.ErrorMessage = "ConsumerCode is null or empty";
                    return response;
                }

                response = await _userClient.Get<AccessControlListResponseDTO>($"{Constant.PersonAccessControlList}/{consumerCode}", new Dictionary<string, long>());

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error processing for ConsumerCode: {ConsumerCode}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", className, methodName, consumerCode, response.ErrorCode, response.ErrorMessage);
                    return response;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Ended processing for ConsumerCode: {ConsumerCode}", className, methodName, consumerCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error processing for auth0Token: {auth0Token}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", className, methodName, auth0Token, response.ErrorCode, response.ErrorMessage);
                return response;
            }
        }
    }
}
