using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class EtlTaskImportDto
    {
        [JsonProperty("planYear")]
        public int PlanYear { get; set; }

        [JsonProperty("taskName")]
        public string? TaskName { get; set; }

        [JsonProperty("deepLink")]
        public string? DeepLink { get; set; }

        [JsonProperty("taskCategory")]
        public string? TaskCategory { get; set; }

        [JsonProperty("rewardValue")]
        public decimal? RewardValue { get; set; }

        [JsonProperty("rewardType")]
        public string? RewardType { get; set; }

        [JsonProperty("priority")]
        public int? Priority { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string? EndDate { get; set; }

        [JsonProperty("selfReport")]
        public bool? SelfReport { get; set; }

        [JsonProperty("taskExternalCode")]
        public string? TaskExternalCode { get; set; }

        [JsonProperty("recurring")]
        public bool? Recurring { get; set; }

        [JsonProperty("frequencyForRecurringActions")]
        public string? FrequencyForRecurringActions { get; set; }

        [JsonProperty("recurringJson")]
        public RecurrenceSettingsDto? RecurringJson { get; set; }

        [JsonProperty("membershipType")]
        public string? MembershipType { get; set; }

        [JsonProperty("completionCriteriaType")]
        public string? CompletionCriteriaType { get; set; }

        [JsonProperty("completionUIComponents")]
        public string? CompletionUIComponents { get; set; }

        [JsonProperty("taskCompletionSteps")]
        public object? TaskCompletionSteps { get; set; }

        [JsonProperty("taskTypeName")]
        public string? TaskTypeName { get; set; }

        [JsonProperty("cohort")]
        public string? Cohort { get; set; }

        [JsonProperty("localizeInfo")]
        public List<LocalizedInfo>? LocalizeInfo { get; set; }

      
    }

    public class LocalizedInfo
    {
        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("taskHeader")]
        public string? TaskHeader { get; set; }

        [JsonProperty("cta")]
        public string? Cta { get; set; }

        [JsonProperty("taskDescription")]
        public object? TaskDescription { get; set; }
    }

    public class TaskImportDto
    {
        public List<EtlTaskImportDto> Tasks { get; set; } = new List<EtlTaskImportDto>();
    }
}
