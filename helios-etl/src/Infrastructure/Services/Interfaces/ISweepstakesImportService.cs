using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ISweepstakesImportService
    {
        /// <summary>
        /// GenerateSweepstakesEntriesReport
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task<List<SweepstakesWalletBalancesReportDto>> GenerateSweepstakesEntriesReport(EtlExecutionContext etlExecutionContext);
    }
}
