using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireQuestionGroupDto
    {
        public long QuestionnaireQuestionGroupId { get; set; }
        public long QuestionnaireId { get; set; }
        public long QuestionnaireQuestionId { get; set; }
        public int SequenceNbr { get; set; }
        public DateTime ValidStartTs { get; set; }
        public DateTime ValidEndTs { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;
    }
}
