using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ISftpUploader
    {
        Task UploadFile(EtlExecutionContext context, string localFilePath);
    }
}
