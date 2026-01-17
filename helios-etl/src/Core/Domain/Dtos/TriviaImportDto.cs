namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class TriviaImportDto
    {
        public TriviaDto Trivia { get; set; } = new TriviaDto();
        public TriviaLanguageQuestionDto[] TriviaQuestions { get; set; } = new TriviaLanguageQuestionDto[0];
    }
}
