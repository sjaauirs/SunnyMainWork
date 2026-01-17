using AutoMapper;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class CohortConsumerMappingProfile : Profile
    {
        public CohortConsumerMappingProfile()
        {
            CreateMap<CohortConsumerRequestDto, ETLCohortConsumerModel>().ReverseMap();
        }
    }
}
