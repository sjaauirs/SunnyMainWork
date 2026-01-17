namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class TriviaUxDto
    {
        public string? BackgroundUrl { get; set; }
        public TriviaUxIconConfigDto QuestionIcon { get; set; } = new TriviaUxIconConfigDto();
        public TriviaUxIconConfigDto WrongAnswerIcon { get; set; } = new TriviaUxIconConfigDto();
        public TriviaUxIconConfigDto CorrectAnswerIcon { get; set; } = new TriviaUxIconConfigDto();
    }
}
