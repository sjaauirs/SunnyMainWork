using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PersonRoleDto : BaseDto
    {
        public long PersonRoleId { get; set; }
        public long PersonId { get; set; }
        public long RoleId { get; set; }
        public string? CustomerCode { get; set; }
        public string? SponsorCode { get; set; }
        public string? TenantCode { get; set; }
    }
}