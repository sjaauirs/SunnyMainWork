using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class QuestionnaireRequestDto
    {
        [Required]
        public required string TaskRewardCode { get; set; }
        [Required]
        public required Questionnaire questionnaire { get; set; }
    }
    public class Questionnaire
    {
        public long QuestionnaireId { get; set; }
        [Required]
        public required string QuestionnaireCode { get; set; }

        public long TaskRewardId { get; set; }
        public string? CtaTaskExternalCode { get; set; }
        public string? ConfigJson { get; set; }
        [Required]
        public required string CreateUser { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;

    }
}
