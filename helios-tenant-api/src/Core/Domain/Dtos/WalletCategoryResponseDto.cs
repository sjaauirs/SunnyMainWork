using SunnyRewards.Helios.Common.Core.Domain.Dtos;
namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class WalletCategoryResponseDto : BaseResponseDto
    {
        public int Id { get; set; }
        public string TenantCode { get; set; } = string.Empty;
        public long WalletTypeId { get; set; }
        public string WalletTypeCode { get; set; } = string.Empty;
        public int CategoryFk { get; set; }
        public string? ConfigJson { get; set; }
    }
}
