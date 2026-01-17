using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IComponentService
    {
        /// <summary>
        /// Creates new component with the data given
        /// </summary>
        /// <param name="createComponentRequestDto">request contains data to create new component</param>
        /// <returns>base response</returns>
        Task<BaseResponseDto> CreateComponent(ComponentRequestDto createComponentRequestDto);
        /// <summary>
        /// Get all component types
        /// </summary>
        /// <returns>List of component types</returns>
        Task<GetAllComponentTypesResponseDto> GetAllComponentTypes();
        /// <summary>
        /// Get all components
        /// </summary>
        /// <param name="getAllComponentsRequestDto">request contains data to retrieve All components</param>
        /// <returns>Returns all the Components which are specific to the Tenant</returns>
        Task<GetAllComponentsResponseDto> GetAllComponents(GetAllComponentsRequestDto getAllComponentsRequestDto);
        /// <summary>
        /// update existing component
        /// </summary>
        /// <param name="componentRequestDto">request contains data to update the component</param>
        /// <returns>returns the updated component</returns>
        Task<UpdateComponentResponseDto> UpdateComponent(ComponentRequestDto componentRequestDto);
        /// <summary>
        /// Imports a list of component types asynchronously by sending them to the CMS client.
        /// </summary>
        /// <param name="componentTypes">The list of component types to import.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an 
        /// <see cref="ImportComponentTypeResponseDto"/> indicating the result of the import operation,
        /// including any error information if the operation fails.
        /// </returns>
        Task<ImportComponentTypeResponseDto> ImportComponentTypesAsync(List<ComponentTypeDto> componentTypes);
    }
}
