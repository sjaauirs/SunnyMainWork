namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class AdventureDto
    {
        public long AdventureId { get; set; }
        public string AdventureCode { get; set; } = null!;
        public string AdventureConfigJson { get; set; } = null!;
        public string? CmsComponentCode { get; set; }
        public string? LanguageCode { get; set; } = null;
    }

    public class AdventureConfig
    {
        public List<string> Cohorts { get; set; } = new();
    }
}
