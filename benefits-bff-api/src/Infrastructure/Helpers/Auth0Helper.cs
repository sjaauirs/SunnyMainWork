using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System.Net;
using System.Text;
using System.Web;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers
{
    public class Auth0Helper : IAuth0Helper
    {
        private static int RETRY_MIN_WAIT_MS = 10; // min amount of milliseconds to wait before retrying
        private static int RETRY_MAX_WAIT_MS = 101; // max amount of milliseconds to wait before retrying
        private readonly ILogger<Auth0Helper> _logger;
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;
        private readonly IUserClient _userClient;
        private readonly ITenantClient _tenantClient;
        private readonly Random _random = new Random();
        private readonly int _maxTries;
        private readonly IPersonHelper _personHelper;
        private readonly IHashingService _hashingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuth0TokenCacheService _tokenCacheService;
        private const string className = nameof(Auth0Helper);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="vault"></param>
        /// <param name="configuration"></param>
        /// <param name="userClient"></param>

        public Auth0Helper(ILogger<Auth0Helper> logger, IVault vault, IConfiguration configuration,
            IUserClient userClient, IPersonHelper personHelper, IHashingService hashingService, IHttpContextAccessor httpContextAccessor, ITenantClient tenantClient, IAuth0TokenCacheService tokenCacheService)

        {
            _vault = vault;
            _configuration = configuration;
            _logger = logger;
            _userClient = userClient;

            _maxTries = 1;
            string? opMaxTries = _configuration.GetSection("OperationMaxTries").Value;
            if (!string.IsNullOrEmpty(opMaxTries))
            {
                _maxTries = Convert.ToInt32(opMaxTries);
            }
            _personHelper = personHelper;
            _hashingService = hashingService;
            _httpContextAccessor = httpContextAccessor;
            _tenantClient = tenantClient;
            _tokenCacheService = tokenCacheService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patchUserRequestDto"></param>
        /// <returns></returns>
        public async Task<UpdateResponseDto> PatchUserOuter(PatchUserRequestDto patchUserRequestDto)
        {
            const string methodName = nameof(PatchUserOuter);
            int maxTries = _maxTries;
            UpdateResponseDto? response = null;
            _logger.LogInformation("{ClassName}.{MethodName} - PatchUser API has been Invoked for UserId:{Id}"
                , className, methodName, patchUserRequestDto.UserId);
            while (maxTries > 0)
            {
                try
                {
                    response = await PatchUser(patchUserRequestDto);
                    if (response.ErrorCode == null)
                    {
                        // Update consumer flag if PATCH operation succeeds
                        await updateRegisterFlag(patchUserRequestDto.Email);
                        maxTries--;
                        break;
                    }
                    _logger.LogError("{ClassName}.{MethodName} - Error in PatchUser,ErrorCode:{Code}, retrying count left={MaxTries}, UserId:{Id}", className, methodName, response.ErrorCode,
                        maxTries, patchUserRequestDto.UserId);
                    maxTries--;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Error in PatchUser,ErrorCode:{Code},ERROR:{Msg} retrying count left={MaxTries}, UserId:{Id}",
                        className, methodName, StatusCodes.Status500InternalServerError, ex.Message, maxTries, patchUserRequestDto.UserId);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                    throw;
                }
            }
            if (response == null)
            {
                // If all retries fail, return with a fallback response
                response = new UpdateResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
                _logger.LogWarning("{ClassName}.{MethodName} - Using a fallback response due to multiple retries: {Response},ErrorCode:{Code}", className, methodName, response.ToJson(), StatusCodes.Status500InternalServerError);
            }

            // creating consumer device
            if (response.ErrorCode == null)
            {
                await CreateConsumerDevice(patchUserRequestDto, response.app_metadata.ConsumerCode, response.app_metadata.TenantCode);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patchUserRequestDto"></param>
        /// <returns></returns>
        public async Task<UpdateResponseDto> PatchUser(PatchUserRequestDto patchUserRequestDto)
        {
            const string methodName = nameof(PatchUser);
            try
            {
                UpdateResponseDto? userUpdateResponse = new();
                string env = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(env) || env == _vault.InvalidSecret)
                    return new UpdateResponseDto() { ErrorCode = (int)HttpStatusCode.InternalServerError, ErrorMessage = "Internal Error" };

                var token = await _tokenCacheService.GetTokenAsync();
                if (token != null)
                {
                    using var client = new HttpClient();
                    var auth0ApiUrl = GetAuth0ApiUrl();
                    var auth0PatchUrl = $"{auth0ApiUrl}{patchUserRequestDto.UserId}";
                    var request = new HttpRequestMessage(HttpMethod.Patch, auth0PatchUrl);
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Authorization", $"Bearer {token.access_token}");

                    var personUniqueIdentifier = GetPersonUniqueIdentifierFromHttpContext() ?? patchUserRequestDto?.Email;
                    var memberNbr = GetMemberNbrFromHttpContext();
                    var regionCode = GetRegionCodeFromHttpContext();

                    if (string.IsNullOrWhiteSpace(personUniqueIdentifier) && string.IsNullOrEmpty(memberNbr) && string.IsNullOrEmpty(regionCode))
                    {
                        const string errorMessage = "Invalid request: personUniqueIdentifier, memberNbr, and regionCode are all missing.";

                        _logger.LogError(
                            "{ClassName}.{MethodName} - {ErrorMessage} ErrorCode: {ErrorCode}",
                            className, methodName, errorMessage, StatusCodes.Status404NotFound);

                        return new UpdateResponseDto
                        {
                            ErrorCode = (int)HttpStatusCode.NotFound,
                            ErrorMessage = errorMessage
                        };
                    }

                    GetConsumerByPersonUniqueIdentifierResponseDto? personConsumerResponse = null;
                    var httpContext = _httpContextAccessor?.HttpContext;
                    if (httpContext?.Items.TryGetValue(HttpContextKeys.ConsumerInfo, out var value) == true &&
                        value is GetConsumerByPersonUniqueIdentifierResponseDto consumerDetails)
                    {
                        personConsumerResponse = consumerDetails;
                    }
                    else
                    {
                        personConsumerResponse = await GetPersonConsumerResponse(patchUserRequestDto, personUniqueIdentifier, memberNbr, regionCode);
                    }

                    if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - GetConsumerByPersonUniqueIdentifier not found for given identifier. ErrorCode: {Code}, ERROR: Not Found",
                            className, methodName, StatusCodes.Status404NotFound);

                        return new UpdateResponseDto
                        {
                            ErrorCode = (int)HttpStatusCode.NotFound,
                            ErrorMessage = "Not Found"
                        };
                    }

                    var consumer = personConsumerResponse?.Consumer[0];

                    var userInfoRequestDto = new UserInfoDataRequestDto
                    {
                        app_metadata = new AppMetadata
                        {
                            ConsumerCode = consumer?.ConsumerCode,
                            Env = env,
                            Role = "subscriber",
                            TenantCode = consumer?.TenantCode,
                            PostalCode = personConsumerResponse?.Person?.PostalCode,
                            PersonUniqueIdentifier = personConsumerResponse?.Person?.PersonUniqueIdentifier,
                            IsSSOUser = consumer?.IsSSOUser ?? false,
                            MemberId = consumer?.MemberId
                        }
                    };

                    if (consumer != null && !consumer.IsSSOUser)
                    {
                        userInfoRequestDto.name = personConsumerResponse?.Person?.FirstName ?? string.Empty;
                    }

                    var jsonContent = JsonConvert.SerializeObject(userInfoRequestDto, new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new CamelCaseNamingStrategy()
                        },
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    request.Content = content;

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    userUpdateResponse = JsonConvert.DeserializeObject<UpdateResponseDto>(responseBody);
                    var auth0UserName = GetAuth0UserNameFromHttpContext();
                    if (!string.IsNullOrEmpty(auth0UserName) && consumer != null)
                    {
                        await _personHelper.UpdateConsumer(consumer?.ConsumerId ?? 0, consumer!, auth0UserName);
                    }

                }
                _logger.LogInformation("{ClassName}.{MethodName} - Patch User Update Successfully for Auth0 Email,UserId : {Auth0UserId}", className, methodName, patchUserRequestDto.UserId);
                return userUpdateResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing PatchUser,ErrorCode:{Code}, ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task<GetConsumerByPersonUniqueIdentifierResponseDto?> GetPersonConsumerResponse(
            PatchUserRequestDto patchUserRequestDto,
            string? personUniqueIdentifier,
            string? memberNbr,
            string? regionCode)
        {
            var personConsumerResponse = !string.IsNullOrEmpty(personUniqueIdentifier)
                ? await GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier)
                : null;

            if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null && !string.IsNullOrEmpty(memberNbr) && !string.IsNullOrEmpty(regionCode))
            {
                personConsumerResponse = await GetConsumerByMemberNbrAndRegionCode(memberNbr!, regionCode!);
            }

            if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null && !string.IsNullOrEmpty(patchUserRequestDto?.Email))
            {
                personConsumerResponse = await GetConsumerByEmail(patchUserRequestDto.Email);
            }

            return personConsumerResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task updateRegisterFlag(string email)
        {
            const string methodName = nameof(updateRegisterFlag);
            try
            {
                var emailResponseDto = await GetConsumerByIdentifierOrEmail(email);
                if (emailResponseDto?.Consumer != null && emailResponseDto.Consumer.Any())
                {
                    var consumerResponse = emailResponseDto.Consumer.ToList();
                    foreach (var consumer in consumerResponse)
                    {
                        if (!consumer.Registered)
                        {
                            await _userClient.Post<ConsumerModel>("consumer/update-register-flag", consumer);
                            _logger.LogInformation("{ClassName}.{MethodName} - Registered Flag Updated Successfully for consumer : {Consumer}", className, methodName, consumer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error updating register flag for given email,ErrorCode:{Code}", className, methodName, StatusCodes.Status500InternalServerError);
                throw;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<(bool emailVerified, string email)> Validatetoken(string accessToken)
        {
            const string methodName = nameof(Validatetoken);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing validate token,Token: {AccessToken}", className, methodName, accessToken);
                var userInfo = await GetUserInfo(accessToken);
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing ValidateToken ,ErrorCode:{Code},ERROR: {Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Post Verification Email
        /// </summary>
        /// <param name="emailRequestDto"></param>
        /// <returns></returns>
        public async Task<UpdateResponseDto> PostVerificationEmail(VerificationEmailRequestDto emailRequestDto)
        {
            const string methodName = nameof(PostVerificationEmail);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing PostVerificationEmail with UserId: {Id}", className, methodName, emailRequestDto.UserId);

            try
            {
                string env = await _vault.GetSecret(CommonConstants.Env);
                if (string.IsNullOrEmpty(env) || env == _vault.InvalidSecret)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid or missing environment secret,ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, StatusCodes.Status500InternalServerError, CommonConstants.InternalError);
                    return new UpdateResponseDto()
                    {
                        ErrorCode = (int)HttpStatusCode.InternalServerError,
                        ErrorMessage = CommonConstants.InternalError
                    };
                }

                var emailResponseDto = await GetConsumerByIdentifierOrEmail(emailRequestDto.Email);
                if (emailResponseDto?.Person?.Email == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - GetConsumerByIdentifierOrEmail not found for given UserId:{Id},ErrorCode:{Code},ERROR:Not Found",
                        className, methodName, emailRequestDto.UserId, StatusCodes.Status404NotFound);
                    return new UpdateResponseDto() { ErrorCode = (int)HttpStatusCode.NotFound, ErrorMessage = "Not Found" };
                }

                var token = await _tokenCacheService.GetTokenAsync();
                if (token == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Failed to obtain token,ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, StatusCodes.Status401Unauthorized, CommonConstants.Unauthorized);
                    return new UpdateResponseDto()
                    {
                        ErrorCode = (int)HttpStatusCode.Unauthorized,
                        ErrorMessage = CommonConstants.Unauthorized
                    };
                }

                var VerificationEmailRequest = new VerificationEmailRequest()
                {
                    UserId = emailRequestDto.UserId,
                };

                var jsonContent = JsonConvert.SerializeObject(VerificationEmailRequest, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    },
                    Formatting = Formatting.Indented
                });

                var auth0ApiUrl = GetAuth0VerificationEmailUrl();

                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, auth0ApiUrl)
                {
                    Headers =
                    {
                        { CommonConstants.Accept, CommonConstants.ApplicationJson },
                        { CommonConstants.Authorization, $"{CommonConstants.Bearer} {token.access_token}"}
                    },
                    Content = new StringContent(jsonContent, Encoding.UTF8, CommonConstants.ApplicationJson)
                };

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("{ClassName}.{MethodName} - Error response from Auth0 Response:{ErrorResponse}, ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, errorResponse, response.StatusCode, response.ReasonPhrase);
                    return new UpdateResponseDto()
                    {
                        ErrorCode = (int)response.StatusCode,
                        ErrorMessage = response.ReasonPhrase
                    };
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var userUpdateResponse = JsonConvert.DeserializeObject<UpdateResponseDto>(responseBody);

                return userUpdateResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed processing.ErrorCode:{Code}, ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing.", className, methodName);
            }
        }

        /// <summary>
        /// Get User by UserId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserGetResponseDto> GetUserById(GetUserRequestDto userRequestDto)
        {
            const string methodName = nameof(GetUserById);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing. Request: userId:{Id}", className, methodName, userRequestDto.UserId);

            try
            {
                string env = await _vault.GetSecret(CommonConstants.Env);
                if (string.IsNullOrEmpty(env) || env == _vault.InvalidSecret)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid or missing environment secret,ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, StatusCodes.Status500InternalServerError, CommonConstants.InternalError);
                    return new UserGetResponseDto()
                    {
                        ErrorCode = (int)HttpStatusCode.InternalServerError,
                        ErrorMessage = CommonConstants.InternalError
                    };
                }
                GetConsumerByPersonUniqueIdentifierResponseDto? personConsumerResponse = null;
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.Items.TryGetValue(HttpContextKeys.ConsumerInfo, out var value) == true &&
                    value is GetConsumerByPersonUniqueIdentifierResponseDto consumerDetails)
                {
                    personConsumerResponse = consumerDetails;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(userRequestDto.Email))
                    {
                        userRequestDto.Email = GetUserEmailFromHttpContext();
                    }

                    var personUniqueIdentifier = GetPersonUniqueIdentifierFromHttpContext() ?? userRequestDto?.Email;
                    var memberNbr = GetMemberNbrFromHttpContext();
                    var regionCode = GetRegionCodeFromHttpContext();

                    if (string.IsNullOrWhiteSpace(personUniqueIdentifier) && string.IsNullOrEmpty(memberNbr) && string.IsNullOrEmpty(regionCode))
                    {
                        const string errorMessage = "Invalid request: personUniqueIdentifier, memberNbr, and regionCode are all missing.";

                        _logger.LogError(
                            "{ClassName}.{MethodName} - {ErrorMessage} ErrorCode: {ErrorCode}",
                            className, methodName, errorMessage, StatusCodes.Status404NotFound);

                        return new UserGetResponseDto
                        {
                            ErrorCode = (int)HttpStatusCode.NotFound,
                            ErrorMessage = errorMessage
                        };
                    }

                    personConsumerResponse = personUniqueIdentifier != null ? await GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier) : null;

                    if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null && !string.IsNullOrEmpty(memberNbr) && !string.IsNullOrEmpty(regionCode))
                    {
                        personConsumerResponse = await GetConsumerByMemberNbrAndRegionCode(memberNbr!, regionCode!);
                    }

                    if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null && !string.IsNullOrEmpty(userRequestDto?.Email))
                    {
                        personConsumerResponse = await GetConsumerByEmail(userRequestDto?.Email ?? string.Empty);
                    }

                    if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - GetConsumerByPersonUniqueIdentifier not found for given identifier. ErrorCode: {Code}, ERROR: Not Found",
                            className, methodName, StatusCodes.Status404NotFound);

                        return new UserGetResponseDto
                        {
                            ErrorCode = (int)HttpStatusCode.NotFound,
                            ErrorMessage = "Not Found"
                        };
                    }
                }
                

                var firstConsumer = personConsumerResponse?.Consumer?.OrderByDescending(x => x.ConsumerId)?.FirstOrDefault();

                var tenantData = await _personHelper.GetTenantByTenantCode(firstConsumer?.TenantCode ?? string.Empty);

                var token = await _tokenCacheService.GetTokenAsync();
                if (token == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Failed to obtain token,ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, StatusCodes.Status401Unauthorized, CommonConstants.Unauthorized);
                    return new UserGetResponseDto()
                    {
                        ErrorCode = (int)HttpStatusCode.Unauthorized,
                        ErrorMessage = CommonConstants.Unauthorized
                    };
                }
                 if (string.IsNullOrWhiteSpace(userRequestDto?.Email))
                 {
                     userRequestDto.Email = GetUserEmailFromHttpContext();
                 }
                var auth0ApiUrl = string.Empty;
                if (!string.IsNullOrEmpty(userRequestDto?.UserId))
                {
                    auth0ApiUrl = $"{GetAuth0ApiUrl()}{userRequestDto?.UserId}";
                }
                else if (!string.IsNullOrEmpty(userRequestDto?.Email))
                {
                    auth0ApiUrl = $"{GetAuth0UserInfoByEmailUrl()}{userRequestDto.Email}";
                }

                using var client = new HttpClient();
                var auth0ApiUri = new Uri(auth0ApiUrl);
                var request = new HttpRequestMessage(HttpMethod.Get, auth0ApiUri)
                {
                    Headers =
                    {
                        { CommonConstants.Accept, CommonConstants.ApplicationJson },
                        { CommonConstants.Authorization, $"{CommonConstants.Bearer} {token.access_token}"}
                    }
                };

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("{ClassName}.{MethodName} - Error response from Auth0 Response:{ErrorResponse}, ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, errorResponse, response.StatusCode, response.ReasonPhrase);
                    return new UserGetResponseDto()
                    {
                        ErrorCode = (int)response.StatusCode,
                        ErrorMessage = response.ReasonPhrase
                    };
                }

                var responseBody = await response.Content.ReadAsStringAsync();

                UserGetResponseDto? userGetResponseDto = null;

                if (!string.IsNullOrEmpty(userRequestDto?.UserId))
                {
                    userGetResponseDto = JsonConvert.DeserializeObject<UserGetResponseDto>(responseBody);
                }
                else if (!string.IsNullOrEmpty(userRequestDto?.Email))
                {
                    var userGetResponseDtos = JsonConvert.DeserializeObject<List<UserGetResponseDto>>(responseBody);
                    userGetResponseDto = userGetResponseDtos?.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(userRequestDto?.DeviceId))
                {
                    var consumerDeviceRequest = new GetConsumerDeviceRequestDto
                    {
                        ConsumerCode = firstConsumer?.ConsumerCode ?? string.Empty,
                        TenantCode = firstConsumer?.TenantCode ?? string.Empty
                    };

                    var consumerDevicesResponse = await GetConsumerDevices(consumerDeviceRequest);
                    userGetResponseDto!.DeviceRegistered = IsDeviceRegistered(userRequestDto.DeviceId, consumerDevicesResponse.ConsumerDevices);
                }
                var isSSOUser = GetIsSsoUserFromHttpContext();
                //update Onboarding State if status Not stared and email verified
                if (userGetResponseDto != null && !isSSOUser && userGetResponseDto.email_verified && personConsumerResponse?.Consumer?[0].OnBoardingState == OnboardingState.NOT_STARTED.ToString())
                {
                    await _personHelper.UpdateOnBoardingState(new UpdateOnboardingStateDto() { ConsumerCode = personConsumerResponse?.Consumer[0].ConsumerCode, TenantCode = personConsumerResponse?.Consumer[0].TenantCode!, OnboardingState = OnboardingState.EMAIL_VERIFIED });
                }
                userGetResponseDto!.language_code = personConsumerResponse?.Person?.LanguageCode ?? CommonConstants.DefaultLanguageCode;
                userGetResponseDto!.EnrollmentStatus = firstConsumer?.EnrollmentStatus;
                userGetResponseDto.TenantInfo = tenantData;
                if (personConsumerResponse != null)
                {
                    userGetResponseDto.ConsumerInfo = new GetConsumerByEmailResponseDto
                    {
                        Consumer = personConsumerResponse.Consumer,
                        Person = personConsumerResponse.Person
                    };
                }
                return userGetResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed processing.ErrorCode:{Code}, ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing.", className, methodName);
            }
        }

        public async Task<TenantDto> GetTenantByTenantCode(string tenantCode)
        {
            const string methodName = nameof(GetTenantByTenantCode);
            var getTenantCodeRequestDto = new GetTenantCodeRequestDto()
            {
                TenantCode = tenantCode,
            };
            var tenantResponse = await _tenantClient.Post<TenantDto>("tenant/get-by-tenant-code", getTenantCodeRequestDto);
            if (tenantResponse.TenantCode == null)
            {
                _logger.LogError("{ClassName}.{MethodName} - TenantDetails Not Found for TenantCode : {TenantCode}", className, methodName, getTenantCodeRequestDto.TenantCode);
                return new TenantDto();
            }
            _logger.LogInformation("Retrieved Tenant Successfully for TenantCode : {TenantCode}", getTenantCodeRequestDto.TenantCode);

            return tenantResponse;
        }

        public async Task<GetConsumerByPersonUniqueIdentifierResponseDto?> GetConsumerDetails()
        {
            const string methodName = nameof(GetConsumerDetails);
            var email = GetUserEmailFromHttpContext();
            var personUniqueIdentifier = GetPersonUniqueIdentifierFromHttpContext() ?? email;
            var memberNbr = GetMemberNbrFromHttpContext();
            var regionCode = GetRegionCodeFromHttpContext();

            if (string.IsNullOrWhiteSpace(personUniqueIdentifier) &&
                string.IsNullOrEmpty(memberNbr) &&
                string.IsNullOrEmpty(regionCode))
            {
                const string errorMessage = "Invalid request: personUniqueIdentifier, memberNbr, and regionCode are all missing.";

                _logger.LogError(
                    "{ClassName}.{MethodName} - {ErrorMessage} ErrorCode: {ErrorCode}",
                    className, methodName, errorMessage, StatusCodes.Status404NotFound);

                return new GetConsumerByPersonUniqueIdentifierResponseDto
                {
                    ErrorCode = (int)HttpStatusCode.NotFound,
                    ErrorMessage = errorMessage
                };
            }

            var personConsumerResponse = !string.IsNullOrWhiteSpace(personUniqueIdentifier)
                ? await GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier)
                : null;

            if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null &&
                !string.IsNullOrEmpty(memberNbr) &&
                !string.IsNullOrEmpty(regionCode))
            {
                personConsumerResponse = await GetConsumerByMemberNbrAndRegionCode(memberNbr!, regionCode!);
            }

            if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null &&
                !string.IsNullOrEmpty(email))
            {
                personConsumerResponse = await GetConsumerByEmail(email!);
            }

            if (personConsumerResponse?.Person?.PersonUniqueIdentifier == null)
            {
                _logger.LogError(
                    "{ClassName}.{MethodName} - GetConsumerByPersonUniqueIdentifier not found for given identifier. ErrorCode: {Code}, ERROR: Not Found",
                    className, methodName, StatusCodes.Status404NotFound);

                return new GetConsumerByPersonUniqueIdentifierResponseDto
                {
                    ErrorCode = (int)HttpStatusCode.NotFound,
                    ErrorMessage = "Not Found"
                };
            }

            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items[HttpContextKeys.ConsumerInfo] = personConsumerResponse;
            }
            
            return personConsumerResponse;
        }

        public async Task<bool> SetAuthConfigToContext(HttpContext context)
        {
            const string methodName = nameof(SetAuthConfigToContext);
            try
            {
                TenantDto? tenantDetails = null;
                GetConsumerByPersonUniqueIdentifierResponseDto? consumerDetails = null;

                // Try from HttpContext cache
                if (context.Items.TryGetValue(HttpContextKeys.ConsumerInfo, out var value) &&
                    value is GetConsumerByPersonUniqueIdentifierResponseDto cachedConsumerDetails)
                {
                    consumerDetails = cachedConsumerDetails;
                }
                else
                {
                    // Fallback to API
                    consumerDetails = await GetConsumerDetails();

                    bool hasError = consumerDetails?.ErrorCode != null;
                    bool missingTenantCode = string.IsNullOrEmpty(consumerDetails?.Consumer?.FirstOrDefault()?.TenantCode);

                    if (hasError || missingTenantCode)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        _logger.LogError("{ClassName}.{MethodName} - Unauthorized Access: Invalid token. ErrorCode: {Code}",
                            className, methodName, StatusCodes.Status401Unauthorized);
                        return false;
                    }
                }

                // Extract tenant code
                var consumer = consumerDetails?.Consumer?.OrderByDescending(x => x.ConsumerId)?.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(consumer?.TenantCode))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    _logger.LogError("{ClassName}.{MethodName} - Missing tenant code for token. ErrorCode: {Code}",
                        className, methodName, StatusCodes.Status401Unauthorized);
                    return false;
                }
                context.Items[HttpContextKeys.TenantCode] = consumer.TenantCode;

                // Fetch tenant details & deserialize AuthConfig
                tenantDetails = await GetTenantByTenantCode(consumer.TenantCode);
                _logger.LogInformation("{ClassName}.{MethodName} - TenantCode:{TenantCode}, Authconfig Details: {AuthConfig}", className, methodName, consumer.TenantCode, tenantDetails.AuthConfig);
                if (!string.IsNullOrWhiteSpace(tenantDetails?.AuthConfig))
                {
                    var authConfig = JsonConvert.DeserializeObject<AuthConfig>(tenantDetails.AuthConfig);
                    context.Items[HttpContextKeys.AuthConfig] = authConfig;
                }

                if(tenantDetails != null && tenantDetails.TenantId > 0)
                {
                    context.Items[HttpContextKeys.TenantInfo] = tenantDetails;
                }
                return true;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "{ClassName}.{MethodName} - Failed to deserialize AuthConfig for token", className, methodName);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Unexpected error occurred while setting auth config", className, methodName);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return false;
            }
        }



        /// <summary>
        /// Gets all the ConsumerDevices
        /// </summary>
        /// <param name="consumerDeviceRequest">request contains tenant code and Consumer code</param>
        /// <returns>List Of Consumer Devices</returns>
        public async Task<GetConsumerDeviceResponseDto> GetConsumerDevices(GetConsumerDeviceRequestDto consumerDeviceRequest)
        {
            const string methodName = nameof(GetConsumerDevices);
            GetConsumerDeviceResponseDto response = new GetConsumerDeviceResponseDto();
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - fetching Consumer Devices for ConsumerCode: {ConsumerCode}",
                    className, methodName, consumerDeviceRequest.ConsumerCode);

                var consumerDevicesResponse = await _userClient.Post<GetConsumerDeviceResponseDto>(CommonConstants.GetConsumerDevices, consumerDeviceRequest);
                if (consumerDevicesResponse == null || consumerDevicesResponse.ConsumerDevices == null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - No consumer devices found for ConsumerCode: {ConsumerCode}",
                        className, methodName, consumerDeviceRequest.ConsumerCode);
                    return response;
                }
                response.ConsumerDevices = consumerDevicesResponse.ConsumerDevices;
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error while checking device registration for ConsumerCode: {ConsumerCode}. ERROR: {Message}",
                    className, methodName, consumerDeviceRequest.ConsumerCode, ex.Message);
                return response;
            }
        }
        public bool IsDeviceRegistered(string deviceId, IList<ConsumerDeviceDto> consumerDevices)
        {
            if (consumerDevices == null || !consumerDevices.Any())
            {
                _logger.LogWarning("Consumer devices list is null or empty.");
                return false;
            }

            string? hashedDeviceId = _hashingService.ComputeSHA256Hash(deviceId);
            if (string.IsNullOrEmpty(hashedDeviceId))
            {
                _logger.LogError("Failed to compute hash for Device ID: {DeviceId}.", deviceId);
                return false;
            }

            // Check for matching hash in the consumer devices list
            bool isRegistered = consumerDevices.Any(device => device.DeviceIdHash == hashedDeviceId);
            _logger.LogInformation("Device registration status for Device ID {DeviceId}: {IsRegistered}.", deviceId, isRegistered);
            return isRegistered;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private async Task<(bool emailVerified, string email)> GetUserInfo(string accessToken)
        {
            const string methodName = nameof(GetUserInfo);
            try
            {
                using var client = new HttpClient();
                var auth0UserInfoUrl = GetAuth0UserInfoUrl();
                var request = new HttpRequestMessage(HttpMethod.Get, auth0UserInfoUrl);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", $"{accessToken}");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var updateResponse = new UpdateResponseDto();
                string consumerCode = string.Empty;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Validated Access Token : {AccessToken} :", className, methodName, accessToken);
                    updateResponse = JsonConvert.DeserializeObject<UpdateResponseDto>(await response.Content.ReadAsStringAsync());

                }
                var isSSOUser = GetIsSsoUserFromHttpContext();
                var isEmailVerified = isSSOUser || (updateResponse?.email_verified ?? false);
                var email = updateResponse?.email ?? string.Empty;

                return (isEmailVerified, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Invalid Access Token : {AccessToken},ErrorCode:{Code},ERROR:{Msg}",
                    className, methodName, accessToken, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private string GetAuth0UserInfoUrl()
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.Auth0UserInfoUrl))
            {
                return authConfig.Auth0.Auth0UserInfoUrl!;
            }
            return _configuration.GetSection("Auth0:Auth0UserInfoUrl").Value ?? string.Empty;
        }

        private string GetAuth0VerificationEmailUrl()
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.Auth0VerificationEmailUrl))
            {
                return authConfig.Auth0.Auth0VerificationEmailUrl!;
            }
            return _configuration.GetSection("Auth0:Auth0VerificationEmailUrl").Value ?? string.Empty;
        }

        private string GetAuth0UserInfoByEmailUrl()
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.Auth0UserInfoByEmailUrl))
            {
                return authConfig.Auth0.Auth0UserInfoByEmailUrl!;
            }
            return _configuration.GetSection("Auth0:Auth0UserInfoByEmailUrl").Value ?? string.Empty;
        }

        private string GetAuth0GrantType()
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.GrantType))
            {
                return authConfig.Auth0.GrantType!;
            }
            //Auth0:grant_type
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



        private string GetAuth0ApiUrl()
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.Auth0ApiUrl))
            {
                return authConfig.Auth0.Auth0ApiUrl!;
            }

            return _configuration.GetSection("Auth0:Auth0ApiUrl").Value ?? string.Empty;
        }

        private string GetAuth0TokenUrl()
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextKeys.AuthConfig, out var authConfigObj) == true &&
                authConfigObj is AuthConfig authConfig &&
                !string.IsNullOrWhiteSpace(authConfig.Auth0?.Auth0TokenUrl))
            {
                return authConfig.Auth0.Auth0TokenUrl!;
            }

            return _configuration.GetSection("Auth0:Auth0TokenUrl").Value ?? string.Empty;
        }

        private async Task<string> GetTenantSecret(string secretKey)
        {
            const string methodName = nameof(GetTenantSecret);

            var tenantCode = _httpContextAccessor?.HttpContext?.Items[HttpContextKeys.TenantCode]?.ToString() ?? string.Empty;

            var secret = await _vault.GetTenantSecret(tenantCode, secretKey);

            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                // Fall back to default secret
                secret = await _vault.GetSecret(secretKey);

                _logger.LogInformation("{Class}.{Method}: Tenant secret not found, fallback used. Status Code: {StatusCode}",
                    className, methodName, StatusCodes.Status500InternalServerError);

                return secret;
            }

            _logger.LogInformation("{Class}.{Method}: Completed processing.", className, methodName);
            return secret;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<GetConsumerByPersonUniqueIdentifierResponseDto> GetConsumerByEmail(string email)
        {
            const string methodName = nameof(GetConsumerByEmail);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("email", HttpUtility.UrlEncode(email));

            try
            {
                var data = await _userClient.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", parameters);
                _logger.LogInformation("{ClassName}.{MethodName} - Email Started Processing GetConsumerBy Email", className, methodName);
                return new GetConsumerByPersonUniqueIdentifierResponseDto
                {
                    Consumer = data?.Consumer ?? Array.Empty<ConsumerDto>(),
                    Person = data?.Person,
                    ErrorCode = data?.ErrorCode,
                    ErrorMessage = data?.ErrorMessage
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error Occurred While performing GetConsumerByEmail,ErrorCode:{Code}, ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<GetConsumerByPersonUniqueIdentifierResponseDto?> GetConsumerByPersonUniqueIdentifier(string personUniqueIdentifier)
        {
            const string methodName = nameof(GetConsumerByPersonUniqueIdentifier);

            var parameters = new Dictionary<string, string>
            {
                { "personUniqueIdentifier", HttpUtility.UrlEncode(personUniqueIdentifier) }
            };

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Starting API call to fetch consumer by PersonUniqueIdentifier", className, methodName);

                var data = await _userClient.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                    $"{UserConstants.GetConsumerByPersonUniqueIdentifierAPIUrl}?personUniqueIdentifier=",
                    parameters);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully retrieved consumer data", className, methodName);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName} - Error while fetching consumer by PersonUniqueIdentifier. ErrorCode: {Code}, Message: {Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<GetConsumerByPersonUniqueIdentifierResponseDto?> GetConsumerByMemberNbrAndRegionCode(string memberNbr, string regionCode)
        {
            const string methodName = nameof(GetConsumerByMemberNbrAndRegionCode);

            var parameters = new Dictionary<string, long>();

            _logger.LogInformation(
                "{ClassName}.{MethodName} - Started. Request: memberNbr={MemberNbr}, regionCode={RegionCode}",
                className, methodName, memberNbr, regionCode);

            try
            {
                var data = await _userClient.Get<ConsumerPersonResponseDto>(
                    $"{UserConstants.GetConsumerByMemNbrAndRegionCodeAPIUrl}?memNbr={memberNbr}&regionCode={regionCode}", parameters);

                _logger.LogInformation(
                    "{ClassName}.{MethodName} - Successfully retrieved consumer data. Response: ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
                    className, methodName, data?.ErrorCode, data?.ErrorMessage);

                return new GetConsumerByPersonUniqueIdentifierResponseDto
                {
                    Consumer = data?.Consumer ?? Array.Empty<ConsumerDto>(),
                    Person = data?.Person,
                    ErrorCode = data?.ErrorCode,
                    ErrorMessage = data?.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{ClassName}.{MethodName} - Exception occurred. memberNbr={MemberNbr}, regionCode={RegionCode}. ErrorCode={ErrorCode}, Message={Message}",
                    className, methodName, memberNbr, regionCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }


        public async Task<GetConsumerByEmailResponseDto> GetConsumerByIdentifierOrEmail(string? email)
        {
            const string methodName = nameof(GetConsumerByIdentifierOrEmail);
            var context = _httpContextAccessor.HttpContext;

            var personUniqueIdentifier = context?.Items[HttpContextKeys.PersonUniqueIdentifier] as string;
            var memberNbr = GetMemberNbrFromHttpContext();
            var regionCode = GetRegionCodeFromHttpContext();

            if (string.IsNullOrWhiteSpace(personUniqueIdentifier) && string.IsNullOrEmpty(memberNbr) && string.IsNullOrEmpty(regionCode))
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    const string errorMessage = "Invalid request: personUniqueIdentifier, email, memberNbr, and regionCode are all missing.";

                    _logger.LogError(
                        "{ClassName}.{MethodName} - {ErrorMessage} ErrorCode: {ErrorCode}",
                        className, methodName, errorMessage, StatusCodes.Status404NotFound);

                    return new GetConsumerByEmailResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = errorMessage
                    };
                }
                personUniqueIdentifier = email;
                _logger.LogWarning("{ClassName}.{MethodName} - PersonUniqueIdentifier not found, falling back to Email", className, methodName);
            }

            var parameters = new Dictionary<string, string>
            {
                { "personUniqueIdentifier", HttpUtility.UrlEncode(personUniqueIdentifier) }
            };

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Starting API call to fetch consumer by PersonUniqueIdentifier", className, methodName);

                var response = await _userClient.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                    $"{UserConstants.GetConsumerByPersonUniqueIdentifierAPIUrl}?personUniqueIdentifier=",
                    parameters);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully retrieved consumer data", className, methodName);

                var consumerResponse = new GetConsumerByEmailResponseDto
                {
                    Consumer = response?.Consumer ?? Array.Empty<ConsumerDto>(),
                    Person = response?.Person,
                    ErrorCode = response?.ErrorCode,
                    ErrorMessage = response?.ErrorMessage
                };
                if (consumerResponse?.Person?.PersonUniqueIdentifier == null && !string.IsNullOrEmpty(memberNbr) && !string.IsNullOrEmpty(regionCode))
                {
                    var consumerResponseData = await GetConsumerByMemberNbrAndRegionCode(memberNbr!, regionCode!);
                    consumerResponse = new GetConsumerByEmailResponseDto
                    {
                        Consumer = consumerResponseData?.Consumer ?? Array.Empty<ConsumerDto>(),
                        Person = consumerResponseData?.Person,
                        ErrorCode = consumerResponseData?.ErrorCode,
                        ErrorMessage = consumerResponseData?.ErrorMessage
                    };
                }

                if (consumerResponse?.Person?.PersonUniqueIdentifier == null)
                {
                    var emilRespones = await GetConsumerByEmail(email ?? string.Empty);
                    consumerResponse = new GetConsumerByEmailResponseDto
                    {
                        Consumer = emilRespones?.Consumer ?? Array.Empty<ConsumerDto>(),
                        Person = emilRespones?.Person,
                        ErrorCode = emilRespones?.ErrorCode,
                        ErrorMessage = emilRespones?.ErrorMessage
                    };
                }
                return consumerResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName} - Error while fetching consumer by PersonUniqueIdentifier. ErrorCode: {Code}, Message: {Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
        /// <summary>
        /// Handles the creation of a consumer device for a specific tenant and consumer. 
        /// Logs the process, prepares the request, and makes an API call to create the consumer device.
        /// Captures any errors and logs them appropriately while maintaining detailed contextual information.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which the device is being created.</param>
        /// <param name="consumerCode">The consumer code for which the device is being created.</param>
        /// <param name="patchUserRequestDto">The request DTO containing device details.</param>
        /// <exception cref="Exception">Rethrows any exception encountered during the process.</exception>
        private async Task CreateConsumerDevice(PatchUserRequestDto patchUserRequestDto, string consumerCode, string tenantCode)
        {
            const string methodName = nameof(CreateConsumerDevice);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} -  Started processing create consumer device with TenantCode:{Code},ConsumerCode:{Consumer}",
                   className, methodName, tenantCode, consumerCode);

                if (string.IsNullOrEmpty(patchUserRequestDto.DeviceId) || string.IsNullOrEmpty(patchUserRequestDto.DeviceType)
                    || string.IsNullOrEmpty(patchUserRequestDto.DeviceAttrJson))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid request. DeviceId or DeviceType or DeviceAttrjson is null or empty", className, methodName);
                    return;
                }
                var postConsumerDeviceRequestDto = new PostConsumerDeviceRequestDto()
                {
                    DeviceId = patchUserRequestDto.DeviceId,
                    DeviceType = patchUserRequestDto.DeviceType,
                    DeviceAttrJson = patchUserRequestDto.DeviceAttrJson,
                    TenantCode = tenantCode,
                    ConsumerCode = consumerCode
                };

                // calling user api to create consumer-device
                var response = await _userClient.Post<BaseResponseDto>(CommonConstants.PostConsumerDeviceUrl, postConsumerDeviceRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while inserting consumer device. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, postConsumerDeviceRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return;
                }
                _logger.LogInformation("{ClassName}.{MethodName} -  Consumer device created sucessfully with TenantCode:{Code},ConsumerCode:{Consumer}",
                  className, methodName, tenantCode, consumerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing  create consumer device with TenantCode:{Code},ConsumerCode:{Consumer},ERROR:{Msg}",
                   className, methodName, tenantCode, consumerCode, ex.Message);
                throw;
            }
        }
        private string? GetUserEmailFromHttpContext()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Items.TryGetValue(HttpContextKeys.Email, out var emailObj) == true ? emailObj as string : null;
        }

        private string? GetHttpContextValue(string key)
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Items.TryGetValue(key, out var value) == true ? value as string : null;
        }

        private bool GetIsSsoUserFromHttpContext()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue(HttpContextKeys.IsSsoUser, out var value) == true)
            {
                if (value is bool boolValue)
                    return boolValue;

                // Fallback: handle string "true"/"false"
                if (value is string str && bool.TryParse(str, out var parsed))
                    return parsed;
            }
            return false; // Default if not present or invalid
        }

        private string? GetPersonUniqueIdentifierFromHttpContext() => GetHttpContextValue(HttpContextKeys.PersonUniqueIdentifier);

        private string? GetAuth0UserNameFromHttpContext() => GetHttpContextValue(HttpContextKeys.UserName);
        private string? GetMemberNbrFromHttpContext() => GetHttpContextValue(HttpContextKeys.MemberNbr);
        private string? GetRegionCodeFromHttpContext() => GetHttpContextValue(HttpContextKeys.RegionCode);

    }
}
