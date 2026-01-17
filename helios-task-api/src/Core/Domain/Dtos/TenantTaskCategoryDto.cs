using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TenantTaskCategoryDto
    {
        public long TenantTaskCategoryId { get; set; }
        public long TaskCategoryId { get; set; }
        public string? TenantCode { get; set; }
        public string? ResourceJson { get; set; }
    }
    public class ExportTenantTaskCategoryDto
    {
        public TenantTaskCategoryDto? TenantTaskCategory { get; set; }
        public string? TaskCategoryCode { get; set; }
    }
}
