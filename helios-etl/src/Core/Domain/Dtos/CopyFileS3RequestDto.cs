namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class CopyFileS3RequestDto
    {
        public required string SourceBucketName { get; set; }
        public string? SourceFolderName { get; set; }
        public required string SourceFileName { get; set; }
        public required string TargetBucketName { get; set; }
        public string? TargetFolderName { get; set; }
        public required string TargetFileName { get; set; }

        public string BatchActionName { get; set; }= string.Empty;
        public string? BatchOperationGroupCode { get; set; } = string.Empty;
    }
}
