using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class PersonAddressService : IPersonAddressService
    {
        private readonly ILogger<PersonAddressService> _logger;
        private readonly IUserClient _userClient;
        private readonly IFisClient _fisClient;
        private readonly IMapper _mapper;
        private const string className = nameof(PersonAddressService);
        public PersonAddressService(ILogger<PersonAddressService> logger, IUserClient userClient, IMapper mapper, IFisClient fisClient)
        {
            _logger = logger;
            _userClient = userClient;
            _mapper = mapper;
            _fisClient = fisClient;
        }

        public async Task<GetAllPersonAddressesResponseDto> GetAllPersonAddresses(long personId)
        {
            const string methodName = nameof(GetAllPersonAddresses);
            _logger.LogInformation("{ClassName}.{MethodName} : Started getting all addresses for person with ID:{PersonId}",
                        className, methodName, personId);
            try
            {
                var parameters = new Dictionary<string, string>();
                var response = await _userClient.GetId<GetAllPersonAddressesResponseDto>($"persons-address/{personId}/get-all-addresses", parameters);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while getting addresses for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, personId, response.ErrorCode, response.ErrorMessage);
                    return response;
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully retrieved addresses for person with ID:{PersonId}",
                   className, methodName, personId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while getting addresses for person with ID:{PersonId},ERROR:{Msg}",
                        className, methodName, personId, ex.Message);
                return new GetAllPersonAddressesResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                    PersonAddressesList = new List<PersonAddressDto>()
                };
            }
        }

        public async Task<PersonAddressResponseDto> CreatePersonAddress(CreatePersonAddressRequestDto request)
        {
            const string methodName = nameof(CreatePersonAddress);
            _logger.LogInformation("{ClassName}.{MethodName} : Started creating address for person with ID:{PersonId}",
                        className, methodName, request.PersonId);
            try
            {
                var response = await _userClient.Post<PersonAddressResponseDto>("persons-address/post-address", request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating address for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PersonId, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully created address for person with ID:{PersonId}",
                   className, methodName, request.PersonId);

                if (request.AddressTypeId == (long)AddressTypeEnum.MAILING)
                {
                    // Set new address to primary address
                    UpdatePrimaryPersonAddressRequestDto setPrimaryAddressrequest = new UpdatePrimaryPersonAddressRequestDto()
                    {
                        PersonAddressId = response.PersonAddress.PersonAddressId,
                        UpdateUser = request.CreateUser
                    };

                    var setPrimaryAddressResponse = await _userClient.Put<PersonAddressResponseDto>("persons-address/set-primary", setPrimaryAddressrequest);
                    if (setPrimaryAddressResponse.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}: Failed to set new address to primary address for person ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                            className, methodName, setPrimaryAddressrequest.PersonAddressId, setPrimaryAddressResponse.ErrorCode, setPrimaryAddressResponse.ErrorMessage);
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} : Successfully set new address to primary address for person ID:{PersonId}",
                            className, methodName, request.PersonId);
                        response.PersonAddress.IsPrimary = true;
                        await UpdateFisSyncRequiredFlag(request.ConsumerCode, request.TenantCode, new List<string> { SyncOptions.ADDRESS_CHANGE.ToString() });
                    }
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while creating address for person with ID:{PersonId},ERROR:{Msg}",
                        className, methodName, request.PersonId, ex.Message);
                return new PersonAddressResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PersonAddressResponseDto> UpdatePersonAddress(UpdatePersonAddressRequestDto request, bool markAsPrimary)
        {
            const string methodName = nameof(UpdatePersonAddress);
            _logger.LogInformation("{ClassName}.{MethodName} : Started updating address for person address ID:{PersonAddressId}",
                        className, methodName, request.PersonAddressId);
            try
            {
                var response = await _userClient.Put<PersonAddressResponseDto>("persons-address/update-address", request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating address for person address ID:{PersonAddressId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PersonAddressId, response.ErrorCode, response.ErrorMessage);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully updated address for person address ID:{PersonAddressId}",
                   className, methodName, request.PersonAddressId);

                // If the address is primary, then update the FIS sync required flag
                if (response.PersonAddress?.IsPrimary == true)
                {
                    await UpdateFisSyncRequiredFlag(request.ConsumerCode, request.TenantCode, new List<string> { SyncOptions.ADDRESS_CHANGE.ToString() });
                }

                if (markAsPrimary)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} : Started setting address to primary for person address ID:{PersonAddressId}",
                       className, methodName, request.PersonAddressId);
                    UpdatePrimaryPersonAddressRequestDto setPrimaryAddressrequest = new UpdatePrimaryPersonAddressRequestDto()
                    {
                        PersonAddressId = request.PersonAddressId,
                        UpdateUser = request.UpdateUser
                    };

                    var setPrimaryAddressResponse = await _userClient.Put<PersonAddressResponseDto>("persons-address/set-primary", setPrimaryAddressrequest);
                    if (setPrimaryAddressResponse.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}: Failed to set address to primary address for PersonAddressId:{PersonAddressId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                            className, methodName, setPrimaryAddressrequest.PersonAddressId, setPrimaryAddressResponse.ErrorCode, setPrimaryAddressResponse.ErrorMessage);
                        return setPrimaryAddressResponse;
                    }

                    _logger.LogInformation("{ClassName}.{MethodName} : Successfully set address to primary for person address ID:{PersonAddressId} to primary address",
                        className, methodName, request.PersonAddressId);
                    await UpdateFisSyncRequiredFlag(request.ConsumerCode, request.TenantCode, new List<string> { SyncOptions.ADDRESS_CHANGE.ToString() });

                    return setPrimaryAddressResponse;
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while updating address for person address ID:{PersonAddressId},ERROR:{Msg}",
                        className, methodName, request.PersonAddressId, ex.Message);
                return new PersonAddressResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task UpdateFisSyncRequiredFlag(string consumerCode, string tenantCode, List<string> syncOptions)
        {
            const string methodName = nameof(UpdateFisSyncRequiredFlag);
            var consumerAccountResponseDtos = new List<ConsumerAccountResponseDto>();
            var consumerAccountDto = new List<ConsumerAccountDto>
            {
                new ConsumerAccountDto
                {
                    ConsumerCode = consumerCode,
                    TenantCode = tenantCode,
                    SyncOptions = syncOptions,
                }
            };
            consumerAccountResponseDtos = await _fisClient.Put<List<ConsumerAccountResponseDto>>(UserConstants.UpdateSyncRequiredFlagUrl, consumerAccountDto);
            if (consumerAccountResponseDtos != null && consumerAccountResponseDtos.Count > 0)
            {
                var record = consumerAccountResponseDtos.FirstOrDefault();

                if (!string.IsNullOrEmpty(record?.ErrorMessage))
                {
                    var message = $"Error code - {record.ErrorCode}, Error message - {record.ErrorMessage}";
                    _logger.LogError("{ClassName}.{MethodName}: Failed to update FIS sync required flag for consumer code:{consumerCode}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                    className, methodName, consumerCode, record.ErrorCode, record.ErrorMessage);
                }
            }
        }
    }
}
