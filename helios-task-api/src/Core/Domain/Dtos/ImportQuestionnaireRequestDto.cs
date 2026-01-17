namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ImportQuestionnaireRequestDto
    {
        public ImportQuestionnaireDetailDto? QuestionnaireDetailDto { get; set; }
        public string? TenantCode { get; set; }
    }
    public class ImportQuestionnaireDetailDto
    {
        public List<ExportQuestionnaireDto>? Questionnaire { get; set; }
        public List<QuestionnaireQuestionGroupDto>? QuestionnaireQuestionGroup { get; set; }
        public List<QuestionnaireQuestionDto>? QuestionnaireQuestion { get; set; }
    }
    public class ImportQuestionnaireQuestionMappingDto
    {
        public List<ImportQuestionnaireQuestionDto>? QuestionnaireCodeMapping { get; set; }
        public List<ImportQuestionnaireQuestionDto>? QuestionnaireQuestionMapping { get; set; }
    }
    public class ImportQuestionnaireQuestionDto
    {
        public long? Id { get; set; }
        public string? Code { get; set; }
    }
}
