using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("/api/v1/server")]
    [ApiController]
    public class ServerLoginController : ControllerBase
    {
        private readonly ILogger<ServerLoginController> _logger;
        private readonly IServerLoginService _serverLoginService;

        public ServerLoginController(ILogger<ServerLoginController> logger, IServerLoginService serverLoginService)
        {
            _logger = logger;
            _serverLoginService = serverLoginService;
        }
        const string className = nameof(ServerLoginController);
        /// <summary>
        /// Authenticates a server login request and generates an API token.
        /// </summary>
        /// <param name="serverLoginRequestDto">The login request data containing the tenant code and Partner Code.</param>
        /// <returns>A response containing the API token or an error message.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ServerLoginResponseDto>> ServerLogin([FromBody] ServerLoginRequestDto serverLoginRequestDto)
        {
            const string methodName = nameof(ServerLogin);
            try
            {
                _logger.LogInformation("{className}.{methodName}: API - Started with Tenant code :{TenantCode}", className, methodName, serverLoginRequestDto.TenantCode);
                var response = await _serverLoginService.CreateApiToken(serverLoginRequestDto);

                return response.ErrorCode switch
                {
                    400 => BadRequest(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServerLoginResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                });
            }
        }
    }
}
