using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/person/")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly ILogger<PersonController> _personLogger;
        private readonly IPersonService _personService;

        /// <summary>
        /// Get Consumer Data Constructor
        /// </summary>
        /// <param name="personLogger"></param>
        /// <param name="personService"></param>
        public PersonController(ILogger<PersonController> personLogger, IPersonService personService)
        {
            _personLogger = personLogger;
            _personService = personService;
        }
        const string className = nameof(PersonController);
        /// <summary>
        /// Retrieves the person details based on the provided person ID.
        /// </summary>
        /// <param name="personId">The ID of the person to retrieve.</param>
        /// <returns>A response containing the person details or an error message.</returns>
        [HttpGet("{personId}")]
        public async Task<ActionResult<PersonDto>> GetPerson(long personId)
        {
            var response = new PersonDto();
            const string methodName = nameof(GetPerson);
            try
            {
                if (personId > 0)
                {
                    _personLogger.LogInformation("{className}.{methodName}: API - Started with PersonId : {personId}", className, methodName, personId);
                    response = await _personService.GetPersonData(personId);
                }

                return response.PersonId > 0 ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _personLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new PersonDto();
            }
        }

        /// <summary>
        /// Retrieves both person and consumer details based on the provided consumer code.
        /// </summary>
        /// <param name="consumerRequestDto">The request data containing the consumer code.</param>
        /// <returns>A response containing both person and consumer details or an error message.</returns>
        [HttpPost("get-details-by-consumer-code")]
        public async Task<ActionResult<GetPersonAndConsumerResponseDto>> GetPersonAndConsumerDetails([FromBody] GetConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(GetPersonAndConsumerDetails);
            _personLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}", className, methodName, consumerRequestDto.ConsumerCode);
            try
            {
                var response = await _personService.GetOverAllConsumerDetails(consumerRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _personLogger.LogError("{className}.{methodName}: API - ERROR: {ErrorCode} and Error Message: {ErrorMessage}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _personLogger.LogError(ex, "{className}.{methodName}: API - Error: occurred while fetching consumer and person details. Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetPersonAndConsumerResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPut("update-person")]
        public async Task<ActionResult<PersonResponseDto>> UpdatePersonData(UpdatePersonRequestDto updatePersonRequestDto)
        {
            const string methodName = nameof(UpdatePersonData);
            _personLogger.LogInformation("{className}.{methodName}: API - Started updating with Consumer Code : {personId}", className, methodName, updatePersonRequestDto.ConsumerCode);
            try
            {
                var response = await _personService.UpdatePersonData(updatePersonRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _personLogger.LogError("{className}.{methodName}: API - ERROR: {ErrorCode} and Error Message: {ErrorMessage}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _personLogger.LogError(ex, "{className}.{methodName}: API - Error: occurred while updating person details. Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new PersonResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}