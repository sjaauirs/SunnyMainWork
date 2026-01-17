using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireDto
    {
        public long QuestionnaireId { get; set; }
        public string? QuestionnaireCode { get; set; }
        public long TaskRewardId { get; set; }
        public string? CtaTaskExternalCode { get; set; }
        public string? ConfigJson { get; set; }
        public TaskRewardDetailDto? taskRewardDetail { get; set; }
    }
    public class ExportQuestionnaireDto
    {
        public string? TaskExternalCode { get; set; }
        public QuestionnaireDto? Questionnaire { get; set; }
    }
}
