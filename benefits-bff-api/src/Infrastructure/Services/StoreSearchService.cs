using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class StoreSearchService : IStoreSearchService
    {
        private readonly ILogger<StoreSearchService> _logger;
        private readonly IFisClient _fisClient;
        private readonly IConfiguration _configuration;
        private const string className = nameof(StoreSearchService);

        public StoreSearchService(ILogger<StoreSearchService> logger, IFisClient fisClient,
             IConfiguration configuration)
        {
            _fisClient = fisClient;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Search stores by Latitude and Longitude
        /// </summary>
        /// <param name="searchStoresRequestDto"></param>
        /// <returns></returns>
        public async Task<PostSearchStoresResponseDto> SearchStores(PostSearchStoresRequestDto searchStoresRequestDto)
        {
            const string methodName = nameof(SearchStores);
            try
            {
                if (IsInvalidSearchRequest(searchStoresRequestDto))
                {
                    return new PostSearchStoresResponseDto { ErrorCode = StatusCodes.Status400BadRequest };
                }

                var searchStoreResponse = await _fisClient.Post<PostSearchStoresResponseDto>("fis/store-search", searchStoresRequestDto);

                

                Dictionary<string, string> fisTags = _configuration.GetSection(CommonConstants.FISStoreTags)?.GetChildren()
                    ?.AsEnumerable()?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();

                if (fisTags.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} - FISStoreTags key is not configured in appsettings", className, methodName);
                    throw new ArgumentNullException("SearchStores: FISStoreTags key is not configured in appsettings");
                }
                if (searchStoreResponse.Stores == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Stores response is null for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}", 
                        className,methodName, searchStoresRequestDto.TenantCode, searchStoresRequestDto.ConsumerCode);
                    throw new ArgumentNullException($"SearchStores: Stores response is null with request: {searchStoresRequestDto.ToJson()}");
                }

                foreach (var store in searchStoreResponse.Stores)
                {
                    store.StoreAttributes = (from storeAttribute in store.StoreAttributes
                                             join tag in fisTags on storeAttribute.AttributeName equals tag.Key
                                             select new StoreAttributeDto
                                             {
                                                 AttributeName = tag.Value,
                                                 AttributeValue = storeAttribute.AttributeValue
                                             }).ToList();
                }

                return searchStoreResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} -  Error occurred while searching for stores with TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}, ERROR:{Message}",
                    className, methodName, searchStoresRequestDto.TenantCode, searchStoresRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }

        }
        private bool IsInvalidSearchRequest(PostSearchStoresRequestDto requestDto)
        {
            return string.IsNullOrEmpty(requestDto.TenantCode)
                || string.IsNullOrEmpty(requestDto.ConsumerCode)
                || requestDto.Latitude == null
                || requestDto.Latitude == 0.0
                || requestDto.Longitude == 0.0;
        }
    }
}
