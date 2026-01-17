using AutoMapper;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;


namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class PostEventMappingProfile : Profile
    {
        public PostEventMappingProfile()
        {
            CreateMap<PostEventRequestDto, PostEventRequestModel>()
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventSubtype, opt => opt.MapFrom(src => src.EventSubtype))
            .ForMember(dest => dest.EventSource, opt => opt.MapFrom(src => src.EventSource))
            .ForMember(dest => dest.TenantCode, opt => opt.MapFrom(src => src.TenantCode))
            .ForMember(dest => dest.ConsumerCode, opt => opt.MapFrom(src => src.ConsumerCode))
            .ForMember(dest => dest.EventData, opt => opt.MapFrom(src => src.EventData))
            .ReverseMap();
        }
    }
}
