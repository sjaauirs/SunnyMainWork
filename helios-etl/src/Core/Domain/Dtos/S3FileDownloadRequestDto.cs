namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class S3FileDownloadRequestDto
    {
        public required string SourceBucketName { get; set; }
        public string? SourceFolderName { get; set; }
        public required string SourceFileName { get; set; }
        public bool DeleteFileAfterCopy { get; set; } = false;
        public required string ArchiveBucketName { get; set; }
        public string? InboundArchiveFolderName { get; set; }

    }


}
