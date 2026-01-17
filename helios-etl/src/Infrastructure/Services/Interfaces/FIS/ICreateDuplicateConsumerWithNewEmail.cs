using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface ICreateDuplicateConsumerWithNewEmail
    {
        Task CreateDuplicateConsumer(EtlExecutionContext etlExecutionContext);
    }
}
