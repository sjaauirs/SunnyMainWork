using AutoMapper;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings.MappingProfile
{
    public class PersonAddressMapping : Profile
    {
        public PersonAddressMapping()
        {
            CreateMap<PersonAddressModel, PersonAddressDto>().ReverseMap();
            CreateMap<PersonAddressModel, CreatePersonAddressRequestDto>().ReverseMap();
            CreateMap<PersonAddressModel, UpdatePersonAddressRequestDto>().ReverseMap();
        }
    }
}
