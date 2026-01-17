using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1")]
    [ApiController]
    public class AdventureController : ControllerBase
    {
        private readonly ILogger<AdventureController> _logger;
        private readonly IAdventureService _adventureService;
        private const string className = nameof(AdventureController);

        public AdventureController(ILogger<AdventureController> logger, IAdventureService adventureService)
        {
            _logger = logger;
            _adventureService = adventureService;
        }
        /// <summary>
        /// Retrieves all adventures based on the given request parameters.
        /// </summary>
        /// <param name="getAdventureRequestDto">The request DTO containing the criteria for fetching adventures.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="GetAdventureResponseDto"/> with the list of adventures.
        /// Returns a status code based on the error condition if an issue occurs.
        /// </returns>
        [HttpPost("adventure/adventures")]
        public async Task<IActionResult> GetAllAdventures(GetAdventureRequestDto getAdventureRequestDto)
        {
            const string methodName = nameof(GetAllAdventures);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing with TenantCode:{TenantCode}",
                    className, methodName, getAdventureRequestDto.TenantCode);

                var response = await _adventureService.GetAllAdventures(getAdventureRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},Response:{Response}",
                    className, methodName, getAdventureRequestDto.TenantCode, response);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, getAdventureRequestDto.TenantCode,StatusCodes.Status500InternalServerError,ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAdventureResponseDto(){ ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// ExportAdventures
        /// </summary>
        /// <param name="exportAdventureRequest"></param>
        /// <returns></returns>
        [HttpPost("export-adventures")]
        public async Task<IActionResult> ExportAdventures(ExportAdventureRequestDto exportAdventureRequest)
        {
            const string methodName = nameof(ExportAdventures);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing with TenantCode:{TenantCode}",
                    className, methodName, exportAdventureRequest.TenantCode);

                var response = await _adventureService.ExportTenantAdventures(exportAdventureRequest);

                if (response.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},Response:{Response}",
                    className, methodName, exportAdventureRequest.TenantCode, response);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, exportAdventureRequest.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAdventureResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("import-adventures")]
        public async Task<IActionResult> ImportAdventures(ImportAdventureRequestDto importAdventureRequest)
        {
            const string methodName = nameof(ImportAdventures);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing with Adventures:{adventures}",
                    className, methodName, importAdventureRequest.ToJson());

                var response = await _adventureService.ImportTenantAdventures(importAdventureRequest);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Import Adventures and Tenant adventures. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, importAdventureRequest.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occured with Request Json:{Json},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, importAdventureRequest.Adventures.ToJson(), StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
