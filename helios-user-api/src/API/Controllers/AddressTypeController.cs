using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/address-type")]
    [ApiController]
    public class AddressTypeController : ControllerBase
    {
        private readonly IAddressTypeService _addressTypeService;
        private readonly ILogger<AddressTypeController> _logger;
        private const string className = nameof(AddressTypeController);
        public AddressTypeController(IAddressTypeService addressTypeService, ILogger<AddressTypeController> logger)
        {
            _addressTypeService = addressTypeService;
            _logger = logger;
        }

        [HttpGet("get-all-address-types")]
        public async Task<IActionResult> GetAllAddressTypes()
        {
            const string methodName = nameof(GetAllAddressTypes);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching all address types.", className, methodName);
                var response = await _addressTypeService.GetAllAddressTypes();
                if(response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error occurred while fetching address types. Response: {response}, ErrorCode: {errorCode}", className, methodName, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched all address types.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting all address types. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAllAddressTypesResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAddressTypeById(long addressTypeId)
        {
            const string methodName = nameof(GetAddressTypeById);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching address type for addressTypeId {addressTypeId}.", className, methodName, addressTypeId);
                var response = await _addressTypeService.GetAddressTypeById(addressTypeId);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error occurred while fetching address type for {addressTypeId}. Response: {response}, ErrorCode: {errorCode}", className, methodName, addressTypeId, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched address type for addressTypeId {addressTypeId}.", className, methodName, addressTypeId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Error occurred while getting address type for addressTypeId {addressTypeName}. ErrorMessage: {ErrorMessage}", className, methodName, addressTypeId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAddressTypeResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
