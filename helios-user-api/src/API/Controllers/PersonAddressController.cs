using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/persons-address")]
    [ApiController]
    public class PersonAddressController : ControllerBase
    {
        private readonly IPersonAddressService _personAddressService;
        private readonly ILogger<PersonAddressController> _logger;
        private const string className = nameof(PersonAddressController);

        public PersonAddressController(IPersonAddressService personAddressService, ILogger<PersonAddressController> logger)
        {
            _personAddressService = personAddressService;
            _logger = logger;
        }

        [HttpGet("{personId:long}/get-all-addresses")]
        public async Task<IActionResult> GetAllPersonAddresses([FromRoute]long personId)
        {
            const string methodName = nameof(GetAllPersonAddresses);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching all addresses for personId {personId}.", className, methodName, personId);
                var response = await _personAddressService.GetAllPersonAddresses(personId);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error occurred while fetching addresses. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetched all addresses for personId {personId}.", className, methodName, personId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{personId:long}")]
        public async Task<IActionResult> GetPersonAddress(long personId, [FromQuery] long? addressTypeId, [FromQuery] bool? isPrimary)
        {
            const string methodName = nameof(GetPersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started fetching addresses for personId {personId}.", className, methodName, personId);
                var response = await _personAddressService.GetPersonAddress(personId, addressTypeId, isPrimary);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Error while filtering addresses. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully fetching addresses for personId {personId}.", className, methodName, personId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("post-address")]
        public async Task<IActionResult> CreatePersonAddress([FromBody] CreatePersonAddressRequestDto request)
        {
            const string methodName = nameof(CreatePersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started creating address.", className, methodName);
                var response = await _personAddressService.CreatePersonAddress(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to create address. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully created address.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("update-address")]
        public async Task<IActionResult> UpdatePersonAddress([FromBody] UpdatePersonAddressRequestDto request)
        {
            const string methodName = nameof(UpdatePersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started updating address.", className, methodName);
                var response = await _personAddressService.UpdatePersonAddress(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to update address. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully updated address.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("delete-address")]
        public async Task<IActionResult> DeletePersonAddress([FromBody] DeletePersonAddressRequestDto request)
        {
            const string methodName = nameof(DeletePersonAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started deleting address.", className, methodName);
                var response = await _personAddressService.DeletePersonAddress(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to delete address. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully deleted address.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName} - Exception: {Message}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("set-primary")]
        public async Task<IActionResult> SetPrimaryAddress([FromBody] UpdatePrimaryPersonAddressRequestDto request)
        {
            const string methodName = nameof(SetPrimaryAddress);
            try
            {
                _logger.LogInformation("{className}.{methodName} - Started setting primary address.", className, methodName);
                var response = await _personAddressService.SetPrimaryAddress(request);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName} - Failed to set primary address. Response: {response}", className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName} - Successfully set primary address.", className, methodName);
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
