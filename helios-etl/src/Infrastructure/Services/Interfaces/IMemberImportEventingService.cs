using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IMemberImportEventingService
    {
        Task MemberImportEventingAsync(EtlExecutionContext etlExecutionContext, string jobId);
    }
}
