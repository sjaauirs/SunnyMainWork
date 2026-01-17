using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class QuestionnaireLanguageQuestionDto
    {
        public Dictionary<string, QuestionnaireLanguageQuestion> LocalizedInfo { get; set; } = new Dictionary<string, QuestionnaireLanguageQuestion>();
        public string? AnswerType { get; set; }
        public string? LayoutType { get; set; }
        public string? QuestionExternalCode { get; set; }
        public DateTime? ValidStartTs { get; set; }
        public DateTime? ValidEndTs { get; set; }
    }
    public class QuestionnaireLanguageQuestion
    {
        public QuestionnaireQuestionLearningDto Learning { get; set; } = new QuestionnaireQuestionLearningDto();
        public string? QuestionText { get; set; }
        public string[]? AnswerText { get; set; }
        public int[]? CorrectAnswer { get; set; }
        public AnswerScaleDto? AnswerScale { get; set; }

    }
    public class AnswerScaleDto
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public Dictionary<int, string> Labels { get; set; } = new Dictionary<int, string>();
    }

    public class QuestionnaireQuestionDto
    {
        public QuestionnaireQuestionLearningDto Learning { get; set; } = new QuestionnaireQuestionLearningDto();
        public string? QuestionText { get; set; }
        public string[]? AnswerText { get; set; }
        public int[]? CorrectAnswer { get; set; }
        public string? AnswerType { get; set; }
        public string? LayoutType { get; set; }
        public string? QuestionExternalCode { get; set; }
        public DateTime? ValidStartTs { get; set; }
        public DateTime? ValidEndTs { get; set; }
        public AnswerScaleDto? AnswerScale { get; set; }
    }
}