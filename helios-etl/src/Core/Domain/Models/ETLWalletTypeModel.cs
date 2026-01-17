using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLWalletTypeModel : BaseModel
    {
        public virtual long WalletTypeId { get; set; }
        public virtual string? WalletTypeCode { get; set; }
        public virtual string? WalletTypeName { get; set; }
        public virtual string? WalletTypeLabel { get;}
        public virtual string? ShortLabel { get; set; }
        public virtual bool IsExternalSync { get; set; }
    }
}
