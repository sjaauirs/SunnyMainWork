using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ConsumersByTaskIdResponseDto : BaseResponseDto
    {
        public List<ConsumerWithTask> consumerwithTask { get; set; } = null!;
        public int totalconsumersTasks { get; set; }
    }

    public class ConsumerWithTask
    {
        public ConsumerDto Consumer { get; set; } = null!;
        public PersonDto Person { get; set; } = null!;
        public List<ConsumerTaskDto> ConsumerTasks { get; set; } = new();
    }
}
