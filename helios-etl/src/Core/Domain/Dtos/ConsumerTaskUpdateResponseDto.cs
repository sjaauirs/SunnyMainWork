using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ConsumerTaskUpdateResponseDto : BaseResponseDto
    {
        public ConsumerTaskDto? ConsumerTask { get; set; }
    }
}
