using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces
{
    public interface IFISFlatFileRecordDtoFactory
    {
        FISFlatFileRecordBaseDto CreateFisRecordInstance(string record);
    }
}
