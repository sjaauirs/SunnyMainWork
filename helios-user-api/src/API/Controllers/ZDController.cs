using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("/api/v1/zd/")]
    [ApiController]
    public class ZDController : ControllerBase
    {
        private readonly ILogger<ZDController> _zdLogger;
        private readonly IZDService _zdService;

        public ZDController(ILogger<ZDController> zdLogger, IZDService zdService)
        {
            _zdLogger = zdLogger;
            _zdService = zdService;
        }
        const string className = nameof(ZDController);
        /// <summary>
        /// Creates a Zendesk token based on the provided request Consumer code.
        /// </summary>
        /// <param name="zdTokenRequestDto">The request data containing the necessary information to create a Zendesk token.</param>
        /// <returns>A response containing the Zendesk token or an error message.</returns>

        [HttpPost("create-zd-token")]
        public async Task<ActionResult<ZdTokenResponseDto>> CreateZdToken([FromBody] ZdTokenRequestDto zdTokenRequestDto)
        {
            const string methodName = nameof(CreateZdToken);
            try
            {
                _zdLogger.LogInformation("{className}.{methodName}: API - Started with ExternalId :{ExternalId}", className, methodName, zdTokenRequestDto.ConsumerCode);
                var response = await _zdService.CreateZdToken(zdTokenRequestDto);
                return response.ErrorCode switch
                {
                    StatusCodes.Status404NotFound => NotFound(response),
                    StatusCodes.Status500InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _zdLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new ZdTokenResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
    }
}
