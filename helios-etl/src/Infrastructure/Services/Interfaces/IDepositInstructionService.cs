using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IDepositInstructionService
    {
        /// <summary>
        /// Processes the deposit instruction file based on the provided ETL execution context.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task ProcessDepositInstructionFile(EtlExecutionContext etlExecutionContext);
    }
}
