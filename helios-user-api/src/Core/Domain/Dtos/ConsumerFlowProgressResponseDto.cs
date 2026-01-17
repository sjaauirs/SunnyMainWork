using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerFlowProgressResponseDto : BaseResponseDto
    {
        public ConsumerFlowProgressDto ConsumerFlowProgress { get; set; }
    }
}