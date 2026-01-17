using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Enums;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Text.Json;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _productServiceLogger;
        private readonly IFisClient _fisClient;
        private readonly IConsumerActivityService _consumerActivityService;
        private const string className = nameof(ProductService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productServiceLogger"></param>
        /// <param name="fisClient"></param>
        public ProductService(ILogger<ProductService> productServiceLogger, IFisClient fisClient, IConsumerActivityService consumerActivityService)
        {
            _productServiceLogger = productServiceLogger;
            _fisClient = fisClient;
            _consumerActivityService = consumerActivityService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchProductRequestDto"></param>
        /// <returns></returns>
        public async Task<ProductSearchResponseDto> SearchProduct(PostSearchProductRequestDto searchProductRequestDto)
        {
            const string methodName = nameof(SearchProduct);
            try
            {
                if (IsInvalidSearchRequest(searchProductRequestDto))
                {
                    return new ProductSearchResponseDto { ErrorCode = StatusCodes.Status400BadRequest };
                }
                var response = await _fisClient.Post<ProductSearchResponseDto>("fis/product-search", searchProductRequestDto);
                _productServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved SearchProduct Successfully for TenantCode:{TenantCode},ConsumerCode:{ConsumerCode}",
                    className, methodName, searchProductRequestDto.TenantCode, searchProductRequestDto.ConsumerCode);

                await HandleCreateConsumerActivty(searchProductRequestDto, methodName, response);
                return response;
            }
            catch (Exception ex)
            {
                _productServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while Retrieving SearchProduct for TenantCode:{TenantCode},ConsumerCode:{ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, searchProductRequestDto.TenantCode, searchProductRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task HandleCreateConsumerActivty(PostSearchProductRequestDto searchProductRequestDto, string methodName, ProductSearchResponseDto response)
        {
            try
            {
                var requestDto = GetConsumerActivityRequestDto(searchProductRequestDto, response);
                // Invoking create consumer-activity API
                _ = await _consumerActivityService.CreateConsumerActivityAsync(requestDto);
            }
            catch (Exception ex)
            {
                _productServiceLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing for TenantCode:{TenantCode},ConsumerCode:{ConsumerCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, searchProductRequestDto.TenantCode, searchProductRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        private bool IsInvalidSearchRequest(PostSearchProductRequestDto requestDto)
        {
            return string.IsNullOrEmpty(requestDto.TenantCode)
                || string.IsNullOrEmpty(requestDto.ConsumerCode)
                || string.IsNullOrEmpty(requestDto.Upc);
        }

        private static ConsumerActivityRequestDto GetConsumerActivityRequestDto(PostSearchProductRequestDto postSearchProductRequestDto, ProductSearchResponseDto productSearchResponseDto)
        {
            var activityJson = PrepareConsumerActivityJson(postSearchProductRequestDto.Upc, productSearchResponseDto);
            return new ConsumerActivityRequestDto()
            {
                TenantCode = postSearchProductRequestDto.TenantCode ?? string.Empty,
                ConsumerCode = postSearchProductRequestDto.ConsumerCode ?? string.Empty,
                ActivitySource = CommonConstants.BenefitsApp,
                ActivityType = ConsumerActivityType.PRODUCT_SEARCH.ToString(),
                ActivityJson = activityJson,
            };
        }

        private static string PrepareConsumerActivityJson(string? upc, ProductSearchResponseDto responseDto)
        {
            var jsonObject = new
            {
                activityType = ConsumerActivityType.PRODUCT_SEARCH.ToString(),
                data = new
                {
                    productSearchActivityData = new
                    {
                        upc = upc,
                        productSearchResult = responseDto
                    }
                }
            };
            return JsonSerializer.Serialize(jsonObject);
        }
    }
}



