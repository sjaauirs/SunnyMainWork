using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    /// <summary>
    /// Reads batch file with card create (30) records for all consumers in execution context
    /// </summary>
    /// <param name="etlExecutionContext"></param>
    public interface ICardBatchFileReadService
    {
        Task CardBatchFileReadAsync(EtlExecutionContext etlExecutionContext);
    }
}
