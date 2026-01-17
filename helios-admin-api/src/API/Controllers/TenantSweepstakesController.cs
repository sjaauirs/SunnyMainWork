using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/tenant-sweepstakes")]
    [ApiController]
    public class TenantSweepstakesController : ControllerBase
    {
        private readonly ILogger<TenantSweepstakesController> _logger;
        private readonly ITenantSweepstakesService _tenantSweepstakesService;
        private const string className = nameof(TenantSweepstakesController);
        public TenantSweepstakesController(ILogger<TenantSweepstakesController> logger, ITenantSweepstakesService tenantSweepstakesService)
        {
            _logger = logger;
            _tenantSweepstakesService = tenantSweepstakesService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTenantSweepstakes([FromBody] TenantSweepstakesRequestDto tenantSweepstakesRequestDto)
        {
            const string methodName = nameof(CreateTenantSweepstakes);
            try
            {

                _logger.LogInformation("{ClassName}.{MethodName} - Started processing create tenant sweepstakes with SweepstakesCode:{Code},TenantCode:{Tenant}",
                        className, methodName, tenantSweepstakesRequestDto.SweepstakesCode, tenantSweepstakesRequestDto.TenantSweepstakes.TenantCode);

                var response = await _tenantSweepstakesService.CreateTenantSweepStakes(tenantSweepstakesRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during tenant sweepstakes creation. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, tenantSweepstakesRequestDto.TenantSweepstakes.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }


                _logger.LogInformation("{ClassName}.{MethodName} - Successfully created tenant sweepstakes with sweestakesCode:{Code},TenantCode:{Tenant}",
                    className, methodName, tenantSweepstakesRequestDto.SweepstakesCode, tenantSweepstakesRequestDto.TenantSweepstakes.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while creating tenant sweepstakes with SweestakesCode:{Code},TenantCode:{TenantCode},ErrorCode:{Code},ERROR:{Msg}",
                        className, methodName, tenantSweepstakesRequestDto.SweepstakesCode, tenantSweepstakesRequestDto.TenantSweepstakes.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
