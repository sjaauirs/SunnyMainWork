using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TenantTaskCategoryMapping : Profile
    {
        public TenantTaskCategoryMapping()
        {
            CreateMap<TenantTaskCategoryDto, TenantTaskCategoryModel>().ReverseMap();
            CreateMap<TenantTaskCategoryRequestDto, TenantTaskCategoryModel>().ReverseMap();
            CreateMap<TenantTaskCategoryRequestDto, TenantTaskCategoryDto>().ReverseMap();
        }
    }
}
