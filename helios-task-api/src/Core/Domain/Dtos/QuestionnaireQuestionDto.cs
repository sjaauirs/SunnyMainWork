namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class QuestionnaireQuestionDto
    {
        public long QuestionnaireQuestionId { get; set; }
        public string? QuestionnaireQuestionCode { get; set; }
        public string? QuestionnaireJson { get; set; }
        public string? QuestionExternalCode { get; set; }
    }
}
