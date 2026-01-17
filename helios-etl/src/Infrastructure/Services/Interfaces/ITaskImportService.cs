using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ITaskImportService
    {
        /// <summary>
        /// ImportTaskAsync
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task<EtlTaskImportFileResponseDto> ImportTaskAsync(EtlExecutionContext etlExecutionContext);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskDefinitionfilePath"></param>
        Task Import(string tenantCode,  string taskDefinitionfilePath = "");
    }
}
