using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    public class AdminLoginController : ControllerBase
    {
        private readonly ILogger<AdminLoginController> _logger;
        private readonly IAdminLoginService _adminLoginService;

        const string ClassName = nameof(AdminLoginController);

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminLoginController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging.</param>
        /// <param name="adminLoginService">The admin login service instance.</param>
        public AdminLoginController(ILogger<AdminLoginController> logger, IAdminLoginService adminLoginService)
        {
            _logger = logger;
            _adminLoginService = adminLoginService;
        }

        /// <summary>
        /// Handles the generation of an authentication token for an admin user.
        /// </summary>
        /// <param name="adminLoginRequestDto">The unique code identifying the consumer.</param>
        /// <returns>A response containing the authentication token or an error message.</returns>
        [HttpPost("admin-login")]
        public async Task<ActionResult<AdminLoginResponseDto>> GenerateAdminTokenAsync(AdminLoginRequestDto adminLoginRequestDto)
        {
            const string MethodName = nameof(GenerateAdminTokenAsync);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request received for ConsumerCode: {ConsumerCode}", ClassName, MethodName, adminLoginRequestDto.ConsumerCode);

                var response = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestDto);

                // Handle response based on error codes
                return response.ErrorCode switch
                {
                    401 => Unauthorized(response),
                    403 => StatusCode(StatusCodes.Status403Forbidden, response),
                    404 => NotFound(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while processing the request. ConsumerCode: {ConsumerCode}, Error: {ErrorMessage}", ClassName, MethodName, adminLoginRequestDto.ConsumerCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AdminLoginResponseDto
                {
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                });
            }
        }
    }
}