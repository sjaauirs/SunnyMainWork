using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using static SunnyRewards.Helios.Task.Core.Domain.Dtos.TaskRewardDto;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ExportTaskResponseDto : BaseResponseDto
    {
        public List<ExportTaskDto>? Task { get; set; }
        public List<SubTaskDto>? SubTask { get; set; }
        public List<ExportTenantTaskCategoryDto>? TenantTaskCategory { get; set; }
        public List<TaskDetailDto>? TaskDetail { get; set; }
        public List<ExportTaskRewardDto>? TaskReward { get; set; }
        public List<TaskExternalMappingDto>? TaskExternalMapping { get; set; }
        public List<ExportTriviaDto>? Trivia { get; set; }
        public List<TriviaQuestionGroupDto>? TriviaQuestionGroup { get; set; }
        public List<TriviaQuestionDto>? TriviaQuestion { get; set; }
        public List<TermsOfServiceDto>? TermsOfService { get; set; }
        public List<ExportQuestionnaireDto>? Questionnaire { get; set; }
        public List<QuestionnaireQuestionGroupDto>? QuestionnaireQuestionGroup { get; set; }
        public List<QuestionnaireQuestionDto>? QuestionnaireQuestion { get; set; }
    }
}
