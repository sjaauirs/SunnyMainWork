using Amazon.S3;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface
{

    public interface IFileHelper
    {
        System.Threading.Tasks.Task UploadFile(AmazonS3Client s3Client, string bucketName, string? folderName, string fileName, Stream inputStream);
    }
}
