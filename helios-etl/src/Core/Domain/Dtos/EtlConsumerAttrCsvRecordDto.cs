
namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class EtlConsumerAttrCsvRecordDto
    {
        public string mem_nbr { get; set; } = string.Empty;
        public string group_name { get; set; } = string.Empty;
        public string attr_name { get; set; } = string.Empty;
        public string attr_value { get; set; } = string.Empty;
    }
}
