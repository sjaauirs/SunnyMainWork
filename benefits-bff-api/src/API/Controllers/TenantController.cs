using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;


namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    [Authorize]

    public class TenantController : ControllerBase
    {
        private readonly ILogger<TenantController> _tenantLogger;
        private readonly ITenantService _tenantService;
        private const string className = nameof(TenantController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantLogger"></param>
        /// <param name="tenantService"></param>
        public TenantController(ILogger<TenantController> tenantLogger,
            ITenantService tenantService)
        {
            _tenantLogger = tenantLogger;
            _tenantService = tenantService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantByConsumerCodeRequestDto"></param>
        /// <returns></returns>
        [HttpGet("get-tenant-by-consumer-code")]
        public async Task<ActionResult<GetTenantResponseDto>> GetTenantByConsumerCode(string consumerCode)
        {
            const string methodName = nameof(GetTenantByConsumerCode);
            try
            {
                _tenantLogger.LogInformation("{ClassName}.{MethodName} - Started processing GetTenantByConsumerCode With ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);
                var tenantResponse = await _tenantService.GetTenantByConsumerCode(consumerCode);

                return tenantResponse != null ? Ok(tenantResponse) : NotFound();
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing GetTenantByConsumerCode With ConsumerCode : {ConsumerCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", 
                    className, methodName, consumerCode, StatusCodes.Status500InternalServerError,ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTenantResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
            finally
            {
                _tenantLogger.LogInformation("{ClassName}.{MethodName} - GetTenantByConsumerCode API - Ended With ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);
            }
        }

    }
}
