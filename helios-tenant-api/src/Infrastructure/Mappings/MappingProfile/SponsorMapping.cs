using AutoMapper;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings.MappingProfile
{
    public class SponsorMapping : Profile
    {
        public SponsorMapping()
        {
            CreateMap<SponsorDto, SponsorModel>().ReverseMap();
            CreateMap<CreateSponsorDto, SponsorModel>().ReverseMap();
        }
    }
}
