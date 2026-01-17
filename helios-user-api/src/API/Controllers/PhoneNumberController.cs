using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/phone-number")]
    [ApiController]
    public class PhoneNumberController : ControllerBase
    {
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly ILogger<PhoneNumberController> _logger;
        private const string className = nameof(PhoneNumberController);

        public PhoneNumberController(IPhoneNumberService phoneNumberService, ILogger<PhoneNumberController> logger)
        {
            _phoneNumberService = phoneNumberService;
            _logger = logger;
        }

        [HttpGet("{personId:long}/get-all-numbers")]
        public async Task<IActionResult> GetAllPhoneNumbers([FromRoute] long personId)
        {
            const string methodName = nameof(GetAllPhoneNumbers);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching all phone numbers for personId {personId}.", className, methodName, personId);
                var response = await _phoneNumberService.GetAllPhoneNumbers(personId);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error occurred while fetching phone numbers. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched all phone numbers for personId {personId}.", className, methodName, personId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{personId:long}")]
        public async Task<IActionResult> GetPhoneNumber(long personId, [FromQuery] long? phoneTypeId, [FromQuery] bool? isPrimary)
        {
            const string methodName = nameof(GetPhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching phone numbers for personId {personId}.", className, methodName, personId);

                var response = await _phoneNumberService.GetPhoneNumber(personId, phoneTypeId, isPrimary);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error while filtering phone numbers. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{className}.{methodName} - Successfully fetched phone numbers for personId {personId}.", className, methodName, personId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("post-number")]
        public async Task<IActionResult> CreatePhoneNumber([FromBody] CreatePhoneNumberRequestDto request)
        {
            const string methodName = nameof(CreatePhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started creating phone number.", className, methodName);
                var response = await _phoneNumberService.CreatePhoneNumber(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to create phone number. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully created phone number.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("update-number")]
        public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberRequestDto request)
        {
            const string methodName = nameof(UpdatePhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started updating phone number.", className, methodName);
                var response = await _phoneNumberService.UpdatePhoneNumber(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to update phone number. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully updated phone number.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("delete-number")]
        public async Task<IActionResult> DeletePhoneNumber([FromBody] DeletePhoneNumberRequestDto request)
        {
            const string methodName = nameof(DeletePhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started deleting phone number.", className, methodName);
                var response = await _phoneNumberService.DeletePhoneNumber(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to delete phone number. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully deleted phone number.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("set-primary")]
        public async Task<IActionResult> SetPrimaryPhoneNumber([FromBody] UpdatePrimaryPhoneNumberRequestDto request)
        {
            const string methodName = nameof(SetPrimaryPhoneNumber);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started setting primary phone number.", className, methodName);
                var response = await _phoneNumberService.SetPrimaryPhoneNumber(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to set primary phone number. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully set primary phone number.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

}
