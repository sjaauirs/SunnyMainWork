using AutoMapper;
using Newtonsoft.Json;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TaskRewardMapping : Profile
    {
        public TaskRewardMapping()
        {
            CreateMap<TaskRewardDto, TaskRewardModel>()
                .ReverseMap().ForMember(src => src.RewardDetails, opt => opt.MapFrom(des => TaskRewardDto.GetRewardDetails(des.Reward??string.Empty))); ;
            CreateMap<TaskRewardModel, FindTaskRewardResponseDto>()
           .ForMember(x => x.TaskRewardDetails, c => c.MapFrom(x => new[]
           {
                new TaskRewardModel()
           })).ReverseMap();
            CreateMap<TaskRewardDto, TaskRewardDto>().ReverseMap();
            CreateMap<TaskAndTaskRewardDto, TaskAndTaskRewardModel>().ReverseMap();
            CreateMap<TaskRewardDetailsDto, TaskRewardDetailModel>().ReverseMap();
            CreateMap<TaskRewardDto, TaskRewardRequestDto>().ReverseMap();
            CreateMap<ImportTaskRewardDto, TaskRewardDto>().ReverseMap();
        }
    }
}

