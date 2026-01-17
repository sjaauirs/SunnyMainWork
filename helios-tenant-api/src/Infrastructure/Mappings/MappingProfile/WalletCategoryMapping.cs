using AutoMapper;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings.MappingProfile
{
    public class WalletCategoryMapping : Profile
    {
        public WalletCategoryMapping()
        {
            CreateMap<WalletCategoryModel, WalletCategoryResponseDto>();
        }
    }
}
