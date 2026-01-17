using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IFileCryptoProcessor
    {
        public Task Process(EtlExecutionContext etlExecutionContext);
    }
}
