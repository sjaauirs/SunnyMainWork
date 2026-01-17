using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    public class TenantExportController : ControllerBase
    {
        private readonly ITenantExportService _tenantExportService;
        private readonly ILogger<TenantExportController> _logger;
        const string _className = nameof(TenantExportController);

        public TenantExportController(ILogger<TenantExportController> logger, ITenantExportService tenantExportService)
        {
            _logger = logger;
            _tenantExportService = tenantExportService;
        }



        /// <summary>
        /// Exports tenant data based on the provided request.
        /// </summary>
        /// <param name="request">The export tenant request DTO containing the necessary parameters.</param>
        /// <returns>An IActionResult containing the export file or an error response.</returns>
        [HttpPost("export-tenant")]
        public async Task<IActionResult> ExportTenant([FromBody] ExportTenantRequestDto request)
        {
            var methodName = nameof(ExportTenant);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", _className, methodName, request.TenantCode);

                var response = await _tenantExportService.ExportTenantAsync(request);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Tenant export. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", _className, methodName, request.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                return File(response.ExportFileData!, response.FileType!, response.FileName);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during Tenant export. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", _className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportTenantResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }

}
