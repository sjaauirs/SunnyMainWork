using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TenantImportController : ControllerBase
    {
        private readonly ILogger<TenantImportController> _adminTenantImportLogger;
        private readonly ITenantImportService _tenantImportService;


        const string className = nameof(TenantImportController);
        public TenantImportController(ILogger<TenantImportController> adminTenantImportLogger, ITenantImportService tenantImportService

          )
        {
            _adminTenantImportLogger = adminTenantImportLogger;
            _tenantImportService = tenantImportService;
           
        }
        [HttpPost("tenant-import")]
        public async Task<IActionResult> UploadFile(
         [FromForm] TenantImportRequestDto tenantImportRequestDto
        )
        {
            const string methodName = nameof(UploadFile);
            try
            {
                tenantImportRequestDto.ImportOptions = JsonConvert.DeserializeObject<List<string>>(tenantImportRequestDto.ImportOptionsString);

                // Validate the importOptions list
                if (!tenantImportRequestDto.ImportOptions.Contains(nameof(ImportOption.ALL),StringComparer.OrdinalIgnoreCase))
                {
                    if (!tenantImportRequestDto.ImportOptions.All(option => Constant.validOptions.Contains(option.ToUpper())))
                    {
                        return StatusCode(StatusCodes.Status400BadRequest,new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid import options provided." });
                    }
                }
                if (string.IsNullOrEmpty(tenantImportRequestDto.tenantCode))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid tenant Code." });

                }
                if (!Path.GetExtension(tenantImportRequestDto.File.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Zip File Expected" });

                }
                if (tenantImportRequestDto.ImportOptions.Any(x => x.ToLower() == nameof(ImportOption.FIS).ToLower()|| x.ToLower() == nameof(ImportOption.ALL).ToLower()) 
                    && string.IsNullOrEmpty(tenantImportRequestDto.SponsorCode)
                    && string.IsNullOrEmpty(tenantImportRequestDto.CustomerCode))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "SponsorCode and ConsumerCode Cannot be null for fis import." });
                }


                var response = await _tenantImportService.TenantImport(tenantImportRequestDto);

                if (response.ErrorCode == null)
                {
                   return Ok(response);
                }
                else {
                    return StatusCode((int)response.ErrorCode, response);
                }


            }
            catch (Exception ex)
            {
                _adminTenantImportLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing with exception: {ex}", className, methodName, ex.Message);

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
       
    }
}
