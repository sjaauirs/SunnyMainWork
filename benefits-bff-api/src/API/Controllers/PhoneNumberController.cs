using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/phone-number")]
    [ApiController]
    [Authorize]
    public class PhoneNumberController : ControllerBase
    {
        private readonly ILogger<PhoneNumberController> _logger;
        private readonly IPhoneNumberService _phoneNumberService;
        private const string className = nameof(PhoneNumberController);

        public PhoneNumberController(ILogger<PhoneNumberController> logger, IPhoneNumberService phoneNumberService)
        {
            _logger = logger;
            _phoneNumberService = phoneNumberService;
        }

        [HttpGet("{personId:long}/get-all-phone-numbers")]
        public async Task<ActionResult> GetAllPhoneNumbers([FromRoute] long personId)
        {
            const string methodName = nameof(GetAllPhoneNumbers);
            _logger.LogInformation("{ClassName}.{MethodName} : Started getting all phone numbers for person with ID:{PersonId}",
                        className, methodName, personId);
            try
            {
                var response = await _phoneNumberService.GetAllPhoneNumbers(personId);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while getting phone numbers for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, personId, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully retrieved phone numbers for person with ID:{PersonId}",
                   className, methodName, personId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleExceptionAsync<GetAllPhoneNumbersResponseDto>(ex, methodName, $"person with ID:{personId}");
            }
        }

        [HttpPost("post-phone-number")]
        public async Task<ActionResult> CreatePhoneNumber([FromBody] CreatePhoneNumberRequestDto request)
        {
            const string methodName = nameof(CreatePhoneNumber);
            _logger.LogInformation("{ClassName}.{MethodName} : Started creating phone number for person with ID:{PersonId}",
                        className, methodName, request.PersonId);
            try
            {
                var response = await _phoneNumberService.CreatePhoneNumber(request);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating phone number for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PersonId, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully created phone number for person with ID:{PersonId}",
                   className, methodName, request.PersonId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleExceptionAsync<PhoneNumberResponseDto>(ex, methodName, $"person with ID:{request.PersonId}");
            }
        }

        [HttpPut("update-phone-number")]
        public async Task<ActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberRequestDto request, [FromQuery] bool markAsPrimary)
        {
            const string methodName = nameof(UpdatePhoneNumber);
            _logger.LogInformation("{ClassName}.{MethodName} : Started updating phone number with ID:{PhoneNumberId}",
                        className, methodName, request.PhoneNumberId);
            try
            {
                var response = await _phoneNumberService.UpdatePhoneNumber(request, markAsPrimary);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating phone number with ID:{PhoneNumberId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PhoneNumberId, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully updated phone number with ID:{PhoneNumberId}",
                   className, methodName, request.PhoneNumberId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleExceptionAsync<PhoneNumberResponseDto>(ex, methodName, $"phone number with ID:{request.PhoneNumberId}");
            }
        }

        private ObjectResult HandleExceptionAsync<T>(Exception ex, string methodName, string contextInfo) where T : class, new()
        {
            _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while processing {ContextInfo}, ERROR:{Msg}",
                className, methodName, contextInfo, ex.Message);

            var errorDto = new T();
            var errorCodeProperty = typeof(T).GetProperty("ErrorCode");

            if (errorCodeProperty != null && errorCodeProperty.CanWrite)
            {
                errorCodeProperty.SetValue(errorDto, StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, errorDto);
        }
    }
}
