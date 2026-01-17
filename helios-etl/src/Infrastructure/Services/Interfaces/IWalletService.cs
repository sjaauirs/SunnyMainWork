using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IWalletService
    {
        /// <summary>
        /// Set entries (secondary) wallet balance=0.0 for all consumers of a given tenant
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task ClearEntriesWallet(string? tenantCode);

        Task RedeemHSA(EtlExecutionContext etlExecutionContext);
        /// <summary>
        /// Generate wallet balances report CSV file with tab delimiter
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task GenerateWalletBalancesReport(EtlExecutionContext etlExecutionContext);
    }
}
