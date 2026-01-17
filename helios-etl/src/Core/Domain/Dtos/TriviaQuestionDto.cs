using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class TriviaLanguageQuestionDto
    {
        public Dictionary<string, LanguageQuestion> LocalizedInfo { get; set; } = new Dictionary<string, LanguageQuestion>();
        public string? AnswerType { get; set; }
        public string? LayoutType { get; set; }
        public string? QuestionExternalCode { get; set; }
        public DateTime? ValidStartTs { get; set; }
        public DateTime? ValidEndTs { get; set; }
    }
    public class LanguageQuestion
    {
        public TriviaQuestionLearningDto Learning { get; set; } = new TriviaQuestionLearningDto();
        public string? QuestionText { get; set; }
        public string[]? AnswerText { get; set; }
        public int[]? CorrectAnswer { get; set; }
    }

    public class TriviaQuestionDto
    {
        public TriviaQuestionLearningDto Learning { get; set; } = new TriviaQuestionLearningDto();
        public string? QuestionText { get; set; }
        public string[]? AnswerText { get; set; }
        public int[]? CorrectAnswer { get; set; }
        public string? AnswerType { get; set; }
        public string? LayoutType { get; set; }
        public string? QuestionExternalCode { get; set; }
        public DateTime? ValidStartTs { get; set; }
        public DateTime? ValidEndTs { get; set; }
    }
}