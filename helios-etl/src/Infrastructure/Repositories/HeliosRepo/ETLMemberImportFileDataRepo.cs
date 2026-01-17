using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class ETLMemberImportFileDataRepo : BaseRepo<ETLMemberImportFileDataModel>, IETLMemberImportFileDataRepo
    {
        private readonly NHibernate.ISession _session;
        public ETLMemberImportFileDataRepo(ILogger<BaseRepo<ETLMemberImportFileDataModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public async Task<List<ETLMemberImportFileDataModel>> GetBatchedData(long memberImportFileId, int pageSize)
        {
            var query = from c in _session.Query<ETLMemberImportFileDataModel>()
                        where c.MemberImportFileId == memberImportFileId && c.DeleteNbr == 0 && c.RecordProcessingStatus == 0
                        orderby c.MemberImportFileDataId
                        select c;

            return await query.Take(pageSize).ToListAsync();
        }

        public async Task<long> GetBatchedDataCount(long memberImportFileId)
        {
            return await _session.Query<ETLMemberImportFileDataModel>()
               .Where(c => c.MemberImportFileId == memberImportFileId && c.DeleteNbr == 0)
               .LongCountAsync();
        }

    }
}
