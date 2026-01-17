namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class EtlTaskImportFileResponseDto : JobImportFileBaseResponse
    {
        public string FileName { get; set; } = string.Empty;
        public IList<EtlTaskImportDto>? ImportedTaskRecords { get; set; }
    }
    
    public class JobImportFileBaseResponse
    {
        public int TotalRecordsReceived { get; set; }
        public int TotalRecordsProcessed { get; set; }
        public int TotalSuccessfulRecords { get; set; }
        public int TotalFailedRecords { get; set; }
    }
}
