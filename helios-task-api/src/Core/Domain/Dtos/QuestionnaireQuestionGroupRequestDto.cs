using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class QuestionnaireQuestionGroupRequestDto
    {
        [Required]
        public required QuestionnaireQuestionGroupPostRequestDto QuestionnaireQuestionGroup { get; set; }

        [Required]

        public required string QuestionnaireCode { get; set; }
        [Required]

        public required string QuestionnaireQuestionCode { get; set; }
    }
    public class QuestionnaireQuestionGroupPostRequestDto
    {
        public long? QuestionnaireQuestionGroupId { get; set; }
        public long? QuestionnaireId { get; set; }
        public long? QuestionnaireQuestionId { get; set; }
        [Required]
        public required int SequenceNbr { get; set; }
        [Required]
        public required DateTime ValidStartTs { get; set; }
        [Required]
        public required DateTime ValidEndTs { get; set; }
        [Required]
        public required string CreateUser { get; set; }
    }
}
