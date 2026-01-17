namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class TriviaDto
    {
        public string? TriviaTaskExternalCode { get; set; }
        public string? CtaTaskExternalCode { get; set; }
        public TriviaConfigDto Config { get; set; } = new TriviaConfigDto();
    }
}
