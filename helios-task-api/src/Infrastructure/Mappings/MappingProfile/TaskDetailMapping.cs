using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TaskDetailMapping : Profile
    {
        public TaskDetailMapping()
        {
            CreateMap<TaskDetailDto, TaskDetailModel>().ReverseMap();
            CreateMap<TaskDetailDto, TaskDetailRequestDto>().ReverseMap();
            CreateMap<TaskDetailModel, FindTaskRewardResponseDto>()
            .ForMember(x => x.TaskRewardDetails, c => c.MapFrom(x => new[]
            {
                new TaskDetailModel()
            })).ReverseMap();
            CreateMap<PostTaskDetailsDto, TaskDetailModel>().ReverseMap();
            CreateMap<PostTaskDetailsDto, TaskDetailDto>().ReverseMap();
        }
    }
}

