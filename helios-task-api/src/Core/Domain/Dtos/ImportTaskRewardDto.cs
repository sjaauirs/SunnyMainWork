using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ImportTaskRewardDto:TaskRewardRequestDto
    {
        public string? NewRewardCode { get; set; }
    }
}
