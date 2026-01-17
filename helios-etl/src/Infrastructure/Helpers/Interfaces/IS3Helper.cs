using Amazon.S3.Model;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces
{
    public interface IS3Helper
    {
        Task UploadFileToS3(Stream streamData, string s3BucketName, string fileName);
        Task UploadCsvFileToS3<T>(List<T> records, string bucketName, string fileName, string delimiter = "\t");
        Task DownloadFileToLocalFolder(string bucketName, string fileKey, string localFolderPath);
        Task<CopyObjectResponse> CopyFileToFolder(string sourceBucketName, string sourceKey, string destinationBucketName, string destinationKey);
        Task MoveFileToFolder(string sourceBucketName, string sourceKey, string destinationBucketName, string destinationKey);
        Task DeleteFileFromBucket(string bucketName, string fileName);
        /// <summary>
        /// Uploads a byte array to an S3 bucket by converting it to a stream. Constructs the full S3 path using the folder and file names.
        /// Uses the existing UploadFileToS3 method for the upload.
        /// </summary>
        /// <param name="fileData">The byte array to upload.</param>
        /// <param name="s3BucketName">The target S3 bucket name.</param>
        /// <param name="folderName">The folder within the S3 bucket.</param>
        /// <param name="fileName">The file name to save as in S3.</param>
        /// <returns>A task representing the async upload operation.</returns>
        Task UploadByteDataToS3(byte[] fileData, string s3BucketName, string folderName, string fileName);
    }
}
