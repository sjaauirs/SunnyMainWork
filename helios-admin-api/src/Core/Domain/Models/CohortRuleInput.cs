using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class CohortRuleInput
    {
        public PersonDto Person { get; set; } = default!;
        public ConsumerDto Consumer { get; set; } = default!;
    }
}
