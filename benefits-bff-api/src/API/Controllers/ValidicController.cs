using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/validic")]
    [ApiController]
    [Authorize]
    public class ValidicController : ControllerBase
    {
        private readonly ILogger<ValidicController> _validicLogger;
        private readonly IValidicService _validicService;
        private const string className = nameof(ValidicController);
        public ValidicController(ILogger<ValidicController> validicLogger, IValidicService validicService)
        {
            _validicLogger = validicLogger;
            _validicService = validicService;
        }

        [HttpPost("create-user")]
        public async Task<ActionResult<CreateValidicUserResponseDto>> CreateValidicUser([FromBody] CreateValidicUserRequestDto request)
        {
            const string methodName = nameof(CreateValidicUser);
            try
            {
                _validicLogger.LogInformation("{ClassName}.{MethodName} - Started creation of validic user for consumer : {ConsumerCode}", className, methodName, request.ConsumerCode);
                var response = await _validicService.CreateValidicUser(request);
                if (response.ErrorCode != null)
                {
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _validicLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while creation of validic user for consumer : {ConsumerCode}",
                    className, methodName, request.ConsumerCode);
                return new CreateValidicUserResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                };
            }
            finally
            {
                _validicLogger.LogInformation("{ClassName}.{MethodName} - Ended creation of validic user for consumer : {ConsumerCode}", className, methodName, request.ConsumerCode);
            }
        }
    }
}
