namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class TriviaQuestionGroupDataDto
    {
        public long Id { get; set; }
        public DateTime? ValidStartTs { get; set; }
        public DateTime? ValidEndTs { get; set; }
    }
}
