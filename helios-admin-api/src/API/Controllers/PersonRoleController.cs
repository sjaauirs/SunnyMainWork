using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class PersonRoleController : ControllerBase
    {
        private readonly ILogger<PersonRoleController> _logger;
        private readonly IPersonRoleService _personRoleService;
        public const string className = nameof(PersonRoleController);

        public PersonRoleController(ILogger<PersonRoleController> logger, IPersonRoleService personRoleService)
        {
            _logger = logger;
            _personRoleService = personRoleService;
        }
        /// <summary>
        /// Get's all the person roles
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [HttpPost("get-person-roles")]
        public async Task<ActionResult<GetPersonRolesResponseDto>> GetPersonRoles(GetPersonRolesRequestDto requestDto)
        {
            const string methodName = nameof(GetPersonRoles);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started fetching Person Roles", className, methodName);
                var response = await _personRoleService.GetPersonRoles(requestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: API Error Occurred while fetching personRoles, ErrorCode:{ErrorCode}", className, methodName, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "{className}.{methodName}: API - Error: occurred while fetching person Roles. Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetPersonRolesResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Fetch the access control list for the specified consumer code.
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        [HttpGet("access-control-list")]
        public async Task<ActionResult<AccessControlListResponseDTO>> GetAccessControlList()
        {
            const string methodName = nameof(GetAccessControlList);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started fetching Person Roles", className, methodName);
                var response = await _personRoleService.GetAccessControlList(Request.Headers.Authorization);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: API Error Occurred while fetching personRoles, ErrorCode:{ErrorCode}", className, methodName, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "{className}.{methodName}: API - Error: occurred while fetching person RoleCode. Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new AccessControlListResponseDTO()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
