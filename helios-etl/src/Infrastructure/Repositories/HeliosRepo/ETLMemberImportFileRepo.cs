using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class ETLMemberImportFileRepo : BaseRepo<ETLMemberImportFileModel>, IETLMemberImportFileRepo
    {
        public ETLMemberImportFileRepo(ILogger<BaseRepo<ETLMemberImportFileModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
