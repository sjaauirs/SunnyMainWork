using AutoMapper;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings.MappingProfile
{
    public class ConsumerWalletDetailsMapping : Profile
    {
        public ConsumerWalletDetailsMapping()
        {
            CreateMap<ConsumerWalletDetailDto, ConsumerWalletDetailsModel>().ReverseMap();

        }
    }
}
