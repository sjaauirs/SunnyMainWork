using AutoMapper;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class ConsumerMappingProfile : Profile
    {
        public ConsumerMappingProfile()
        {
            CreateMap<ConsumerDto, ETLConsumerModel>()
     .ForMember(dest => dest.ConsumerAttribute, opt => opt.MapFrom(src => src.ConsumerAttribute))
     .ReverseMap()
     .ForMember(dest => dest.ConsumerAttribute, opt => opt.MapFrom(src => src.ConsumerAttribute));
        }
    }
}
