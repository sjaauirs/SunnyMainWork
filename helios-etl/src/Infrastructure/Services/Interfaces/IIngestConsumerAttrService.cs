using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IIngestConsumerAttrService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerAttrFileContent"></param>
        /// <returns></returns>
        Task<List<ETLConsumerModel>> Ingest(string tenantCode, byte[] consumerAttrFileContent);
    }
}
