using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;


namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("/api/v1/consumer/")]
    [ApiController]
    public class ConsumerController : ControllerBase
    {
        private readonly ILogger<ConsumerController> _consumerLogger;
        private readonly IConsumerService _consumerService;

        /// <summary>
        /// Get Consumer Data Constructor
        /// </summary>
        /// <param name="consumerLogger"></param>
        /// <param name="consumerService"></param>
        public ConsumerController(ILogger<ConsumerController> consumerLogger, IConsumerService consumerService)
        {
            _consumerLogger = consumerLogger;
            _consumerService = consumerService;
        }
        const string className = nameof(ConsumerController);
        /// <summary>
        /// Retrieves consumer details based on the provided request data.
        /// </summary>
        /// <param name="consumerRequestDto">The request data containing the consumer code.</param>
        /// <returns>A response containing the consumer details or an error message.</returns>
        [HttpPost("get-consumer")]
        public async Task<ActionResult<GetConsumerResponseDto>> GetConsumer([FromBody] GetConsumerRequestDto consumerRequestDto)
        {
            var response = new GetConsumerResponseDto();
            const string methodName = nameof(GetConsumer);
            try
            {
                if (!string.IsNullOrEmpty(consumerRequestDto.ConsumerCode))
                {
                    _consumerLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode :{ConsumerCode}", className, methodName, consumerRequestDto.ConsumerCode);
                    response = await _consumerService.GetConsumerData(consumerRequestDto);
                }
                return response != null && response.Consumer != null ? Ok(response) : NotFound(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new GetConsumerResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// Updates the onboarding state for a consumer based on the provided data.
        /// </summary>
        /// <param name="updateOnboardingStateDto">The data containing the consumerCode and new onboarding state.</param>
        /// <returns>A response containing the updated consumer or an error message.</returns>
        [HttpPatch]
        public async Task<ActionResult<ConsumerResponseDto>> OnboardingState(UpdateOnboardingStateDto updateOnboardingStateDto)
        {
            const string methodName = nameof(OnboardingState);
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}: API -  OnBoardingState Started with ConsumerCode : {consumerCode}", className, methodName, updateOnboardingStateDto.ConsumerCode);

                var response = await _consumerService.UpdateOnboardingState(updateOnboardingStateDto);
                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError("{ClassName}.{MethodName}: Error occurred while Person OnBoarding State. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, updateOnboardingStateDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _consumerLogger.LogInformation("{ClassName}.{MethodName}: Person On Boarding Status successful, ConsumerCode: {ConsumerCode}", className, methodName, response.Consumer.ConsumerCode ?? "Consumer not found");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {Msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a consumer that matches the given TenantCode and MemNbr.
        /// </summary>
        /// <param name="consumerRequestDto">The request data containing the TenantCode and MemNbr.</param>
        /// <returns>A response containing the consumer details or an error message.</returns>
        [HttpPost("get-consumer-by-memid")]
        public async Task<ActionResult<GetConsumerByMemIdResponseDto>> GetConsumerByMemId([FromBody] GetConsumerByMemIdRequestDto consumerRequestDto)
        {
            var response = new GetConsumerByMemIdResponseDto();
            const string methodName = nameof(GetConsumerByMemId);
            try
            {
                if (!string.IsNullOrEmpty(consumerRequestDto.TenantCode) &&
                    !string.IsNullOrEmpty(consumerRequestDto.MemberId))
                {
                    _consumerLogger.LogInformation("{className}.{methodName}: API - Started with TenantCode: {tenant}, MemberId: {memnbr}", className, methodName,
                        consumerRequestDto.TenantCode, consumerRequestDto.MemberId);
                    response = await _consumerService.GetConsumerByMemId(consumerRequestDto);
                }
                return response.Consumer != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new GetConsumerByMemIdResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
        /// <summary>
        /// Creates consumers based on the provided data.
        /// </summary>
        /// <param name="consumerDataDto">The list of consumer data to be created.</param>
        /// <returns>A list of responses containing the result of the consumer creation process.</returns>
        [HttpPost("post-consumer")]
        public async Task<ActionResult<List<ConsumerDataResponseDto>>> CreateConsumers([FromBody] IList<ConsumerDataDto> consumerDataRequestDto)
        {
            const string methodName = nameof(CreateConsumers);
            try
            {
                var response = await _consumerService.CreateConsumers(consumerDataRequestDto);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new List<ConsumerDataResponseDto>();
            }
        }

        /// <summary>
        /// Updates consumers based on the provided data.
        /// </summary>
        /// <param name="consumersUpdateRequestDto">The list of consumer data to be updated.</param>
        /// <returns>A list of responses containing the result of the consumer update process.</returns>
        [HttpPost("update-consumers")]
        public async Task<ActionResult<List<ConsumerDataResponseDto>>> UpdateConsumers([FromBody] IList<ConsumerDataDto> consumersUpdateRequestDto)
        {
            try
            {
                var response = await _consumerService.UpdateConsumers(consumersUpdateRequestDto);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.UpdateConsumers: Failed Processing Error Code:{errorCode} and ERROR - msg: {msg}", className, StatusCodes.Status500InternalServerError, ex.Message);
                return new List<ConsumerDataResponseDto>();
            }
        }

        /// <summary>
        /// Cancels consumers based on the provided data.
        /// </summary>
        /// <param name="cancelConsumersRequestDto">The list of consumer data to be canceled.</param>
        /// <returns>A list of responses containing the result of the consumer cancellation process.</returns>
        [HttpPost("cancel-consumers")]
        public async Task<ActionResult<List<ConsumerDataResponseDto>>> CancelConsumers([FromBody] IList<ConsumerDataDto> consumersCancelRequestDto)
        {
            try
            {
                var response = await _consumerService.UpdateConsumers(consumersCancelRequestDto, true, false);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.CancelConsumers: Failed Processing Error Code:{errorCode} and ERROR - msg: {msg}", className, StatusCodes.Status500InternalServerError, ex.Message);
                return new List<ConsumerDataResponseDto>();
            }
        }

        /// <summary>
        /// Soft-deletes consumers based on the provided data.
        /// </summary>
        /// <param name="consumersDeleteRequestDto">The list of consumer data to be soft-deleted.</param>
        /// <returns>A response containing the result of the consumer deletion process.</returns>
        [HttpPost("delete-consumers")]
        public async Task<ActionResult<DeleteConsumersResponseDto>> DeleteConsumers([FromBody] IList<ConsumerDataDto> consumersDeleteRequestDto)
        {
            try
            {
                var response = await _consumerService.UpdateConsumers(consumersDeleteRequestDto, false, true);
                if (response != null && response.Count > 0)
                {
                    return Ok(new DeleteConsumersResponseDto()
                    {
                        ConsumersData = response
                    });
                }
                else
                {
                    var errorMessage = $"DeleteConsumers: Consumers not found, consumers - {consumersDeleteRequestDto.ToJson()}";
                    _consumerLogger.LogError(errorMessage);
                    return StatusCode(StatusCodes.Status404NotFound, new DeleteConsumersResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = errorMessage });
                }

            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.DeleteConsumers: Failed processing Error Code:{errorCode} and ERROR - msg: {msg}", className, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new DeleteConsumersResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves consumer attributes based on the provided request data.
        /// </summary>
        /// <param name="consumerAttributesRequestDto">The request data containing consumer attribute filters.</param>
        /// <returns>A response containing the consumer attributes or an error message.</returns>
        [HttpPost("consumer-attributes")]
        public async Task<ActionResult<ConsumerAttributesResponseDto>> ConsumerAttributes([FromBody] ConsumerAttributesRequestDto consumerAttributesRequestDto)
        {
            const string methodName = nameof(ConsumerAttributes);
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}:ConsumerAttributes API - Started TenantCode: {tenantCode}", className, methodName, consumerAttributesRequestDto.TenantCode);
                var response = await _consumerService.ConsumerAttributes(consumerAttributesRequestDto);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new ConsumerAttributesResponseDto();
            }
        }

        /// <summary>
        /// Retrieves consumer information by email.
        /// </summary>
        /// <param name="email">The email of the consumer to retrieve.</param>
        /// <returns>A response containing the consumer details or an error message.</returns>
        [HttpGet("get-consumers-by-email")]
        public async Task<ActionResult<GetConsumerByEmailResponseDto>> GetConsumerByEmail(string email)
        {
            const string methodName = nameof(GetConsumerByEmail);
            try
            {

                _consumerLogger.LogInformation("{className}.{methodName} API - Started with Email", className, methodName);
                var response = await _consumerService.GetConsumerByEmail(email);

                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }

            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new GetConsumerByEmailResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// Updates the registration flag for the specified consumer.
        /// </summary>
        /// <param name="consumer">The consumer data for which the registration flag needs to be updated.</param>
        /// <returns>The updated consumer data or an error message.</returns>
        [HttpPost("update-register-flag")]
        public async Task<ActionResult<ConsumerModel>> updateRegisterFlag([FromBody] ConsumerDto consumer)
        {
            const string methodName = nameof(updateRegisterFlag);
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}: API - Started for Consumer Code: {consumerCode}", className, methodName, consumer.ConsumerCode);
                var response = await _consumerService.updateRegisterFlag(consumer);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: API: Error - msg: {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerModel();
            }
        }

        /// <summary>
        /// Retrieves consumer and person details based on the tenant code provided.
        /// </summary>
        /// <param name="consumerByTenantRequestDto">Contains tenant code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of consumer and person details that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        [HttpPost("get-consumers-by-tenant-code")]
        public async Task<ActionResult<ConsumersAndPersonsListResponseDto>> GetConsumerDetailsByTenantCode([FromBody] GetConsumerByTenantRequestDto consumerByTenantRequestDto)
        {
            const string methodName = nameof(GetConsumerDetailsByTenantCode);
            _consumerLogger.LogInformation("{className}.{methodName}: API - Started with TenantCode : {TenantCode}", className, methodName, consumerByTenantRequestDto.TenantCode);
            try
            {
                var response = await _consumerService.GetConsumersByTenantCode(consumerByTenantRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _consumerLogger.LogError("{className}.{methodName}: API - ERROR: {ErrorCode} and Error Message: {ErrorMessage}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: API - Error: occurred while fetching consumer and person details. Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumersAndPersonsListResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
        // <summary>
        /// Updates the consumer details asynchronously.
        /// </summary>
        /// <param name="consumerRequestDto">The data transfer object containing consumer details to be updated.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> containing the result of the update operation.
        /// If successful, returns an HTTP 200 (OK) response with the updated consumer details.
        /// If an error occurs, returns an appropriate HTTP status code and error details.
        /// </returns>
        [HttpPut("{consumerId}")]
        public async Task<ActionResult> UpdateConsumerAsync(long consumerId, [FromBody] ConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(UpdateConsumerAsync);
            _consumerLogger.LogInformation("{ClassName}.{MethodName} : Started updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);
            try
            {
                var response = await _consumerService.UpdateConsumerAsync(consumerId, consumerRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _consumerLogger.LogError("{ClassName}.{MethodName}: Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant},ERROR:{Msg}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Retrieves consumer and person details based on the List consumer codes provided.
        /// </summary>
        [HttpPost("get-consumers-by-consumer-codes")]
        public async Task<ActionResult<ConsumersAndPersonsListResponseDto>> GetConsumerDetailsByConsumerCodes([FromBody] GetConsumerByConsumerCodes getConsumerByConsumerCodes)
        {
            const string methodName = nameof(GetConsumerDetailsByConsumerCodes);
            _consumerLogger.LogInformation("{className}.{methodName}: API - Started for cosnumerCode count : {count}", className, methodName, getConsumerByConsumerCodes.ConsumerCodes.Count);
            try
            {
                var consumerresponse = await _consumerService.GetConsumersByConsumerCodes(getConsumerByConsumerCodes);
                if (consumerresponse.ErrorCode != null)
                {
                    var errorCode = consumerresponse.ErrorCode;
                    _consumerLogger.LogWarning("{className}.{methodName}: API - ERROR occured while fetching consumer details:  Error : {ErrorCode} and Error Message: {ErrorMessage}", className, methodName, consumerresponse.ErrorCode, consumerresponse.ErrorMessage);
                    return StatusCode((int)errorCode, consumerresponse);
                }
                return Ok(consumerresponse);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: API -Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumersAndPersonsListResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Retrieves consumer and person details based on the DOB and tenant code
        /// </summary>
        [HttpPost("get-consumers-by-dob")]
        public async Task<ActionResult<ConsumersAndPersonsListResponseDto>> GetConsumerByDOB([FromBody] GetConsumerByTenantCodeAndDOBRequestDto getConsumerByDOB)
        {
            const string methodName = nameof(GetConsumerByDOB);
            _consumerLogger.LogInformation("{className}.{methodName}: API - Started for tenant code {tenantCode} and dob {DOB}", className, methodName, getConsumerByDOB.TenantCode, getConsumerByDOB.DOB);
            try
            {
                var consumerListResponse = await _consumerService.GetConsumerByDOB(getConsumerByDOB);
                if (consumerListResponse.ErrorCode != null)
                {
                    var errorCode = consumerListResponse.ErrorCode;
                    _consumerLogger.LogWarning("{className}.{methodName}: API - ERROR occured while fetching consumer details:  Error : {ErrorCode} and Error Message: {ErrorMessage}", className, methodName, consumerListResponse.ErrorCode, consumerListResponse.ErrorMessage);
                    return StatusCode((int)errorCode, consumerListResponse);
                }
                return Ok(consumerListResponse);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: API -Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumersAndPersonsListResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("get-consumers-by-person-unique-identifier")]
        public async Task<ActionResult<GetConsumerByPersonUniqueIdentifierResponseDto>> GetConsumerByPersonUniqueIdentifier(string personUniqueIdentifier)
        {
            const string methodName = nameof(GetConsumerByPersonUniqueIdentifier);
            _consumerLogger.LogInformation(
                "{ClassName}.{MethodName}: API - Started with PersonUniqueIdentifier: {PersonUniqueIdentifier}",
                className, methodName, personUniqueIdentifier);

            try
            {
                var response = await _consumerService.GetConsumerByPersonUniqueIdentifier(personUniqueIdentifier);

                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError(
                        "{ClassName}.{MethodName}: Error occurred while retrieving consumer for PersonUniqueIdentifier: {PersonUniqueIdentifier}. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        className, methodName, personUniqueIdentifier, response.ErrorCode, response.ErrorMessage);

                    return StatusCode((int)response.ErrorCode, response);
                }

                _consumerLogger.LogInformation(
                    "{ClassName}.{MethodName}: Successfully retrieved consumer for PersonUniqueIdentifier: {PersonUniqueIdentifier}",
                    className, methodName, personUniqueIdentifier);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName}: API - Error occurred while retrieving consumer. StatusCode: {StatusCode}, Message: {Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new GetConsumerByPersonUniqueIdentifierResponseDto
                {
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("get-consumers-by-mem-nbr-and-region-code")]
        public async Task<ActionResult<ConsumerPersonResponseDto>> GetConsumersByMemNbrAndRegionCode([FromQuery] string memNbr, [FromQuery] string regionCode)
        {
            const string methodName = nameof(GetConsumersByMemNbrAndRegionCode);

            _consumerLogger.LogInformation(
                "{ClassName}.{MethodName}: API - Started with MemNbr: {MemNbr}, RegionCode: {RegionCode}",
                className, methodName, memNbr, regionCode);

            try
            {
                var response = await _consumerService.GetConsumersByMemberNbrAndRegionCode(memNbr, regionCode);

                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError(
                        "{ClassName}.{MethodName}: Error occurred while retrieving consumer. MemNbr: {MemNbr}, RegionCode: {RegionCode}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        className, methodName, memNbr, regionCode, response.ErrorCode, response.ErrorMessage);

                    return StatusCode((int)response.ErrorCode, response);
                }

                _consumerLogger.LogInformation(
                    "{ClassName}.{MethodName}: Successfully retrieved consumer for MemNbr: {MemNbr}, RegionCode: {RegionCode}",
                    className, methodName, memNbr, regionCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName}: API - Error occurred while retrieving consumer. StatusCode: {StatusCode}, Message: {Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerPersonResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("update-agreement-status")]
        public async Task<ActionResult<ConsumerResponseDto>> UpdateAgreementStatus([FromBody] UpdateAgreementStatusDto dto)
        {
            const string methodName = nameof(UpdateAgreementStatus);
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}: API - Started for ConsumerCode: {consumerCode}", className, methodName, dto.ConsumerCode);
                var response = await _consumerService.UpdateAgreementStatus(dto);
                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError("{className}.{methodName}: Error updating agreement status. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, dto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {Msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

        [HttpPut("update-enrollment-status")]
        public async Task<ActionResult<ConsumerResponseDto>> UpdateEnrollmentStatus([FromBody] UpdateEnrollmentStatusRequestDto requestDto)
        {
            const string methodName = nameof(UpdateEnrollmentStatus);
            try
            {
                _consumerLogger.LogInformation("{ClassName}.{MethodName}: API - Started for ConsumerCode: {ConsumerCode}", className, methodName, requestDto.ConsumerCode);

                var response = await _consumerService.UpdateEnrollmentStatus(requestDto);
                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError("{ClassName}.{MethodName}: Error updating enrollment status. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{ClassName}.{MethodName}: ERROR - msg: {Msg}, Error Code:{ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates the subscription status of a consumer.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [HttpPost("consumer-subscription-status")]
        public async Task<ActionResult<BaseResponseDto>> UpdateConsumerSubscriptionStatus([FromBody] ConsumerSubscriptionStatusRequestDto requestDto)
        {
            const string methodName = nameof(UpdateConsumerSubscriptionStatus);
            try
            {
                _consumerLogger.LogInformation("{className}.{methodName}:ConsumerAttributes API - Started TenantCode: {tenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _consumerService.UpdateConsumerSubscriptionStatus(requestDto);
                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError("{ClassName}.{MethodName}: Error occurred while updating consumer subscription status. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}