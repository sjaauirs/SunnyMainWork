using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IFileExportService
    {
        Task<string> WriteDataToFile(string data, string fileName);
        Task EncryptFileIfRequiredThenUploadToS3Async(EtlExecutionContext context, string inputFilePath, string fileName, bool encrypt);
    }
}
