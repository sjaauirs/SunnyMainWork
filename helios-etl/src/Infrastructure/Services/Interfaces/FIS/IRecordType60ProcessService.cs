using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface IRecordType60ProcessService
    {
        Task Process60RecordFile(string line, FISCardAdditionalDisbursementRecordDto modelObject, EtlExecutionContext etlExecutionContext);
    }
}
