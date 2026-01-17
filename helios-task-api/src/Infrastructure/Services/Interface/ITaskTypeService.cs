using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITaskTypeService
    {
        /// <summary>
        /// Retrieves a list of task types from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TaskTypesResponseDto> GetTaskTypesAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypeId"></param>
        /// <returns></returns>
        Task<TaskTypeResponseDto> GetTaskTypeById(long taskTypeId);

        Task<TaskTypeResponseDto> GetTaskTypeByTypeCode(string taskTypeCode);
        /// <summary>
        /// Imports a list of task types from the request DTO. 
        /// </summary>
        /// <param name="taskTypeRequestDto">The import request containing task types.</param>
        /// <returns>Import result with error messages if any failures occur.</returns>
        Task<ImportTaskTypeResponseDto> ImportTaskTypesAsync(ImportTaskTypeRequestDto taskTypeRequestDto);
    }
}
