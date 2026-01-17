using FluentNHibernate.Automapping.Steps;
using log4net.Util;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaDto
    {
        public  long TriviaId { get; set; }
        public  string? TriviaCode { get; set; }
        public  long TaskRewardId { get; set; }
        public  string? CtaTaskExternalCode { get; set; }
        public  string? ConfigJson { get; set; }
        public TaskRewardDetailDto? taskRewardDetail { get; set; }
    }
    public class ExportTriviaDto
    {   
        public  string? TaskExternalCode { get; set; }
        public TriviaDto? Trivia { get; set; }
    }
}
