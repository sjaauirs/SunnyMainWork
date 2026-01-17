using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IBatchOperationService
    {
        Task SaveBatchOperationGenerateRecord(string batchOperationGroupCode, string storageName, string? folderName, string filename);
        Task SaveBatchOperation(string batchOperationGroupCode, BatchActionDtoBase batchAction);
        Task<IList<EtlBatchOperationModel>> GetBatchOperationsRecords(string batchOperationGroupCode , List<BatchActions> actions);
        Task ValidateTenant(string tenantCode);

        void ValidateBatchOperationGroupCode(EtlExecutionContext excontext);
    }
}
