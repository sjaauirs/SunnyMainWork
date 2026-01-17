namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class SecureFileTransferRequestDto
    {
        public string? TenantCode { get; set; }
        public required string SourceBucketName { get; set; }
        public string? SourceFolderName { get; set; }
        public required string SourceFileName { get; set; }
        public required string TargetBucketName { get; set; }
        public string? TargetFolderName { get; set; }
        public required string TargetFileName { get; set; }
        public required string FisAplPublicKeyName { get; set; }
        public required string SunnyAplPublicKeyName { get; set; }
        public required string SunnyAplPrivateKeyName { get; set; }
        public bool DeleteFileAfterCopy { get; set; } = false;
        public bool UploadToArchiveAfterCopy { get; set; } = false;
        public string? PassPhraseKeyName { get; set; }
        public string? ArchiveBucketName { get; set; }
        public string? InboundArchiveFolderName { get; set; }
        public string? OutboundArchiveFolderName { get; set; }
        public string? BatchOperationGroupCode {  get; set; } = string.Empty;
    }
}
