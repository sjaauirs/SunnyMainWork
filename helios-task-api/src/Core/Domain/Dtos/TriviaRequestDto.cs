using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaRequestDto
    {
        [Required]
        public required string TaskRewardCode { get; set; }
        [Required]
        public required Trivia trivia { get; set; }
    }
    public class Trivia
    {
        public long TriviaId { get; set; }
        [Required]
        public required string TriviaCode { get; set; }
       
        public long TaskRewardId { get; set; }
        public string? CtaTaskExternalCode { get; set; }
        public string? ConfigJson { get; set; }
        [Required]
        public required string CreateUser { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;

    }
}
