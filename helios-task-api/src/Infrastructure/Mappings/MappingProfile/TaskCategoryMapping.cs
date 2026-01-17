using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TaskCategoryMapping : Profile
    {
        public TaskCategoryMapping()
        {
            CreateMap<TaskCategoryDto, TaskCategoryModel>().ReverseMap();
        }
    }
}
