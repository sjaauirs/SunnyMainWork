using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/person")]
    [ApiController]
    [Authorize]
    public class PersonController : ControllerBase
    {
        private readonly ILogger<PersonController> _logger;
        private readonly IPersonService _personService;
        private const string className = nameof(PersonController);
        public PersonController(ILogger<PersonController> logger, IPersonService personService)
        {
            _logger = logger;
            _personService = personService;
        }

        [HttpPut("update-person")]
        public async Task<ActionResult<PersonResponseDto>> UpdatePersonData(UpdatePersonRequestDto updatePersonRequestDto)
        {
            const string methodName = nameof(UpdatePersonData);
            _logger.LogInformation("{className}.{methodName}: API - Started updating with Consumer Code : {personId}", className, methodName, updatePersonRequestDto.ConsumerCode);
            try
            {
                var response = await _personService.UpdatePersonData(updatePersonRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{className}.{methodName}: API - ERROR: {ErrorCode} and Error Message: {ErrorMessage}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - Error: occurred while updating person details. Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new PersonResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
            finally
            {
                _logger.LogInformation("{className}.{methodName} - UpdatePersonData API - Ended with Consumer Code : {personId}", className, methodName, updatePersonRequestDto.ConsumerCode);
            }
        }
    }
}
