using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface ICmsService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getComponentListRequestDto"></param>
        /// <returns></returns>
        Task<GetComponentListResponseDto> GetComponentList(GetComponentListRequestDto getComponentListRequestDto, string? requestId = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findStoreRequestDTO"></param>
        /// <returns></returns>
        Task<List<StoreResponseMockDTO>> FindStoreMock(FindStoreRequestDTO findStoreRequestDTO);
        Task<FaqSectionResponseDto> GetFaqSection(FaqRetriveRequestDto faqRetriveRequestDto);

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <param name="getComponentRequestDto">The get component request dto.</param>
        /// <returns></returns>
        Task<GetComponentResponseDto> GetComponent(GetComponentRequestDto getComponentRequestDto);
        Task<GetComponentListResponseDto> GetCmsComponentList(GetComponentListRequestDto getComponentListRequestDto, string? requestId = null);

        /// <summary>
        /// Gets the List of components based on the tenant code and component type name
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        Task<GetComponentsResponseDto> GetTenantComponentsByTypeName(GetTenantComponentByTypeNameRequestDto requestDto);
        
        /// <summary>
        /// Gets multiple lists of components based on the tenant code and list of component type names
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        Task<GetTenantComponentsByTypeNamesResponseDto> GetTenantComponentsByTypeNames(GetTenantComponentsByTypeNamesRequestDto requestDto);
        
        Task<GetComponentByCodeResponseDto> GetComponentBycode(GetComponentByCodeRequestDto getComponentRequestDto);

    }
}
