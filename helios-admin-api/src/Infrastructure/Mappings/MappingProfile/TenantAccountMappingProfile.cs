using AutoMapper;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings.MappingProfile
{
    public class TenantAccountMappingProfile : Profile
    {
        public TenantAccountMappingProfile()
        {
            CreateMap<TenantAccountRequestDto, PostTenantAccountDto>()
            .ReverseMap();
            CreateMap<TenantAccountRequestDto, TenantAccountDto>()
            .ReverseMap();
            CreateMap<TenantAccountRequestDto, GetTenantAccountDto>()
            .ReverseMap();
            CreateMap<PostTenantAccountDto, GetTenantAccountDto>()
            .ReverseMap();
        }
    }
}
