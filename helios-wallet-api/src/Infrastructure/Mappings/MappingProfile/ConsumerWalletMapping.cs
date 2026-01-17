using AutoMapper;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings.MappingProfile
{
    public class ConsumerWalletMapping : Profile
    {
        public ConsumerWalletMapping()
        {
            CreateMap<ConsumerWalletDto, ConsumerWalletModel>().ReverseMap();
            CreateMap<ConsumerWalletModel, FindConsumerWalletResponseDto>()
            .ForMember(x => x.ConsumerWallets, c => c.MapFrom(x => new[]
            {
                new ConsumerWalletModel
                {
                   ConsumerWalletId = x.ConsumerWalletId,
                   WalletId = x.WalletId,
                   TenantCode = x.TenantCode,
                   ConsumerCode = x.ConsumerCode,
                   ConsumerRole = x.ConsumerRole,
                   EarnMaximum = x.EarnMaximum,
                   CreateTs = x.CreateTs,
                   UpdateTs = x.UpdateTs,
                   CreateUser =x.CreateUser,
                   UpdateUser=x.UpdateUser,
                   DeleteNbr = x.DeleteNbr,
                   Xmin = x.Xmin,
                }
            })).ReverseMap();
        }
    }
}
