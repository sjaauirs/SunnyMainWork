using AutoMapper;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class ConsumerAccountHistoryMappingProfile : Profile
    {
        public ConsumerAccountHistoryMappingProfile()
        {
            CreateMap<ETLConsumerAccountModel, ETLConsumerAccountHistoryModel>().ReverseMap();
        }
    }
}
