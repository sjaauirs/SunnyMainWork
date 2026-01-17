using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json
{
    public class TenantConfig
    {

        [JsonProperty("purseConfig")]
        public PurseConfig? PurseConfig { get; set; }

        [JsonProperty("fisProgramDetail")]
        public FisProgramDetail? FISProgramDetail { get; set; }
    }
    public class PurseConfig
    {
        [JsonProperty("purses")]
        public List<Purse>? Purses { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class FisProgramDetail
    {
        [JsonProperty("clientId")]
        public string? ClientId { get; set; }

        [JsonProperty("companyId")]
        public string? CompanyId { get; set; }

        [JsonProperty("packageId")]
        public string? PackageId { get; set; }

        [JsonProperty("subprogramId")]
        public string? SubprogramId { get; set; }  
    }

    public class Purse
    {
        [JsonProperty("purseLabel")]
        public string? PurseLabel { get; set; }

        [JsonProperty("walletType")]
        public string? WalletType { get; set; }

        [JsonProperty("purseNumber")]
        public int PurseNumber { get; set; }
        [JsonProperty("periodConfig")]
        public PeriodConfig? PeriodConfig { get; set; }

        [JsonProperty("purseWalletType")]
        public string? PurseWalletType { get; set; }

        [JsonProperty("masterWalletType")]
        public string? MasterWalletType { get; set; }

        [JsonProperty("masterRedemptionWalletType")]
        public string? MasterRedemptionWalletType { get; set; }

        [JsonProperty("activeStartTs")]
        public DateTime? ActiveStartTs { get; set; }

        [JsonProperty("activeEndTs")]
        public DateTime? ActiveEndTs { get; set; }

        [JsonProperty("redeemEndTs")]
        public DateTime? RedeemEndTs { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; } = 0;

    }
    public class PeriodConfig
    {
        [JsonProperty("fundDate")]
        public int FundDate { get; set; }

        [JsonProperty("interval")]
        public string? Interval { get; set; }

        [JsonProperty("applyDate")]
        public ApplyDateConfig? ApplyDateConfig { get; set; }
    }
    public class ApplyDateConfig
    {
        [JsonProperty("applyDate")]
        public int ApplyDate { get; set; }

        [JsonProperty("applyDateType")]
        public string? ApplyDateType { get; set; }

    }
}
