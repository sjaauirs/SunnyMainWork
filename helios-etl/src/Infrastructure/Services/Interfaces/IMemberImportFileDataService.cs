using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IMemberImportFileDataService
    {/// <summary>
     /// model to store validation json
     /// </summary>
        BatchJobReportValidationJson batchJobReportValidationJson { get; }
        /// <summary>
        /// model to store consumer current record counts per tenant
        /// </summary>
        PerTenantConsumerCount perTenantConsumerCountData { get; }
        /// <summary>
        /// save member import data
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task<(long, bool)> saveMemberImportFileData(EtlExecutionContext etlExecutionContext);
        /// <summary>
        /// get member import data based on id
        /// </summary>
        /// <param name="memberImportFileId"></param>
        /// <returns></returns>
        Task<List<ETLMemberImportFileDataModel>> GetMemberImportFileDataRecords(long memberImportFileId, int take);
        /// <summary>
        /// get consumer count per tenant
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task GetCounsumerCount(string tenantCode);
        /// <summary>
        /// Add tenant to list for Per tenant record
        /// </summary>
        /// <param name="tenantcode"></param>
        /// <returns></returns>
        Task AddtenantforPostRun(MemberEnrollmentDetailDto[] memberEnrollmentDetailDtos, string actionType);

        /// <summary>
        /// Updates the success count
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="tenantcode"></param>
        /// <returns></returns>
        Task UpdateSuccessCount(string actionType, string tenantcode);
        Task GetCounsumerPostCount(string tenantcode, DateTime UpdatedTs);
        Task VerifyCounsumerCount(string tenantcode);
        Task<string> CreateAndUploadCsv( string fileName);
        Task<IList<ETLMemberImportFileModel>> GetMemberImportFilesToImport();
        Task<bool> updateFileStatus(long memberImportFileId, FileStatus status);


        MemberImportCSVDto ConvertToConsumerCsvDto(ETLMemberImportFileDataModel memberImportFileData);
        Task<long> GetBatchedDataCount(long memberImportFileId);

        Task UpdateMemberImportFileDataRecordProcessingStatus(long memberImportFileDataId, long recordProcessingStatus);
    }
}
