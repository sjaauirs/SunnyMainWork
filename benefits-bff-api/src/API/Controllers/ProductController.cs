using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/fis")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _productLogger;
        private readonly IProductService _productService;
        private const string className = nameof(ProductController);

        public ProductController(ILogger<ProductController> productLogger, IProductService productService)
        {
            _productLogger = productLogger;
            _productService = productService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchProductRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("product-search")]
        public async Task<IActionResult> SearchProduct(PostSearchProductRequestDto searchProductRequestDto)
        {
            const string methodName = nameof(SearchProduct);
            _productLogger.LogInformation("{ClassName}.{MethodName} - Started processing Product Search with TenantCode: {TenantCode}, " +
                "ConsumerCode: {ConsumerCode}, UPC: {Upc}", className, methodName, searchProductRequestDto.TenantCode,
                searchProductRequestDto.ConsumerCode, searchProductRequestDto.Upc);
            try
            {
                var response = await _productService.SearchProduct(searchProductRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _productLogger.LogError("{ClassName}.{MethodName} - Error occurred while processing Product Search - ConsumerCode: {ConsumerCode},TenantCode: {TenantCode}" +
                        " - ErrorCode: {ErrorCode}, Error : {ErrorMessage}", className, methodName, searchProductRequestDto.ConsumerCode, searchProductRequestDto.TenantCode, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "SearchProduct API: Error occurred while processing product search request,ConsumerCode: {ConsumerCode},TenantCode: {TenantCode}," +
                    " ErrorCode:{ErrorCode}, ERROR: {Message}", searchProductRequestDto.ConsumerCode, searchProductRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new FisProductSearchResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
