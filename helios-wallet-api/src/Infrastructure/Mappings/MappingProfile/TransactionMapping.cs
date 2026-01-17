using AutoMapper;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings.MappingProfile
{
    public class TransactionMapping : Profile
    {
        public TransactionMapping()
        {
            CreateMap<TransactionDto, TransactionModel>().ReverseMap();
            CreateMap<TransactionModel, GetRecentTransactionResponseDto>()
             .ForMember(x => x.Transactions, c => c.MapFrom(x => new[]
             {
                new TransactionModel
                {
                    TransactionId= x.TransactionId,
                    WalletId = x.WalletId,
                    TransactionCode = x.TransactionCode,
                    TransactionType = x.TransactionType,
                    PreviousBalance = x.PreviousBalance,
                    TransactionAmount = x.TransactionAmount,
                    Balance = x.Balance,
                    PrevWalletTxnCode = x.PrevWalletTxnCode,
                    CreateTs = x.CreateTs,
                    CreateUser =x.CreateUser,
                    DeleteNbr = x.DeleteNbr,
                    TransactionDetailId = x.TransactionDetailId
                }
             })).ReverseMap();
            CreateMap<TransactionModel, ConsumerTransactionsResponseDto>()
             .ForMember(x => x.Transactions, c => c.MapFrom(x => new[]
             {
                new List<TransactionModel>()
                {
                    new TransactionModel
                    {
                    TransactionId= x.TransactionId,
                    WalletId = x.WalletId,
                    TransactionCode = x.TransactionCode,
                    TransactionType = x.TransactionType,
                    PreviousBalance = x.PreviousBalance,
                    TransactionAmount = x.TransactionAmount,
                    Balance = x.Balance,
                    PrevWalletTxnCode = x.PrevWalletTxnCode,
                    CreateTs = x.CreateTs,
                    CreateUser =x.CreateUser,
                    DeleteNbr = x.DeleteNbr,
                    TransactionDetailId = x.TransactionDetailId
                    }
                }
             })).ReverseMap();
        }
    }
}
