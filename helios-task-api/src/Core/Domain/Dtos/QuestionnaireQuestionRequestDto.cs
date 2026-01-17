using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class QuestionnaireQuestionRequestDto
    {
        public long QuestionnaireQuestionId { get; set; }

        [Required]

        public required string? QuestionnaireQuestionCode { get; set; }
        public string? QuestionnaireJson { get; set; }
        [Required]
        public required string? QuestionExternalCode { get; set; }
        [Required]
        public required string? CreateUser { get; set; }
    }
}
