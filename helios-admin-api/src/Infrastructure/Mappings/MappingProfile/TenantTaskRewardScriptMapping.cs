using AutoMapper;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings.MappingProfile
{
    public class TenantTaskRewardScriptMapping : Profile
    {
        public TenantTaskRewardScriptMapping()
        {
            CreateMap<TenantTaskRewardScriptDto, TenantTaskRewardScriptModel>().ReverseMap();
            CreateMap<TenantTaskRewardScriptRequestDto, TenantTaskRewardScriptModel>().ReverseMap();
            CreateMap<UpdateTenantTaskRewardScriptRequestDto, TenantTaskRewardScriptModel>().ReverseMap();
        }
    }

    public class PostTenantMapping : Profile
    {
        public PostTenantMapping()
        {
            CreateMap<TenantDto, PostTenantDto>().ReverseMap();
            CreateMap<TenantDto, UpdateTenantDto>().ReverseMap();
        }
    }
}
