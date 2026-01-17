using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface ICardDisbursementFileRecordCreateService
    {
        /// <summary>
        /// Generate 60 record type for each consumer
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="fisPurses"></param>
        /// <param name="fundingRuless"></param>
        /// <returns></returns>
        Task<(List<string>, double)> GenerateDisbursementRecords(EtlExecutionContext etlExecutionContext, List<FISPurseDto> fisPurses,
            bool justInTimeFunding);
    }
}
