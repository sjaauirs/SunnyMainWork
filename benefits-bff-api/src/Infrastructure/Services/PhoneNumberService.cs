using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class PhoneNumberService : IPhoneNumberService
    {
        private readonly ILogger<PhoneNumberService> _logger;
        private readonly IUserClient _userClient;
        private readonly IMapper _mapper;
        private const string className = nameof(PhoneNumberService);

        public PhoneNumberService(ILogger<PhoneNumberService> logger, IUserClient userClient, IMapper mapper)
        {
            _logger = logger;
            _userClient = userClient;
            _mapper = mapper;
        }

        public async Task<GetAllPhoneNumbersResponseDto> GetAllPhoneNumbers(long personId)
        {
            const string methodName = nameof(GetAllPhoneNumbers);
            _logger.LogInformation("{ClassName}.{MethodName} : Started getting all phone numbers for person with ID:{PersonId}",
                        className, methodName, personId);
            try
            {
                var parameters = new Dictionary<string, string>();
                var response = await _userClient.GetId<GetAllPhoneNumbersResponseDto>($"phone-number/{personId}/get-all-numbers", parameters);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while getting phone numbers for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, personId, response.ErrorCode, response.ErrorMessage);
                    return response;
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully retrieved phone numbers for person with ID:{PersonId}",
                   className, methodName, personId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while getting phone numbers for person with ID:{PersonId},ERROR:{Msg}",
                        className, methodName, personId, ex.Message);
                return new GetAllPhoneNumbersResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                    PhoneNumbersList = new List<PhoneNumberDto>()
                };
            }
        }

        public async Task<PhoneNumberResponseDto> CreatePhoneNumber(CreatePhoneNumberRequestDto request)
        {
            const string methodName = nameof(CreatePhoneNumber);
            _logger.LogInformation("{ClassName}.{MethodName} : Started creating phone number for person with ID:{PersonId}",
                        className, methodName, request.PersonId);
            try
            {
                var response = await _userClient.Post<PhoneNumberResponseDto>("phone-number/post-number", request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating phone number for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PersonId, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully created phone number for person with ID:{PersonId}",
                   className, methodName, request.PersonId);

                if (request.PhoneTypeId == (long)PhoneTypeEnum.MOBILE)
                {
                    UpdatePrimaryPhoneNumberRequestDto setPrimaryPhoneRequest = new UpdatePrimaryPhoneNumberRequestDto()
                    {
                        PhoneNumberId = response.PhoneNumber.PhoneNumberId,
                        UpdateUser = request.CreateUser
                    };

                    var setPrimaryPhoneResponse = await _userClient.Put<PhoneNumberResponseDto>("phone-number/set-primary", setPrimaryPhoneRequest);
                    if (setPrimaryPhoneResponse.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}: Failed to set new phone number to primary for person ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                            className, methodName, request.PersonId, setPrimaryPhoneResponse.ErrorCode, setPrimaryPhoneResponse.ErrorMessage);
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} : Successfully set new phone number to primary for person ID:{PersonId}",
                            className, methodName, request.PersonId);
                        response.PhoneNumber.IsPrimary = true;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while creating phone number for person with ID:{PersonId},ERROR:{Msg}",
                        className, methodName, request.PersonId, ex.Message);
                return new PhoneNumberResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PhoneNumberResponseDto> UpdatePhoneNumber(UpdatePhoneNumberRequestDto request, bool markAsPrimary)
        {
            const string methodName = nameof(UpdatePhoneNumber);
            _logger.LogInformation("{ClassName}.{MethodName} : Started updating phone number with ID:{PhoneNumberId}",
                        className, methodName, request.PhoneNumberId);
            try
            {
                var response = await _userClient.Put<PhoneNumberResponseDto>("phone-number/update-number", request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating phone number with ID:{PhoneNumberId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PhoneNumberId, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully updated phone number with ID:{PhoneNumberId}",
                   className, methodName, request.PhoneNumberId);

                if (markAsPrimary)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} : Started setting phone number to primary with ID:{PhoneNumberId}",
                       className, methodName, request.PhoneNumberId);

                    UpdatePrimaryPhoneNumberRequestDto setPrimaryPhoneRequest = new UpdatePrimaryPhoneNumberRequestDto()
                    {
                        PhoneNumberId = request.PhoneNumberId,
                        UpdateUser = request.UpdateUser
                    };

                    var setPrimaryPhoneResponse = await _userClient.Put<PhoneNumberResponseDto>("phone-number/set-primary", setPrimaryPhoneRequest);
                    if (setPrimaryPhoneResponse.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}: Failed to set phone number to primary with ID:{PhoneNumberId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                            className, methodName, request.PhoneNumberId, setPrimaryPhoneResponse.ErrorCode, setPrimaryPhoneResponse.ErrorMessage);
                        return setPrimaryPhoneResponse;
                    }

                    _logger.LogInformation("{ClassName}.{MethodName} : Successfully set phone number with ID:{PhoneNumberId} to primary",
                        className, methodName, request.PhoneNumberId);
                    response = setPrimaryPhoneResponse;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while updating phone number with ID:{PhoneNumberId},ERROR:{Msg}",
                        className, methodName, request.PhoneNumberId, ex.Message);
                return new PhoneNumberResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
