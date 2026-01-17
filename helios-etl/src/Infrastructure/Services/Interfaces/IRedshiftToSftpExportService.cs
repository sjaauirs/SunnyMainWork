using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IRedshiftToSftpExportService
    {
        Task ExecuteExportAsync(EtlExecutionContext etlExecutionContext);
    }
}
