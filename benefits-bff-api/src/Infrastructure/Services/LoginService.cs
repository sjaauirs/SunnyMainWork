using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Core.Domain.Enums;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Fis.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class LoginService : ILoginService
    {

        private readonly ILogger<LoginService> _loginServiceLogger;
        private readonly IUserClient _userClient;
        private readonly IFisClient _fisClient;
        private readonly ICmsService _cmsService;
        private readonly IPersonHelper _personHelper;
        private readonly IEventService _eventService;
        private readonly IVault _vault;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string className = nameof(LoginService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginServiceLogger"></param>
        /// <param name="userClient"></param>
        public LoginService(ILogger<LoginService> loginServiceLogger, IUserClient userClient, IFisClient fisClient, IPersonHelper personHelper, IEventService eventService, IVault vault
            , IHttpContextAccessor httpContextAccessor, ICmsService cmsService)
        {
            _loginServiceLogger = loginServiceLogger;
            _userClient = userClient;
            _fisClient = fisClient;
            _personHelper = personHelper;
            _eventService = eventService;
            _vault = vault;
            _cmsService = cmsService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<GetConsumerByEmailResponseDto> GetConsumerByEmail(string email)
        {
            const string methodName = nameof(GetConsumerByEmail);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("email", HttpUtility.UrlEncode(email));

            try
            {
                var data = await _userClient.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", parameters);
                _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Data Successfully for GetConsumerByEmail", className, methodName);
                return data;
            }
            catch (Exception ex)
            {
                _loginServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while GetConsumerByEmail, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }

        }

        public async Task<GetConsumerByEmailResponseDto> GetConsumerByPersonUniqueIdentifier(string? email)
        {
            const string methodName = nameof(GetConsumerByPersonUniqueIdentifier);
            var context = _httpContextAccessor.HttpContext;

            var personUniqueIdentifier = context?.Items[HttpContextKeys.PersonUniqueIdentifier] as string;

            // Fallback to email if personUniqueIdentifier is null or empty
            if (string.IsNullOrWhiteSpace(personUniqueIdentifier))
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _loginServiceLogger.LogError("{ClassName}.{MethodName} - Both PersonUniqueIdentifier and Email are null or empty", className, methodName);
                    return new GetConsumerByEmailResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Email or PersonUniqueIdentifier is required"
                    };
                }

                personUniqueIdentifier = email;
                _loginServiceLogger.LogWarning("{ClassName}.{MethodName} - PersonUniqueIdentifier not found, falling back to Email", className, methodName);
            }

            var parameters = new Dictionary<string, string>
            {
                { "personUniqueIdentifier", HttpUtility.UrlEncode(personUniqueIdentifier) }
            };

            try
            {
                _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - Starting API call to fetch consumer by PersonUniqueIdentifier", className, methodName);

                var response = await _userClient.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                    $"{UserConstants.GetConsumerByPersonUniqueIdentifierAPIUrl}?personUniqueIdentifier=",
                    parameters);

                _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - Successfully retrieved consumer data", className, methodName);

                var consumerResponse = new GetConsumerByEmailResponseDto
                {
                    Consumer = response?.Consumer ?? Array.Empty<ConsumerDto>(),
                    Person = response?.Person,
                    ErrorCode = response?.ErrorCode,
                    ErrorMessage = response?.ErrorMessage
                };

                if (consumerResponse?.Person?.PersonUniqueIdentifier == null)
                {
                    consumerResponse = await GetConsumerByEmail(email ?? string.Empty);
                }
                return consumerResponse;
            }
            catch (Exception ex)
            {
                _loginServiceLogger.LogError(ex,
                    "{ClassName}.{MethodName} - Error while fetching consumer by PersonUniqueIdentifier. ErrorCode: {Code}, Message: {Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }




        public async Task<GetConsumerByEmailResponseDto> GetPersonAndConsumerDetails(string consumerCode)
        {
            const string methodName = nameof(GetPersonAndConsumerDetails);
            var response = new GetConsumerByEmailResponseDto();

            try
            {
                var request = new GetConsumerRequestDto { ConsumerCode = consumerCode };
                var personAndConsumer = await _userClient.Post<GetPersonAndConsumerResponseDto>(
                    CommonConstants.GetPersonAndConsumerAPIUrl, request);

                if (personAndConsumer?.Consumer != null && personAndConsumer.Person != null)
                {
                    response.Consumer = new[] { personAndConsumer.Consumer };
                    response.Person = personAndConsumer.Person;

                    _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - Successfully retrieved consumer and person for ConsumerCode: {ConsumerCode}", className, methodName, consumerCode);
                }
                else
                {
                    _loginServiceLogger.LogWarning("{ClassName}.{MethodName} - Consumer or Person not found for ConsumerCode: {ConsumerCode}", className, methodName, consumerCode);
                }

                return response;
            }
            catch (Exception ex)
            {
                _loginServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while retrieving consumer and person, ConsumerCode: {ConsumerCode}, ErrorCode: {ErrorCode}, Message: {Message}",
                    className, methodName, consumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }


        public async Task<VerifyMemberResponseDto> VerifyMember(VerifyMemberDto verifyMemberDto)
        {
            const string methodName = nameof(VerifyMember);
            var response = new VerifyMemberResponseDto { ErrorCode = StatusCodes.Status400BadRequest };

            // Fetch consumer details
            var consumerDetails = new GetConsumerByEmailResponseDto();
            if (!string.IsNullOrEmpty(verifyMemberDto.ConsumerCode))
            {
                consumerDetails = await GetPersonAndConsumerDetails(verifyMemberDto.ConsumerCode);
            }
            else
            {
                consumerDetails = await GetConsumerByPersonUniqueIdentifier(verifyMemberDto.Email);
            }

            if (HandleConsumerErrors(consumerDetails, verifyMemberDto.Email, response, methodName))
                return response;

            _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved consumer detail for consumer : {Email}", className, methodName, verifyMemberDto.Email);
            // Check VerifyOps and handle accordingly
            var consumer = consumerDetails.Consumer[0];
            
            switch (verifyMemberDto.verifyOps.ToUpper().Trim())
            {
                case nameof(VerifyOps.DOB):
                    await HandleDOBVerification(consumerDetails, verifyMemberDto, response, methodName);
                    break;

                case nameof(VerifyOps.CARDLAST4):
                    await HandleCardLast4Verification(consumerDetails, verifyMemberDto, response, methodName);
                    break;

                case nameof(VerifyOps.PICK_A_PURSE_COMPLETED):
                    await UpdateOnboardingState(consumerDetails, OnboardingState.PICK_A_PURSE_COMPLETED, verifyMemberDto.Email, response, methodName);

                    break;

                case nameof(VerifyOps.COSTCO_ACTIONS_VISITED):
                    await UpdateOnboardingState(consumerDetails, OnboardingState.COSTCO_ACTIONS_VISITED, verifyMemberDto.Email, response, methodName);
                    break;

                case nameof(VerifyOps.DECLINED):
                    await UpdateAgreementStatus(consumerDetails, response, verifyMemberDto.verifyOps.ToUpper().Trim());
                    break;
                case nameof(VerifyOps.AGREEMENT_VERIFIED):
                    Dictionary<string, string>? htmlAgreementfileName = await GetAgreementUrl(verifyMemberDto.ComponentCode, verifyMemberDto.LanguageCode);
                    if (htmlAgreementfileName.Count > 0)
                    {
                        _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - Process started for verified OnboardingState for email : {Email}", className, methodName, verifyMemberDto.Email);
                        await UpdateOnboardingState(consumerDetails, OnboardingState.AGREEMENT_VERIFIED, verifyMemberDto.Email, response, methodName, htmlAgreementfileName, verifyMemberDto.LanguageCode);
                    }
                    else
                    {
                        _loginServiceLogger.LogError("{ClassName}.{MethodName} - Agreement pdf upload failed for consumer with verify Member request Dto : {verifyMemberDto}", className, methodName, verifyMemberDto.ToJson());
                        response.ErrorCode = StatusCodes.Status500InternalServerError;
                            response.ErrorMessage = "Agreement Pdf not Uploaded";
                    }
                    break;

                case nameof(VerifyOps.VERIFIED):

                    _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - Process started for verified OnboardingState for email : {Email}", className, methodName, verifyMemberDto.Email);
                    await UpdateOnboardingState(consumerDetails, OnboardingState.VERIFIED, verifyMemberDto.Email, response, methodName);
                    await _personHelper.UpdateOnBoardingTask(consumer);

                    //call event for pick a purse

                    var consumerAccountDto = new ConsumerAccountDto
                    {
                        ConsumerCode = consumer.ConsumerCode,
                        TenantCode = consumer.TenantCode
                    };
                    await _eventService.CreatePickAPurseEvent(consumerAccountDto);

                    break;
            }

            return response;
        }
        private async Task<Dictionary<string, string>> GetAgreementUrl(string componentCode, string languageCode)
        {
            const string methodName = nameof(GetAgreementUrl);

            try
            {
                _loginServiceLogger.LogInformation(
                    "{ClassName}.{MethodName} - Sending request to get agreement html link for component Code {componentCode}",
                    className, methodName, componentCode);

                var getComponentRequestDto = new GetComponentByCodeRequestDto
                {
                    componentCode = componentCode
                };

                var agreementComponent = await _cmsService.GetComponentBycode(getComponentRequestDto);

                if (agreementComponent.ErrorCode != null)
                {
                    _loginServiceLogger.LogError(
                        "{ClassName}.{MethodName} - Error in getting html link for component Code {componentCode}, response {response}",
                        className, methodName, componentCode, agreementComponent.ToJson());
                    return new Dictionary<string, string>();
                }

                _loginServiceLogger.LogInformation(
                    "{ClassName}.{MethodName} - Response of getting html link for componentCode {componentCode}, response {response}",
                    className, methodName, componentCode, agreementComponent.ToJson());

                if (string.IsNullOrEmpty(agreementComponent.Component.DataJson))
                {
                    _loginServiceLogger.LogError(
                        "{ClassName}.{MethodName} - DataJson is null or empty",
                        className, methodName);
                    return new Dictionary<string, string>();
                }

                // Deserialize JSON into model
                var dto = JsonConvert.DeserializeObject<AgreementDataComponentDto>(agreementComponent.Component.DataJson);

                if (dto?.Data?.Agreements == null || !dto.Data.Agreements.Any())
                {
                    _loginServiceLogger.LogWarning(
                        "{ClassName}.{MethodName} - No agreements found for componentCode {componentCode}",
                        className, methodName, componentCode);
                    return new Dictionary<string, string>();
                }

                // Convert List<Agreement> → Dictionary<string, string>
                // Add "Agreement" suffix (no space)
                var agreementDict = dto.Data.Agreements
                    .Where(a => !string.IsNullOrWhiteSpace(a.DisplayName) && !string.IsNullOrWhiteSpace(a.Url))
                    .ToDictionary(
                        a =>
                        {
                            var name = a.DisplayName.Trim().Replace(" ", string.Empty); // remove all spaces
                            if (!name.Contains("Agreement", StringComparison.OrdinalIgnoreCase))
                                name += "Agreement"; // 👈 no space before "Agreement"
                            return name;
                        },
                        a => GetHtmlFileNameFromUrl(a.Url)
                    );

                return agreementDict;
            }
            catch (Exception ex)
            {
                _loginServiceLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName}: ERROR - msg : {msg}",
                    className, methodName, ex.Message);

                return new Dictionary<string, string>();
            }
        }



        public async Task<LoginResponseDto> InternalLogin(LoginRequestDto loginRequestDto)
        {
            const string methodName = nameof(InternalLogin);
            try
            {
                _loginServiceLogger.LogInformation("{ClassName}.{MethodName} Start processing. {Payload}", className, methodName, loginRequestDto.ToJson());

                string env = await _vault.GetSecret(CommonConstants.Env);
                if (string.IsNullOrWhiteSpace(env) || env.Equals(_vault.InvalidSecret, StringComparison.OrdinalIgnoreCase) || env.Equals(EnvironmentConstants.Production, StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginResponseDto
                    {
                        ErrorCode = StatusCodes.Status403Forbidden,
                        ErrorDescription = "Not Allowed for Production environment"
                    };
                }

                var consumerLoginRequestDto = new ConsumerLoginRequestDto()
                {
                    ConsumerCode = loginRequestDto.ConsumerCode, // if ConsumerCode is set, other params above are not required
                };

                var userResponse = await _userClient.Post<ConsumerLoginResponseDto>(CommonConstants.ConsumerLoginUrl, consumerLoginRequestDto);
                _loginServiceLogger.LogInformation("{className}.{methodName}: Retrieved JWT Token Successfully for ConsumerCode: {ConsumerCode}", className, methodName, consumerLoginRequestDto.ConsumerCode);
                if (string.IsNullOrWhiteSpace(userResponse.Jwt))
                {
                    return new LoginResponseDto
                    {
                        ErrorCode = userResponse.ErrorCode ?? StatusCodes.Status404NotFound,
                        ErrorMessage = "No consumer found for the given Consumer Code."
                    };
                }

                //await SendPushNotification(tenant?.TenantCode ?? string.Empty, userResponse.ConsumerCode ?? string.Empty);

                return new LoginResponseDto() { ConsumerCode = userResponse.ConsumerCode, Jwt = userResponse.Jwt };
            }
            catch (Exception ex)
            {
                _loginServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }

        private bool HandleConsumerErrors(GetConsumerByEmailResponseDto consumerDetails, string? email, VerifyMemberResponseDto response, string methodName)
        {
            if (consumerDetails.ErrorCode != null)
            {
                LogAndSetResponse(response, methodName, StatusCodes.Status404NotFound, $"Consumer not found for Email : {email}");
                return true;
            }
            if (consumerDetails.Consumer.Length != 1)
            {
                LogAndSetResponse(response, methodName, StatusCodes.Status422UnprocessableEntity, $"{consumerDetails.Consumer.Length} consumers found for Email : {email}");
                return true;
            }
            if (!string.IsNullOrEmpty(email)
                && !string.IsNullOrEmpty(consumerDetails.Person?.Email) && !string.Equals(email, consumerDetails.Person?.Email, StringComparison.OrdinalIgnoreCase))
            {
                LogAndSetResponse(response, methodName, StatusCodes.Status400BadRequest, $"{email} is an invalid Email adddres");
                return true;
            }
            return false;
        }

        private async Task HandleDOBVerification(GetConsumerByEmailResponseDto consumerDetails, VerifyMemberDto verifyMemberDto, VerifyMemberResponseDto response, string methodName)
        {
            if (verifyMemberDto.DOB == null)
            {
                LogAndSetResponse(response, methodName, StatusCodes.Status400BadRequest, $"Please provide date of Birth for Person with Email: {verifyMemberDto.Email}");
                return;
            }

            if (consumerDetails.Person?.DOB.Date == verifyMemberDto.DOB?.Date)
            {
                await UpdateOnboardingState(consumerDetails, OnboardingState.DOB_VERIFIED, verifyMemberDto.Email, response, methodName);
            }
            else
            {
                LogAndSetResponse(response, methodName, StatusCodes.Status422UnprocessableEntity, $"Date Of Birth not Matched for person with Email: {verifyMemberDto.Email}");
            }
        }

        private async Task HandleCardLast4Verification(GetConsumerByEmailResponseDto consumerDetails, VerifyMemberDto verifyMemberDto, VerifyMemberResponseDto response, string methodName)
        {
            if (string.IsNullOrEmpty(verifyMemberDto.CardLast4))
            {
                LogAndSetResponse(response, methodName, StatusCodes.Status400BadRequest, $"Please provide Card Last 4 digits for Email: {verifyMemberDto.Email}");
                return;
            }

            var consumer = consumerDetails.Consumer[0];
            var fisRequest = new VerifyFisMemberDto { CardLast4 = verifyMemberDto.CardLast4, TenantCode = consumer.TenantCode, ConsumerCode = consumer.ConsumerCode, CardActivationChannel = verifyMemberDto.CardActivationChannel };
            var fisResponse = await _fisClient.Post<VerifyMemberResponseDto>("verify-member-info", fisRequest);

            if (fisResponse.ErrorCode == StatusCodes.Status200OK)
            {
                if (consumer.OnBoardingState != OnboardingState.VERIFIED.ToString())
                {
                    await UpdateOnboardingState(consumerDetails, OnboardingState.CARD_LAST_4_VERIFIED, verifyMemberDto.Email, response, methodName);
                }
                else
                {
                    response.ErrorCode = StatusCodes.Status200OK;
                    response.ErrorMessage = "Card Last 4 verified";
                }
            }
            else
            {
                LogAndSetResponse(response, methodName, fisResponse.ErrorCode ?? 500, $"Error occurred in FIS verify member info for Email: {verifyMemberDto.Email}");
            }
        }

        private async Task UpdateOnboardingState(GetConsumerByEmailResponseDto consumerDetails, OnboardingState state, string email, VerifyMemberResponseDto response, string methodName, Dictionary<string, string>? htmlfileName = null, string? languageCode = null)
        {
            var isUpdated = await _personHelper.UpdateOnBoardingState(new UpdateOnboardingStateDto
            {
                ConsumerCode = consumerDetails.Consumer[0].ConsumerCode,
                TenantCode = consumerDetails.Consumer[0].TenantCode!,
                OnboardingState = state,
                HtmlFileName = htmlfileName,
                LanguageCode = languageCode


            });

            response.ErrorCode = isUpdated ? StatusCodes.Status200OK : StatusCodes.Status422UnprocessableEntity;
            response.ErrorMessage = isUpdated
                ? $"OnBoarding state updated to {state.ToString()} for person with Email: {email}"
                : $"OnBoarding state not updated for person with Email: {email}";

            _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - {Message}, ErrorCode:{ErrorCode}", className, methodName, response.ErrorMessage, response.ErrorCode);
        }

        private async Task UpdateAgreementStatus(GetConsumerByEmailResponseDto consumerDetails, VerifyMemberResponseDto response, string agreementStatus)
        {
            var methodName = nameof(GetConsumerByEmailResponseDto);

            if (consumerDetails.Consumer == null || consumerDetails?.Consumer?.Length == 0)
            {
                response.ErrorCode = StatusCodes.Status404NotFound;
                response.ErrorMessage = "Consumer details not found.";
                return;
            }

            var consumerInfo = consumerDetails?.Consumer[0];

            var updateAgreementStatusDto = new UpdateAgreementStatusDto
            {
                ConsumerCode = consumerInfo?.ConsumerCode!,
                TenantCode = consumerInfo?.TenantCode!,
                AgreementStatus = agreementStatus
            };

            var consumerResponse = await _userClient.Put<ConsumerResponseDto>(
                UserConstants.UpdateConsumerAgreementStatusAPIUrl,
                updateAgreementStatusDto);

            if (consumerResponse?.ErrorCode != null)
            {
                _loginServiceLogger.LogError(
                    "{ClassName}.{MethodName} - Failed to update consumer. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    className,
                    methodName,
                    updateAgreementStatusDto.ConsumerCode,
                    updateAgreementStatusDto.TenantCode,
                    consumerResponse.ErrorCode,
                    consumerResponse.ErrorMessage);

                response.ErrorMessage = consumerResponse.ErrorMessage;
                response.ErrorCode = consumerResponse.ErrorCode;
                return;
            }

            response.ErrorCode = StatusCodes.Status200OK;
        }


        private void LogAndSetResponse(VerifyMemberResponseDto response, string methodName, int errorCode, string errorMessage)
        {
            response.ErrorCode = errorCode;
            response.ErrorMessage = errorMessage;
            _loginServiceLogger.LogInformation("{ClassName}.{MethodName} - {ErrorMessage}, ErrorCode:{ErrorCode}", className, methodName, errorMessage, errorCode);
        }


        public string GetHtmlFileNameFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }
            var uri = new Uri(url);
            return Path.GetFileName(uri.AbsolutePath);
        }
    }
}
