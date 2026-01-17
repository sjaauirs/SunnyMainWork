using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaQuestionGroupRequestDto
    {
        [Required]
        public required TriviaQuestionGroupPostRequestDto TriviaQuestionGroup { get; set; }

        [Required]

        public required string TriviaCode { get; set; }
        [Required]

        public required string TriviaQuestionCode { get; set; }
    }
    public class TriviaQuestionGroupPostRequestDto
    {
        public long? TriviaQuestionGroupId { get; set; }
        public long? TriviaId { get; set; }
        public long? TriviaQuestionId { get; set; }
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
