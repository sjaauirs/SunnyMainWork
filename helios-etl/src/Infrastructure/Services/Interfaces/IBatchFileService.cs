using NHibernate;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IBatchFileService
    {
        /// <summary>
        /// Save Batch file details when processing start
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="fileType"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        Task<ETLBatchFileModel> SaveBatchFileRecord(BatchFileDirection direction, ScanS3FileType fileType,string filename);

        /// <summary>
        /// Update process end time for batch file processing
        /// </summary>
        /// <param name="batchFile"></param>
        /// <returns></returns>
        Task<ETLBatchFileModel> UpdateProcessEndTime(ETLBatchFileModel BatchFileId);
    }
}
