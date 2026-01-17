namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class QuestionnaireDataDto
    {
        public long QuestionnaireId { get; set; }
        public string? QuestionnaireCode { get; set; }
        public long TaskRewardId { get; set; }
        public string? CtaTaskExternalCode { get; set; }
        public string? ConfigJson { get; set; }
    }
}
