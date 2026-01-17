namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaQuestionGroupDto
    {
        public  long TriviaQuestionGroupId { get; set; }
        public  long TriviaId { get; set; }
        public  long TriviaQuestionId { get; set; }
        public  int SequenceNbr { get; set; }
        public  DateTime ValidStartTs { get; set; }
        public  DateTime ValidEndTs { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;

    }
}
