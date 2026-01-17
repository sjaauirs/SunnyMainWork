using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class ScriptController : ControllerBase
    {
        private readonly ILogger<ScriptController> _logger;
        private readonly IScriptService _scriptService;
        private const string className = nameof(ScriptController);
        public ScriptController(ILogger<ScriptController> logger, IScriptService scriptService)
        {
            _logger = logger;
            _scriptService = scriptService;
        }
        [HttpGet("scripts")]
        public async Task<ActionResult<ScriptResponseDto>> GetScript()
        {
            const string methodName = nameof(GetScript);
            try
            {
                _logger.LogInformation("{className}.{methodName}: API - Started to get scripts ", className, methodName);
                var response = await _scriptService.GetScript();
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new ScriptResponseDto()
                {
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message,
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
