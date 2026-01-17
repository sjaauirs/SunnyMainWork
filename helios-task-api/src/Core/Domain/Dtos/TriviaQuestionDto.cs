namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaQuestionDto
    {
        public long TriviaQuestionId { get; set; }
        public  string? TriviaQuestionCode { get; set; }
        public string? TriviaJson { get; set; }
        public  string? QuestionExternalCode { get; set; }
    }
}
