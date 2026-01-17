using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TaskExternalMapping : Profile
    {
        public TaskExternalMapping()
        {
            CreateMap<TaskExternalMappingDto, TaskExternalMappingModel>().ReverseMap();
            CreateMap<TaskExternalMappingRequestDto, TaskExternalMappingModel>().ReverseMap();
            CreateMap<TaskExternalMappingRequestDto, TaskExternalMappingDto>().ReverseMap();
        }
    }
}
