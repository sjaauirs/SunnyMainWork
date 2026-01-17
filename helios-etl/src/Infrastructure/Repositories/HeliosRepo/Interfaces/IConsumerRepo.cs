using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IConsumerRepo : IBaseRepo<ETLConsumerModel>
    {

        /// <summary>
        /// Retrieves a list of consumers and wallets for given tenant and wallet type.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="walletTypeCode"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IQueryable<ETLConsumerAndWalletModel> GetConsumersAndWalletsByWalletTypeId(string? tenantCode, long walletTypeId, int skip, int take, List<string>? consumerCodesList = null);

        IQueryable<ETLConsumerWalletAggregate> GetConsumersAndWalletsByWalletTypeIdByCutOffDate(
      string? tenantCode, long walletTypeId, int skip, int take, DateTime cutoffDate, List<string>? consumerCodesList = null);
        IQueryable<ETLConsumerAndConsumerWalleModel> GetConsumersWalletsByWalletTypeId(string? tenantCode, long walletTypeId, int skip, int take, List<string>? consumerCodesList = null);

        /// <summary>
        /// Retrieves a list of consumers for given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        IQueryable<ETLConsumerModel> GetConsumers(string? tenantCode);

        /// <summary>
        /// Retrieves a batch of consumers for the given tenant.
        /// </summary>
        /// <param name="tenantCode">The tenant code to filter consumers, or null to retrieve all consumers.</param>
        /// <param name="start">The starting index for batching.</param>
        /// <param name="batchSize">The number of records to retrieve in each batch.</param>
        /// <returns>An IQueryable collection of ETLConsumerModel representing the requested batch.</returns>
        IQueryable<ETLConsumerModel> GetConsumers(string? tenantCode, int start, int batchSize);

        /// <summary>
        /// Retrieves a list of consumers and persons for given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IQueryable<ETLConsumerAndPersonModel> GetConsumersAndPersonsByTenantCode(string? tenantCode, int skip, int take, List<string>? consumerCodesList = null);

        /// <summary>
        /// GetNonSyntheticConsumer
        /// </summary>
        /// <param name="anonymousCode"></param>
        /// <returns></returns>
        Task<ETLConsumerModel> GetNonSyntheticConsumer(string anonymousCode);
        Task<List<MemberInsurancePeriodDto>> GetInsurancePeriod(List<MemberGroupByKey> mem_nbrs);
        Task<List<ETLConsumerModel>> UpdateInsurancePeriodsAsync(List<MemberInsurancePeriodDto> periodDtos);
    }
}

