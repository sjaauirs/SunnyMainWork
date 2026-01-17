using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/phone-type")]
    [ApiController]
    public class PhoneTypeController : ControllerBase
    {
        private readonly IPhoneTypeService _phoneTypeService;
        private readonly ILogger<PhoneTypeController> _logger;
        private const string className = nameof(PhoneTypeController);

        public PhoneTypeController(IPhoneTypeService phoneTypeService, ILogger<PhoneTypeController> logger)
        {
            _phoneTypeService = phoneTypeService;
            _logger = logger;
        }

        [HttpGet("get-all-phone-types")]
        public async Task<IActionResult> GetAllPhoneTypes()
        {
            const string methodName = nameof(GetAllPhoneTypes);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching all phone types.", className, methodName);
                var response = await _phoneTypeService.GetAllPhoneTypes();
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error occurred while fetching phone types. Response: {response}, ErrorCode: {errorCode}", className, methodName, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched all phone types.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting all phone types. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAllPhoneTypesResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPhoneTypeById(long phoneTypeId)
        {
            const string methodName = nameof(GetPhoneTypeById);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching phone type for phoneTypeId {phoneTypeId}.", className, methodName, phoneTypeId);
                var response = await _phoneTypeService.GetPhoneTypeById(phoneTypeId);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error occurred while fetching phone type for {phoneTypeId}. Response: {response}, ErrorCode: {errorCode}", className, methodName, phoneTypeId, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched phone type for phoneTypeId {phoneTypeId}.", className, methodName, phoneTypeId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting phone type for phoneTypeId {phoneTypeId}. ErrorMessage: {ErrorMessage}", className, methodName, phoneTypeId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetPhoneTypeResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }

}
