namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class QuestionnaireDto
    {
        public string? QuestionnaireTaskExternalCode { get; set; }
        public string? CtaTaskExternalCode { get; set; }
        public QuestionnaireConfigDto Config { get; set; } = new QuestionnaireConfigDto();
    }
}
