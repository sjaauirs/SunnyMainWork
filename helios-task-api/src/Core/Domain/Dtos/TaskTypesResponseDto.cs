using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskTypesResponseDto : BaseResponseDto
    {
        public IList<TaskTypeDto>? TaskTypes { get; set; }
    }
}
