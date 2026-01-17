using AutoMapper;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings.MappingProfile
{
    public class ConsumerActivityMapping : Profile
    {
        public ConsumerActivityMapping()
        {
            CreateMap<ConsumerActivityDto, ConsumerActivityModel>().ReverseMap();
        }
    }
}
