using AutoMapper;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings.MappingProfile
{
    public class TenantMapping : Profile
    {
        public TenantMapping()
        {
            CreateMap<TenantDto, TenantModel>().ReverseMap();
            CreateMap<TenantModel, PostTenantDto>().ReverseMap();
            CreateMap<TenantModel, UpdateTenantDto>().ReverseMap();
            CreateMap<TenantDto, UpdateTenantDto>().ReverseMap();
        }
    }
}
