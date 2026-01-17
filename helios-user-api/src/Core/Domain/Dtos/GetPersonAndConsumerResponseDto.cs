using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetPersonAndConsumerResponseDto: BaseResponseDto
    {
        public PersonDto? Person { get; set; }
        public ConsumerDto? Consumer { get; set; }
    }

    public class ConsumersAndPersonsListResponseDto : BaseResponseDto
    {
        public List<ConsumersAndPersons> ConsumerAndPersons { get; set; } = new List<ConsumersAndPersons>();
    }

    public class ConsumersAndPersons
    {
        public PersonDto? Person { get; set; }
        public ConsumerDto? Consumer { get; set; }
    }
}
