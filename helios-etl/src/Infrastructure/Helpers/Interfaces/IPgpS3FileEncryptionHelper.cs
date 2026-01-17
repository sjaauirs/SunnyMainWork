using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces
{
    public interface IPgpS3FileEncryptionHelper
    {
        Task EncryptGeneratedFile(string batchOperationGroupCode, string tenantCode, string targetBucket = "", string targetFolder = "", string targetFileName = "");
        Task CopyFileToS3Destination(string batchOperationGroupCode, string destinationBucket, string destinationFolder);
        Task ArchiveFile(string batchOperationGroupCode, string archiveBucketName, string archiveDestination);
        Task DeleteFile(string batchOperationGroupCode);
        Task DecryptAndSaveToLocalPath(string bucketName, string folderName, string fileName, string fisPrivateKey, string passPhrase, string localFolderPath);
        Task<byte[]> DownloadAndDecryptFile(SecureFileTransferRequestDto requestDto);
        Task<LatestFileResult> DownloadLatestFileByName(S3FileDownloadRequestDto requestDto);

        void DecryptFile(Stream inputStream, Stream outputStream, Stream privateKeyStream, char[] passPhrase);

        void EncryptFile(Stream inputFileStream, Stream outputStream, Stream publicKeyStream);
    }
}
