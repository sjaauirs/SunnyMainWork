namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class FileLocationDetailsDto
    {
        public string S3BucketName { get; set; }
        public string FileName { get; set; }
        public string FolderName { get; set; }
        public string ConcatFileName { get; set; }

        public FileLocationDetailsDto(string s3BucketName, string fileName, string folderName)
        {
            S3BucketName = s3BucketName;
            FileName = fileName;
            FolderName = folderName;
            ConcatFileName = folderName + fileName;
        }
    }
}
