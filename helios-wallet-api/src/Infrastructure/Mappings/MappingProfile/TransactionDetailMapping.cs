using AutoMapper;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings.MappingProfile
{
    public class TransactionDetailMapping :Profile
    {
        public TransactionDetailMapping()
        {
            CreateMap<TransactionDetailDto, TransactionDetailModel>().ReverseMap();
            CreateMap<TransactionDetailModel, GetRecentTransactionResponseDto>().ForMember(x => x.Transactions, c => c.MapFrom(x => new[]
            {
                new TransactionDetailModel
                {
                    TransactionDetailId= x.TransactionDetailId,
                    TransactionDetailType = x.TransactionDetailType,
                    ConsumerCode = x.ConsumerCode,
                    TaskRewardCode = x.TaskRewardCode,
                    Notes = x.Notes,
                    RedemptionRef = x.RedemptionRef,
                    RedemptionItemDescription = x.RedemptionItemDescription,
                    CreateTs = x.CreateTs,
                    //UpdateTs = x.UpdateTs,
                    CreateUser =x.CreateUser,
                    //UpdateUser=x.UpdateUser,
                    DeleteNbr = x.DeleteNbr,
                    RewardDescription = x.RewardDescription
                }
            })).ReverseMap();
        }
    }
}