namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class QuestionnaireUxDto
    {
        public string? BackgroundUrl { get; set; }
        public QuestionnaireUxIconConfigDto QuestionIcon { get; set; } = new QuestionnaireUxIconConfigDto();
        public QuestionnaireUxIconConfigDto WrongAnswerIcon { get; set; } = new QuestionnaireUxIconConfigDto();
        public QuestionnaireUxIconConfigDto CorrectAnswerIcon { get; set; } = new QuestionnaireUxIconConfigDto();
    }
}
