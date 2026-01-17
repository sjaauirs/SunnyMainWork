namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaQuestionData
    {
        public long TriviaQuestionId { get; set; }
        public required string? TriviaQuestionCode { get; set; }
        public string? TriviaJson { get; set; }
        public required string? QuestionExternalCode { get; set; }

        public string? UpdateUser { get; set; } = string.Empty;

    }
}
