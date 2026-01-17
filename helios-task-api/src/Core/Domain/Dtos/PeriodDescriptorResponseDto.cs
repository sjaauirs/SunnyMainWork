using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class PeriodDescriptorResponseDto : BaseResponseDto
    {
        public PeriodDescriptorDto PeriodDescriptorDtO { get; set; } = new PeriodDescriptorDto();
    }
}
