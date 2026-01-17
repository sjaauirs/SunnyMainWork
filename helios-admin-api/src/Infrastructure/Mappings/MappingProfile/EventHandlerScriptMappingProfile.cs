using AutoMapper;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings.MappingProfile
{
    public class EventHandlerScriptMappingProfile : Profile
    {
        public EventHandlerScriptMappingProfile()
        {
            CreateMap<EventHandlerScriptDto, EventHandlerScriptModel>().ReverseMap();
        }
    }
}
