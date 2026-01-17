using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface IConsumerNonMonetaryTransactionsFileReadService
    {
        Task ImportConsumerNonMonetaryTransactions(EtlExecutionContext etlExecutionContext);
    }
}
