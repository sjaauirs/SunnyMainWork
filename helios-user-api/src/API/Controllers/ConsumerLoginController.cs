using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;


namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("/api/v1/consumer/")]
    [ApiController]
    public class ConsumerLoginController : ControllerBase
    {
        private readonly ILogger<ConsumerLoginController> _consumerLoginLogger;
        private readonly IConsumerLoginService _consumerLoginService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerLoginLogger"></param>
        /// <param name="consumerLoginService"></param>
        public ConsumerLoginController(ILogger<ConsumerLoginController> consumerLoginLogger, IConsumerLoginService consumerLoginService)
        {
            _consumerLoginLogger = consumerLoginLogger;
            _consumerLoginService = consumerLoginService;
        }
        const string className = nameof(ConsumerLoginController);
        /// <summary>
        /// Handles consumer login and generates an authentication token.
        /// </summary>
        /// <param name="consumerLoginRequestDto">The login request containing consumer credentials.</param>
        /// <returns>A response containing the authentication token or an error message.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ConsumerLoginResponseDto>> ConsumerLogin([FromBody] ConsumerLoginRequestDto consumerLoginRequestDto)
        {
            const string methodName = nameof(ConsumerLogin);
            try
            {
                _consumerLoginLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode :{ConsumerCode}", className, methodName, consumerLoginRequestDto.ConsumerCode);
                var response = await _consumerLoginService.CreateToken(consumerLoginRequestDto);

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
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new ConsumerLoginResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// Refreshes the authentication token based on the provided refresh token request.
        /// </summary>
        /// <param name="refreshTokenRequestDto">The request data containing the refresh token.</param>
        /// <returns>A response containing the new authentication token or an error message.</returns>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
        {
            const string methodName = nameof(RefreshToken);
            try
            {
                _consumerLoginLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode :{ConsumerCode}", className, methodName, refreshTokenRequestDto.ConsumerCode);
                var response = await _consumerLoginService.RefreshToken(refreshTokenRequestDto);

                return response.ErrorCode switch
                {
                    400 => BadRequest(response),
                    404 => NotFound(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new RefreshTokenResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// Validates the provided authentication token.
        /// </summary>
        /// <param name="validateTokenRequestDto">The request data containing the access token to be validated.</param>
        /// <returns>A response indicating whether the token is valid or an error message.</returns>
        [HttpPost("validate-token")]
        public async Task<ActionResult<ValidateTokenResponseDto>> ValidateToken([FromBody] ValidateTokenRequestDto validateTokenRequestDto)
        {
            const string methodName = nameof(ValidateToken);
            try
            {
                if (validateTokenRequestDto == null || string.IsNullOrEmpty(validateTokenRequestDto.AccessToken))
                    return BadRequest("token cannot be null/empty");
                var response = await _consumerLoginService.ValidateToken(validateTokenRequestDto);

                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new ValidateTokenResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
        [HttpGet("get-consumer-login-detail/{consumerCode}")]
        public async Task<ActionResult<ConsumerLoginDateResponseDto>> GetConsumerLoginDetail(string consumerCode)
        {
            const string methodName = nameof(GetConsumerLoginDetail);
            try
            {
                if (consumerCode == null || string.IsNullOrEmpty(consumerCode))
                    return BadRequest("consumerCode cannot be null/empty");
                var response = await _consumerLoginService.GetConsumerLoginDetail(consumerCode);

                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new ConsumerLoginDateResponseDto() { ErrorCode=StatusCodes.Status500InternalServerError,ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };

            }
        }
      
        [HttpPost("get-consumer-engagement-detail")]
        public async Task<ActionResult<GetConsumerEngagementDetailResponseDto>> GetConsumerEngagementDetail(GetConsumerEngagementDetailRequestDto consumerEngagementDetailRequestDto)
        {
            const string methodName = nameof(GetConsumerEngagementDetail);
            try
            {
                if (consumerEngagementDetailRequestDto.ConsumerCode == null || string.IsNullOrEmpty(consumerEngagementDetailRequestDto.ConsumerCode)
                    || consumerEngagementDetailRequestDto.EngagementFrom>=consumerEngagementDetailRequestDto.EngagementUntil)
                    return BadRequest("consumerCode cannot be null/empty");
                var response = await _consumerLoginService.GetConsumerEngagementDetail(consumerEngagementDetailRequestDto);

                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new GetConsumerEngagementDetailResponseDto() { ErrorCode=StatusCodes.Status500InternalServerError,ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };

            }
        }
    }
}