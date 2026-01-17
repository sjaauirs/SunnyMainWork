using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IETLMemberImportFileDataRepo : IBaseRepo<ETLMemberImportFileDataModel>
    {
        Task<List<ETLMemberImportFileDataModel>> GetBatchedData(long memberImportFileId, int pageSize);

        Task<long> GetBatchedDataCount(long memberImportFileId);
    }
}
