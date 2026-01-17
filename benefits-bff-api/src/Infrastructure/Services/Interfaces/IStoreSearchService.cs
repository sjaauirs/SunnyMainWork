using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IStoreSearchService
    {
        /// <summary>
        /// Search stores by Latitude and Longitude
        /// </summary>
        /// <param name="searchStoresRequestDto"></param>
        /// <returns></returns>
        Task<PostSearchStoresResponseDto> SearchStores(PostSearchStoresRequestDto searchStoresRequestDto);
    }
}
