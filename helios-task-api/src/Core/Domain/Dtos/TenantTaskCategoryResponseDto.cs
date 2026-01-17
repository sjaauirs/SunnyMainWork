using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TenantTaskCategoryResponseDto : BaseResponseDto
    {
        public TenantTaskCategoryDto? TenantTaskCategory { get; set; }
    }
}
