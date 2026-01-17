using AutoMapper;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings.MappingProfile
{
    public class WalletMapping : Profile
    {
        public WalletMapping()
        {
            CreateMap<WalletDto, WalletModel>().ReverseMap();
            CreateMap<WalletRequestDto, WalletModel>().ReverseMap();
        }
    }
}
