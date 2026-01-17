using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class AdventureMapping : Profile
    {
        public AdventureMapping()
        {
            CreateMap<AdventureDto, AdventureModel>().ReverseMap();
        }
    }
}
