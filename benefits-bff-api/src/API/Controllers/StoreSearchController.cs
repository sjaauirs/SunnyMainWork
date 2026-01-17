using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/fis")]
    [ApiController]
    [Authorize]
    public class StoreSearchController : ControllerBase
    {
        private readonly ILogger<StoreSearchController> _logger;
        private readonly IStoreSearchService _storeSearchService;
        private const string className=nameof(StoreSearchController);

        public StoreSearchController(IStoreSearchService storeSearchService, ILogger<StoreSearchController> logger)
        {
            _storeSearchService = storeSearchService;
            _logger = logger;
        }

        [HttpPost("store-search")]
        public async Task<IActionResult> StoreSearch(PostSearchStoresRequestDto request)
        {
            const string methodName=nameof(StoreSearch);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing StoreSearch. TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}", className, methodName, request.TenantCode, request.ConsumerCode);
            try
            {

                var response = await _storeSearchService.SearchStores(request);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName} - Error occured while processing StoreSearch with TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode} - ErrorCode: {ErrorCode} , ERROR:{ErrorMessage}",
                        className, methodName, request.TenantCode,request.ConsumerCode, response.ErrorCode,response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing StoreSearch with TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, request.TenantCode,request.ConsumerCode, StatusCodes.Status500InternalServerError,ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new PostSearchStoresResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }

        }
    }
}
