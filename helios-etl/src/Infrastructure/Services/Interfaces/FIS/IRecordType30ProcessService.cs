using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface IRecordType30ProcessService
    {
        Task<ETLConsumerAccountModel> Process30RecordFile(string line, FISCardHolderDataDto modelObject, EtlExecutionContext etlExecutionContext);
        Task<CreateConsumerAccountResponse> CreateConsumerAccount(List<ETLConsumerAccountModel> consumerAccounts);
    }
}
