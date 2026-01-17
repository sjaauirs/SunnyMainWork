extern alias SunnyRewards_Task;

using AutoMapper;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class ConsumerTaskMappingProfile : Profile
    {
        public ConsumerTaskMappingProfile()
        {
            CreateMap<ETLConsumerTaskModel, ConsumerTaskDto>().ReverseMap();
        }
    }
}