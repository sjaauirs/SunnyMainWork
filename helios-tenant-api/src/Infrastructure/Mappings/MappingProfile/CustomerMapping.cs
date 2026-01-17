using AutoMapper;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings.MappingProfile
{
    public class CustomerMapping : Profile
    {
        public CustomerMapping()
        {
            CreateMap<CustomerDto, CustomerModel>().ReverseMap();
            CreateMap<CreateCustomerDto, CustomerModel>().ReverseMap();

        }
    }
}
