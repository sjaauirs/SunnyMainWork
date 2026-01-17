using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ConsumerWithTask
    {
        public ConsumerDto Consumer { get; set; } = null!;
        public PersonDto Person { get; set; } = null!;
        public List<ConsumerTaskDto> ConsumerTasks { get; set; } = new();
    }

}
