using SunnyRewards.Helios.Task.Core.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class AdventureTaskCollectionRequestDto
    {
        [Required]
        public Dictionary<string, List<string>> CohortTaskMap { get; set; } = null!;
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ConsumerCode { get; set; } = null!;

        public string? LanguageCode { get; set; } = Constant.LanguageCode;
        public Dictionary<string, string> LanguageComponentCodes { get; set; } = new Dictionary<string, string>();

    }
}
