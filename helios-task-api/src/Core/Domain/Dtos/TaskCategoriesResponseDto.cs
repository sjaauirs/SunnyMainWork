using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskCategoriesResponseDto : BaseResponseDto
    {
        public IList<TaskCategoryDto>? TaskCategories { get; set; }
    }
}
