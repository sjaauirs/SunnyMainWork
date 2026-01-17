using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TaskMapping : Profile
    {
        public TaskMapping()
        {
            CreateMap<TaskDto, TaskModel>().ReverseMap();
            CreateMap<TaskModel, FindTaskRewardResponseDto>()
            .ForMember(x => x.TaskRewardDetails, c => c.MapFrom(x => new[]
            {
                new TaskModel()
            })).ReverseMap();
            CreateMap<TaskModel, CreateTaskRequestDto>().ReverseMap();
            CreateMap<TaskDto, CreateTaskRequestDto>().ReverseMap();
            CreateMap<TaskModel, TaskRequestDto>().ReverseMap();
            CreateMap<TaskDto, TaskRequestDto>().ReverseMap();
        }
    }
}

