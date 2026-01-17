using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class ConsumerTaskMapping : Profile
    {
        public ConsumerTaskMapping()
        {
            CreateMap<ConsumerTaskDto, ConsumerTaskModel>().ReverseMap();
            CreateMap<ConsumerTaskModel, FindConsumerTaskResponseDto>()
            .ForMember(x => x.ConsumerTask, c => c.MapFrom(x => new[]
            {
                new ConsumerTaskModel()
            })).ReverseMap();
            CreateMap<ConsumerTaskModel, FindConsumerTaskResponseDto>()
           .ForMember(x => x.TaskRewardDetail, c => c.MapFrom(x => new[]
           {
                new ConsumerTaskModel()
           })).ReverseMap();
        }
    }
}
