using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class CreateValidicUserResponseDto : BaseResponseDto
    {
        public string Id { get; set; }
        public string Uid { get; set; }
        public Marketplace? Marketplace { get; set; }
        public Mobile? Mobile { get; set; }
        public string? Status { get; set; }
        public DateTime? Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
        public List<string>? errors { get; set; }
        public string OrgID { get; set; }
    }

    public class Marketplace
    {
        public string? Token { get; set; }
        public string? Url { get; set; }
    }

    public class Mobile
    {
        public string? Token { get; set; }
    }
}
