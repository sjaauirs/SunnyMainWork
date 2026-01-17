using SunnyRewards.Helios.User.Core.Domain.enums;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdateAgreementStatusDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? AgreementStatus { get; set; }
    }
}
