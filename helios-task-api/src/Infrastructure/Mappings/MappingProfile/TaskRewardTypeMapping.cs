using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TaskRewardTypeMapping : Profile
    {
        public TaskRewardTypeMapping()
        {
            CreateMap<TaskRewardTypeDto, TaskRewardTypeModel>().ReverseMap();
            CreateMap<TaskRewardTypeDto, TaskRewardTypeRequestDto>().ReverseMap();
        }
    }
}
