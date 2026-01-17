using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IS3Service
    {
        /// <summary>
        /// Uploads a file to the specified S3 bucket.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="content">The content of the file.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <returns>A task representing the asynchronous upload operation.</returns>
        System.Threading.Tasks.Task UploadFile(string bucketName, string fileName, string content, string contentType);

        /// <summary>
        /// Retrieves a list of files in the specified S3 folder.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="folderPath">The path of the folder in the S3 bucket.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of file keys.</returns>
        Task<IEnumerable<string>> GetListOfFilesInFolder(string bucketName, string folderPath);

        /// <summary>
        /// Retrieves the content of a file from the specified S3 bucket.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="filePath">The path of the file in the S3 bucket.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the file content as a byte array.</returns>
        Task<byte[]> GetFileContent(string bucketName, string filePath);

        /// <summary>
        /// Zips the contents of the specified S3 folder and uploads the zip file to the S3 bucket.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="s3FolderPrefix">The prefix of the folder in the S3 bucket.</param>
        /// <param name="zipFileName">The name of the zip file to be created and uploaded.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        Task<bool> ZipFolderAndUpload(string bucketName, string s3FolderPrefix, string zipFileName);

        /// <summary>
        /// Deletes the specified folder and its contents from the S3 bucket.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="folderName">The name of the folder to be deleted.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        System.Threading.Tasks.Task DeleteFolder(string bucketName, string folderName);

        /// <summary>
        /// Downloads a zip file from the specified S3 bucket.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="zipFileName">The name of the zip file to be downloaded.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the zip file as a MemoryStream.</returns>
        Task<MemoryStream> DownloadZipFile(string bucketName, string zipFileName);
    }

}
