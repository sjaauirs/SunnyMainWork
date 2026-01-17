extern alias SunnyRewards_Task;

using AutoMapper;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;


namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class TaskRewardMappingProfile : Profile
    {
        public TaskRewardMappingProfile()
        {
            CreateMap<ETLTaskRewardModel, TaskRewardDto>().ReverseMap();
        }
    }
}