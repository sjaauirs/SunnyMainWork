namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLFundingHistoryModel
    {
        public virtual long FundingHistoryId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual int FundRuleNumber { get; set; }
        public virtual DateTime FundTs { get; set; }
        public virtual DateTime CreateTs { get; set; }
        public virtual string? CreateUser { get; set; }
        public virtual long DeleteNbr { get; set; }
    }
}
