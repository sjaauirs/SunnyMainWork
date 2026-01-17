using Newtonsoft.Json;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ImportDto
    {
        public MetadataImportJson Metadata { get; set; }
        public TaskImportJson TaskData { get; set; }
        public FisImportJson TenantData { get; set; }
        public CmsImportJson CMSData { get; set; }
        public CohortImportJson CohortData { get; set; }
        public SweepstakesImportJson SweepstakesData { get; set; }

        public TenantImportJson TenantCodeData { get; set; }
        public TaskRewardCollectionImportJson TaskRewardCollectionData { get; set; }
        public AdventureAndTenanImportJson AdventureAndTenantAdventureData { get; set; }
        public AdminImportJson AdminData { get; set; }

        public WalletImportJson WalletData { get; set; }

    }
    public class BaseExportDto
    {
        public string ExportVersion { get; set; }
        public string ExportTs { get; set; }
        public string ExportType { get; set; }
    }
    public class CmsImportJson : BaseExportDto
    {

        public CmsData? Data { get; set; }
    }
    public class CmsData
    {
        public List<ImportComponentDto> Component { get; set; }
    }
    public class ImportComponentDto
    {
        [JsonProperty("component")]
        public ComponentDto? Component { get; set; }

        [JsonProperty("componentTypeCode")]
        public string? ComponentTypeCode { get; set; }
    }
    public class FisImportJson : BaseExportDto
    {

        public TenantAccountData? Data { get; set; }
        
    }
    public class SweepstakesImportJson : BaseExportDto
    {
        [JsonProperty("data")]

        public SweepstakesData? Data { get; set; }
    }
    public class SweepstakesData
    {
        public List<SweepstakesDto> Sweepstakes { get; set; }
        public List<TenantSweepstakesDto> TenantSweepstakes { get; set; }
    }
    public class CohortData
    {
        public List<CohortDto> Cohort { get; set; }
        public List<ImportCohortDto> CohortTenantTaskReward { get; set; }
    }
    public class ImportCohortDto
    {
        [JsonProperty("cohortTenantTaskReward")]
        public CohortTenantTaskRewardDto? CohortTenantTaskReward { get; set; }
        [JsonProperty("taskExternalCode")]
        public string? TaskExternalCode { get; set; }
    }
    public class CohortImportJson : BaseExportDto
    {
        [JsonProperty("data")]

        public CohortData? Data { get; set; }
    }


    public class TenantImportJson : BaseExportDto
    {

        public required TenantData Data { get; set; }
    }

    public class TenantData
    {
        public required TenantDto Tenant { get; set; }
    }
    public class TenantAccountData
    {

        public required GetTenantAccountDto TenantAccount { get; set; }
        [JsonProperty("tenantProgramConfig")]
        public required List<TenantProgramConfigDto> TenantProgramConfig { get; set; }
    }
    public class TaskRewardCollectionImportJson : BaseExportDto
    {
        [JsonProperty("data")]
        public TaskRewardCollectionData Data { get; set; }
    }
    public class TaskRewardCollectionData
    {
        [JsonProperty("taskRewardCollections")]
        public List<ExportTaskRewardCollectionDto> TaskRewardCollections { get; set; }
    }

    public class AdventureAndTenanImportJson : BaseExportDto
    {
        [JsonProperty("data")]
        public AdventureAndTenantAdventureData Data { get; set; }
    }
    public class AdventureAndTenantAdventureData
    {
        [JsonProperty("adventure")]
        public IList<AdventureDto> Adventures { get; set; }
        [JsonProperty("tenantAdventure")]
        public IList<TenantAdventureDto> TenantAdventures { get; set; }
    }

    public class AdminScriptsData
    {
        public List<ScriptDto> Scripts { get; set; } = [];
        public List<EventHandlerScriptDto> EventHandlerScripts { get; set; } = [];
        public List<TenantTaskRewardScriptDto> TenantTaskRewardScripts { get; set; } = [];
    }
    public class AdminImportJson : BaseExportDto
    {
        [JsonProperty("data")]
        public AdminScriptsData Data { get; set; }
    }


    public class WalletImportJson : BaseExportDto
    {
        [JsonProperty("data")]
        public required WalletData Data { get; set; }
    }

    public class WalletData
    {
        [JsonProperty("walletTypeTransferRule")]
        public List<ExportWalletTypeTransferRuleDto> WalletTypeTransferRule { get; set; } = new List<ExportWalletTypeTransferRuleDto>();
    }

    public class MetadataImportJson : BaseExportDto
    {
        [JsonProperty("data")]
        public required MetadataTables Data { get; set; }
    }

    public class MetadataTables
    {
        [JsonProperty("taskType")]
        public List<TaskTypeDto> TaskTypes { get; set; } = new List<TaskTypeDto>();
        [JsonProperty("taskCategory")]
        public List<TaskCategoryDto> TaskCategories { get; set; } = new List<TaskCategoryDto>();
        [JsonProperty("rewardType")]
        public List<TaskRewardTypeDto> RewardTypes { get; set; } = new List<TaskRewardTypeDto>();
        [JsonProperty("walletType")]
        public List<WalletTypeDto> WalletTypes { get; set; } = new List<WalletTypeDto>();
        [JsonProperty("componentType")]
        public List<ComponentTypeDto> ComponentTypes { get; set; } = new List<ComponentTypeDto>();
    }

}