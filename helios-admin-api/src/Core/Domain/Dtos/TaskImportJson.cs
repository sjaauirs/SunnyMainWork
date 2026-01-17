using Newtonsoft.Json;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class Data
    {
        public List<ImportTaskDto> Task { get; set; }
        public List<SubTaskDto> SubTask { get; set; }
        public List<ExportTenantTaskCategoryDto> TenantTaskCategory { get; set; }
        public List<TaskDetailDto> TaskDetail { get; set; }
        public List<ImportTaskRewardDto> TaskReward { get; set; }
        public List<TaskExternalMappingDto> TaskExternalMapping { get; set; }
        public List<TermsOfServiceDto> TermsOfService { get; set; }
        public List<ImportTriviaDto> Trivia { get; set; }
        public List<TriviaQuestionGroupDto> TriviaQuestionGroup { get; set; }
        public List<TriviaQuestionDto> TriviaQuestion { get; set; }
        public List<ExportTaskRewardCollectionDto> TaskRewardCollection { get; set; }
        public List<ImportQuestionnaireDto> Questionnaire { get; set; }
        public List<QuestionnaireQuestionGroupDto> QuestionnaireQuestionGroup { get; set; }
        public List<QuestionnaireQuestionDto> QuestionnaireQuestion { get; set; }
    }


    public class TaskImportJson
    {
        public string ExportVersion { get; set; }
        public DateTime ExportTs { get; set; }
        public string ExportType { get; set; }
        public Data Data { get; set; }

    }
    public class ImportTriviaDto
    {
        [JsonProperty("taskExternalCode")]
        public string? TaskExternalCode { get; set; }
        [JsonProperty("trivia")]
        public TriviaDto? Trivia { get; set; }
    }
    public class ImportQuestionnaireDto
    {
        [JsonProperty("taskExternalCode")]
        public string? TaskExternalCode { get; set; }
        [JsonProperty("questionnaire")]
        public QuestionnaireDto? Questionnaire { get; set; }
    }
    public class ImportTaskDto
    {
        [JsonProperty("taskTypeCode")]
        public string? TaskTypeCode { get; set; }
        [JsonProperty("taskCategoryCode")]
        public string? TaskCategoryCode { get; set; }
        [JsonProperty("task")]
        public TaskDto? Task { get; set; }
    }

    public class ImportTaskRewardDto
    {
        [JsonProperty("taskRewardTypeCode")]
        public string? TaskRewardTypeCode { get; set; }
        [JsonProperty("taskReward")]
        public TaskRewardDto? TaskReward { get; set; }
    }


    public class TermsOfService
    {
        public int TermsOfServiceId { get; set; }
        public string TermsOfServiceText { get; set; }
        public string LanguageCode { get; set; }

        public string TermsOfServiceCode { get; set; }
    }





}
