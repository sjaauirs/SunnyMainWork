using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces
{
    public interface IS3FileEncryptionHelper
    {
        Task DecryptAndSaveToLocalPath(string bucketName, string folderName, string fileName, byte[] fisPrivateKey, byte[] sunnyPublicKey, string localFolderPath);

        Task<byte[]> DownloadAndDecryptFile(SecureFileTransferRequestDto requestDto);
    }
}
