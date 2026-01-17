using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ISubtaskService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        System.Threading.Tasks.Task CreateConsumerSubtask(UpdateConsumerTaskDto consumerTaskDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getConsumerSubtasksRequestDto"></param>
        /// <returns></returns>
        Task<GetConsumerSubTaskResponseDto> GetConsumerSubtask(GetConsumerSubtasksRequestDto getConsumerSubtasksRequestDto);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateSubtaskRequestDto"></param>
        /// <returns></returns>
        Task<UpdateSubtaskResponseDto> UpdateConsumerSubtask(UpdateSubtaskRequestDto updateSubtaskRequestDto);
        Task<BaseResponseDto> CreateSubTask(SubtaskRequestDto requestDto);

        Task<SubtaskResponseDto> UpdateSubtask(SubTaskUpdateRequestDto requestDto);
    }
}
