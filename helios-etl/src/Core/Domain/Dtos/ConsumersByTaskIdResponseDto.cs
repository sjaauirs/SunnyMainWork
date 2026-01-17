using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ConsumersByTaskIdResponseDto : BaseResponseDto
    {
        public List<ConsumerWithTask> consumerwithTask { get; set; } = null!;
        public int totalconsumersTasks { get; set; }
    }

}
