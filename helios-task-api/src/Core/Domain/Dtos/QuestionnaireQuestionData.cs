using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireQuestionData
    {
        public long QuestionnaireQuestionId { get; set; }
        public required string? QuestionnaireQuestionCode { get; set; }
        public string? QuestionnaireJson { get; set; }
        public required string? QuestionExternalCode { get; set; }

        public string? UpdateUser { get; set; } = string.Empty;
    }
}
