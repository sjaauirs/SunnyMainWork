using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    [Authorize]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _loginLogger;
        private readonly ILoginService _loginService;
        private readonly IAuth0Helper _auth0Helper;
        private const string className = nameof(LoginController);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginLogger"></param>
        /// <param name="loginService"></param>
        /// <param name="auth0Helper"></param>
        public LoginController(ILogger<LoginController> loginLogger, ILoginService loginService,
           IAuth0Helper auth0Helper)
        {
            _loginLogger = loginLogger;
            _loginService = loginService;
            _auth0Helper = auth0Helper;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("get-consumers-by-email")]
        public async Task<ActionResult<GetConsumerByEmailResponseDto>> GetConsumerByEmail([FromQuery] string? email)
        {
            const string methodName = nameof(GetConsumerByEmail);
            _loginLogger.LogInformation("{ClassName}.{MethodName} - Started processing", className, methodName);

            try
            {
                var response = await _loginService.GetConsumerByPersonUniqueIdentifier(email);

                if (response?.ErrorCode != null )
                {
                    _loginLogger.LogError(
                        "{ClassName}.{MethodName} - Failed to retrieve consumer. Email: {Email}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        className, methodName, email, response?.ErrorCode, response?.ErrorMessage);

                    return StatusCode((int)(response?.ErrorCode ?? StatusCodes.Status400BadRequest), response);

                }

                _loginLogger.LogInformation(
                    "{ClassName}.{MethodName} - Successfully retrieved consumer. Email: {Email}",
                    className, methodName, email);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _loginLogger.LogError(ex,
                    "{ClassName}.{MethodName} - Unexpected error occurred. Email: {Email}, ErrorCode: {ErrorCode}, Message: {Message}",
                    className, methodName, email, StatusCodes.Status500InternalServerError, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new GetConsumerByEmailResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An internal server error occurred while processing the request."
                });
            }
            finally
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Finished processing", className, methodName);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="patchUserRequestDto"></param>
        /// <returns></returns>
        [HttpPatch("patch-user")]
        public async Task<ActionResult<UpdateResponseDto>> PatchUser([FromBody] PatchUserRequestDto patchUserRequestDto)
        {
            const string methodName = nameof(PatchUser);
            try
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Started processing Patch User with UserId : {Auth0UserId}", className, methodName, patchUserRequestDto.UserId);
                var response = await _auth0Helper.PatchUserOuter(patchUserRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response.email),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response.email),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _loginLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while updating user with UserId : {Auth0UserId}, " +
                    "ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, patchUserRequestDto.UserId, StatusCodes.Status500InternalServerError, ex.Message);
                return new UpdateResponseDto();
            }
            finally
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Ended with UserId : {Auth0UserId}", className, methodName, patchUserRequestDto.UserId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verifyMemberDto"></param>
        /// <returns></returns>
        [HttpPost("verify-member-info")]
        public async Task<ActionResult<VerifyMemberResponseDto>> VerifyMember([FromBody] VerifyMemberDto verifyMemberDto)
        {
            const string methodName = nameof(VerifyMember);
            try
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Started processing Verify Member", className, methodName);
                var response = await _loginService.VerifyMember(verifyMemberDto);
                if (response?.ErrorCode != null)
                {
                    _loginLogger.LogError(
                        "{ClassName}.{MethodName} - Failed to verify consumer. Email: {Email}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        className, methodName, verifyMemberDto.Email, response?.ErrorCode, response?.ErrorMessage);

                    return StatusCode((int)(response?.ErrorCode ?? StatusCodes.Status400BadRequest), response);

                }
               

                return Ok(response);
            }
            catch (Exception ex)
            {
                _loginLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while verifying member,ErrorCode:{ErrorCode},ERROR:{ErrorMessage}", 
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return BadRequest(ex);
            }
            finally
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Ended", className, methodName);
            }
        }

        /// <summary>
        /// Post Verification Email
        /// </summary>
        /// <param name="patchUserRequestDto"></param>
        /// <returns></returns>
        [HttpPost("post-verification-email")]
        public async Task<ActionResult<UpdateResponseDto>> PostVerificationEmail([FromBody] VerificationEmailRequestDto emailRequestDto)
        {
            const string methodName = nameof(PostVerificationEmail);
            try
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Started processing Post Verification Email with UserId:{UserId}", 
                    className, methodName,emailRequestDto.UserId);

                var response = await _auth0Helper.PostVerificationEmail(emailRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response.email),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response.email),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _loginLogger.LogError(ex, "{ClassName}.{MethodName} - Failed processing.ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}", 
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new UpdateResponseDto();
            }
            finally
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Ended processing.", className, methodName);
            }
        }


        /// <summary>
        /// Get User by UserId
        /// </summary>
        /// <param name="patchUserRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-user-by-id")]
        public async Task<ActionResult<UserGetResponseDto>> GetUserById(GetUserRequestDto userRequestDto)
        {
            const string methodName = nameof(GetUserById);
            try
            {
                _loginLogger.LogInformation("{ClassName}.{MethodName} - Started processing GetUserById with UserId:{UserId}",
                    className, methodName, userRequestDto.UserId);

                var response = await _auth0Helper.GetUserById(userRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _loginLogger.LogError(ex, "{ClassName} {MethodName} - Failed processing Get UserById with UserId:{UserId}, ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}",
                    className, methodName, userRequestDto.UserId, StatusCodes.Status500InternalServerError, ex.Message);
                return new UserGetResponseDto();
            }
            finally
            {
                _loginLogger.LogInformation("{ClassName} {MethodName} Ended processing.", className, methodName);
            }
        }
        
        [AllowAnonymous]
        [HttpPost("internal-login")]
        public async Task<ActionResult<LoginResponseDto>> InternalLogin([FromBody] LoginRequestDto loginRequestDto)
        {
            const string methodName = nameof(InternalLogin);
            try
            {
                _loginLogger.LogInformation("{className}.{methodName}: API - Started With Request : {request}", className, methodName, loginRequestDto.ConsumerCode);
                var response = await _loginService.InternalLogin(loginRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _loginLogger.LogError("{className}.{methodName}: API - Error occurred while Log in. Error Response: {response}, Error Code:{errorCode}", className, methodName, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)errorCode, response);
                }
                _loginLogger.LogInformation("{className}.{methodName}: API - Successfully Logged in", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _loginLogger.LogError(ex, "{className}.{methodName}: API - exception while LogIn. ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status400BadRequest);
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponseDto());
            }
        }
    }
}

