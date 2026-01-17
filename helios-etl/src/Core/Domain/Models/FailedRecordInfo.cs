using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class FailedRecordInfo
    {
        public EtlTaskUpdateCustomFormatRecordDto Record { get; set; }
        public string Reason { get; set; }
    }

}