using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TaskRewardCollectionMapping : Profile
    {
        public TaskRewardCollectionMapping()
        {
            CreateMap<TaskRewardCollectionDto, TaskRewardCollectionModel>().ReverseMap();
        }

    }
}
