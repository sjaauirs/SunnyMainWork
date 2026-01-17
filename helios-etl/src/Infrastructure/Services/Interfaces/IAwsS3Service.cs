using Amazon.S3;
using Amazon.S3.Model;
using CsvHelper.Configuration;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IAwsS3Service
    {
        Task AppendInFile(string keyName, string contentJson);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        Task<byte[]> GetFileFromAwsS3(string keyName, string? bucketName = null);

        /// <summary>
        /// It will move file to destination and delete file form source
        /// </summary>
        /// <param name="sourceKey">e.g incoming/pld.txt</param>
        /// <param name="destinationKey">e.g processing/pld.txt</param>
        /// <returns></returns>
        Task MoveFileInAwsS3(string sourceKey, string destinationKey, string? sourceBucket = null, string? destinationBucket = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        Task<List<string>> GetAllFileNames(string folderName, string? bucketName = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<bool> CreateFile(string fileName);

        /// <summary>
        /// Move file form processing to archive folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task MoveFileFromProcessingToArchive(string fileName);

        /// <summary>
        /// Read consumer codes from S3/local file
        /// </summary>
        /// <param name="consumerListFile"></param>
        /// <returns></returns>
        Task<List<string>> GetConsumerListFromFile(string consumerListFile);

        Task<List<object>?> GetConsumerListFromFileForCard60(string consumerListFile);

        Task<bool> CreateCsvAndUploadToS3<T>(CsvConfiguration csvConfig, List<T> records, string fileName, string bucketName);

        Task<Stream> DownloadFile(string bucketName, string fileKey);

        Task UploadStreamToS3(Stream inputStream, string bucketName, string key);

        Task DeleteFile(string bucketName, string key);

        Task<GetObjectMetadataResponse> GetFileMetadata(string bucketName, string key);

        Task UploadFileToS3(string filePath, string bucketName, string key);

        /// <summary>
        /// Reads cohort codes from a file, which can be either in S3 or local storage.
        /// </summary>
        /// <param name="cohortListFile"></param>
        /// <returns></returns>
        Task<List<string>> GetCohortListFromFile(string cohortListFile);
        Task UploadImageToS3Async(byte[] imageBytes, string bucketName, string key);
    }
}
