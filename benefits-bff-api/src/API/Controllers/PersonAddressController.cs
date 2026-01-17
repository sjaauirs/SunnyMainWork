using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/person-address")]
    [ApiController]
    [Authorize]
    public class PersonAddressController : ControllerBase
    {
        private readonly ILogger<PersonAddressController> _logger;
        private readonly IPersonAddressService _personAddressService;
        private const string className = nameof(PersonAddressController);
        public PersonAddressController(ILogger<PersonAddressController> logger, IPersonAddressService personAddressService)
        {
            _logger = logger;
            _personAddressService = personAddressService;
        }

        [HttpGet("{personId:long}/get-all-addresses")]
        public async Task<ActionResult> GetAllPersonAddresses([FromRoute] long personId)
        {
            const string methodName = nameof(GetAllPersonAddresses);
            _logger.LogInformation("{ClassName}.{MethodName} : Started getting all addresses for person with ID:{PersonId}",
                        className, methodName, personId);
            try
            {
                var response = await _personAddressService.GetAllPersonAddresses(personId);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while getting addresses for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, personId, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully retrieved addresses for person with ID:{PersonId}",
                   className, methodName, personId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while getting addresses for person with ID:{PersonId},ERROR:{Msg}",
                        className, methodName, personId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAllPersonAddressesResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("post-address")]
        public async Task<ActionResult> CreatePersonAddress([FromBody] CreatePersonAddressRequestDto request)
        {
            const string methodName = nameof(CreatePersonAddress);
            _logger.LogInformation("{ClassName}.{MethodName} : Started creating address for person with ID:{PersonId}",
                        className, methodName, request.PersonId);
            try
            {
                var response = await _personAddressService.CreatePersonAddress(request);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating address for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PersonId, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully created address for person with ID:{PersonId}",
                   className, methodName, request.PersonId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while creating address for person with ID:{PersonId},ERROR:{Msg}",
                        className, methodName, request.PersonId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new PersonAddressResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPut("update-address")]
        public async Task<ActionResult> UpdatePersonAddress([FromBody] UpdatePersonAddressRequestDto request, [FromQuery]bool markAsPrimary)
        {
            const string methodName = nameof(CreatePersonAddress);
            _logger.LogInformation("{ClassName}.{MethodName} : Started updating address for person address ID:{PersonAddressId}",
                        className, methodName, request.PersonAddressId);
            try
            {
                var response = await _personAddressService.UpdatePersonAddress(request, markAsPrimary);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating address for person address ID:{PersonAddressId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, request.PersonAddressId, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully updated address for person address ID:{PersonAddressId}",
                   className, methodName, request.PersonAddressId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while updating address for person address ID:{PersonAddressId},ERROR:{Msg}",
                        className, methodName, request.PersonAddressId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new PersonAddressResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
