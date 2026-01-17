namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class DeleteFileS3RequestDto
    {
        public required string SourceBucketName { get; set; }
        public string? SourceFolderName { get; set; }
        public required string SourceFileName { get; set; }
        public string? BatchOperationGroupCode { get; set; } = string.Empty;
    }
}
