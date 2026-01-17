using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class GetTenantNotificationCategoryResponseDto : BaseResponseDto
    {
        public IList<TenantNotificationCategoryDto>? TenantNotificationCategoryList { get; set; }
    }

    public class TenantNotificationCategoryDto
    {
        public long TenantNotificationCategoryId { get; set; }
        public string? TenantNotificationCategoryCode { get; set; }
        public long NotificationCategoryId { get; set; }
        public string? NotificationCategoryName { get; set; }
    }
}
