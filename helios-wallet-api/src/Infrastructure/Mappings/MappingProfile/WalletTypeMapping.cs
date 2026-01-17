using AutoMapper;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings.MappingProfile
{
    public class WalletTypeMapping : Profile
    {
        public WalletTypeMapping()
        {
            CreateMap<WalletTypeDto, WalletTypeModel>().ReverseMap();
        }
    }
}
