using AutoMapper;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings.MappingProfile
{
    public class PhoneNumberMapping : Profile
    {
        public PhoneNumberMapping()
        {
            CreateMap<PhoneNumberDto, PhoneNumberModel>().ReverseMap();
            CreateMap<PhoneNumberModel, CreatePhoneNumberRequestDto>().ReverseMap();
            CreateMap<PhoneNumberModel, UpdatePhoneNumberRequestDto>().ReverseMap();
        }
    }
}
