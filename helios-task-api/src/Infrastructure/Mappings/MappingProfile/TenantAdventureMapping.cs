using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TenantAdventureMapping : Profile
    {
        public TenantAdventureMapping()
        {
            CreateMap<TenantAdventureDto, TenantAdventureModel>().ReverseMap();
        }
    }
}
