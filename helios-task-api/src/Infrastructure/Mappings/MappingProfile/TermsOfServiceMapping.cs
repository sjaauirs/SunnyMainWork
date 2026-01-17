using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TermsOfServiceMapping : Profile
    {
        public TermsOfServiceMapping()
        {
            CreateMap<TermsOfServiceDto, TermsOfServiceModel>().ReverseMap();
            CreateMap<CreateTermsOfServiceRequestDto, TermsOfServiceModel>().ReverseMap();
            CreateMap<TermsOfServiceModel, FindTaskRewardResponseDto>()
        .ForMember(x => x.TaskRewardDetails, c => c.MapFrom(x => new[]
       {
                new TermsOfServiceModel()
       })).ReverseMap();
        }
    }
}
