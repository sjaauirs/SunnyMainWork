namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class QuestionnaireImportDto
    {
        public QuestionnaireDto Questionnaire { get; set; } = new QuestionnaireDto();
        public QuestionnaireLanguageQuestionDto[] QuestionnaireQuestions { get; set; } = new QuestionnaireLanguageQuestionDto[0];
    }
}
