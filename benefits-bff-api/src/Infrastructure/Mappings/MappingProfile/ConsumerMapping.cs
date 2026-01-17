using AutoMapper;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace Sunny.Benefits.Bff.Infrastructure.Mappings.MappingProfile
{
    [ExcludeFromCodeCoverage]
    public class ConsumerMapping : Profile
    {
        public ConsumerMapping()
        {
            CreateMap<ConsumerDto, ConsumerRequestDto>().ReverseMap();
            CreateMap<ConsumerDto, ConsumerFilter>().ReverseMap();
        }
    }
}
