using AutoMapper;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings.MappingProfile
{
    public class ConsumerHistoryMapping : Profile
    {
        public ConsumerHistoryMapping()
        {
            CreateMap<ConsumerModel, ConsumerHistoryModel>().ReverseMap();
            CreateMap<ConsumerDto, ConsumerHistoryModel>().ReverseMap();
        }
    }
}