using AutoMapper;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings.MappingProfile
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<TaskUpdateRequestDto, TaskDto>().ReverseMap();
        }
    }
}
