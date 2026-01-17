using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IMemberImportService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> Import(EtlExecutionContext etlExecutionContext);

        Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> ProcessBatchAsync(List<MemberImportCSVDto> memberCsvDtoList, EtlExecutionContext etlExecutionContext);

    }
}
