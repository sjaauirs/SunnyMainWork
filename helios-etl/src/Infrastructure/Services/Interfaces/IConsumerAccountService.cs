using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IConsumerAccountService
    {
        Task<List<ETLConsumerAccountModel>> MergeConsumerAccountAsync(List<ETLConsumerAccountModel> requestDto);
    }
}
