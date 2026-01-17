using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaQuestionRequestDto 
    {
        public long TriviaQuestionId { get; set; }

        [Required]

        public required string? TriviaQuestionCode { get; set; }
        public string? TriviaJson { get; set; }
        [Required]
        public required string? QuestionExternalCode { get; set; }
        [Required]
        public required string? CreateUser { get; set; }

    }
}
