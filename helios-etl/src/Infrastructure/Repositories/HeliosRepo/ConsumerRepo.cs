using FluentNHibernate.Utils;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class ConsumerRepo : BaseRepo<ETLConsumerModel>, IConsumerRepo
    {
        private readonly ILogger<BaseRepo<ETLConsumerModel>> _logger;
        private readonly NHibernate.ISession _session;
        private const string _className = nameof(ConsumerRepo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        public ConsumerRepo(ILogger<BaseRepo<ETLConsumerModel>> logger, NHibernate.ISession session) : base(logger, session)
        {
            _logger = logger;
            _session = session;
        }

        /// <summary>
        /// Retrieves a list of consumers and wallets for given tenant and wallet type.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="walletTypeId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public IQueryable<ETLConsumerAndWalletModel> GetConsumersAndWalletsByWalletTypeId(string? tenantCode, long walletTypeId,
            int skip, int take, List<string>? consumerCodesList = null)
        {
            var query = from wt in _session.Query<ETLWalletModel>()
                        join cwt in _session.Query<ETLConsumerWalletModel>() on wt.WalletId equals cwt.WalletId
                        join cmr in _session.Query<ETLConsumerModel>() on cwt.ConsumerCode equals cmr.ConsumerCode
                        join pn in _session.Query<ETLPersonModel>() on cmr.PersonId equals pn.PersonId
                        where wt.WalletTypeId == walletTypeId &&
                              wt.TenantCode == tenantCode &&
                              wt.DeleteNbr == 0 &&
                              cwt.TenantCode == tenantCode &&
                              cwt.DeleteNbr == 0 &&
                              cmr.TenantCode == tenantCode &&
                              cmr.DeleteNbr == 0 &&
                              pn.DeleteNbr == 0 &&
                              pn.SyntheticUser == false
                        // wt.Balance > 0 
                        orderby cmr.ConsumerId
                        select new ETLConsumerAndWalletModel()
                        {
                            Consumer = cmr,
                            Wallet = wt
                        };

            //filter to get only consumers from consumerCodesList 
            if (consumerCodesList != null && consumerCodesList.Count > 0)
            {
                query = from cw in query
                        where consumerCodesList.Contains(cw.Consumer.ConsumerCode)
                        select cw;
            }

            return query.Skip(skip).Take(take);
        }
        public IQueryable<ETLConsumerWalletAggregate> GetConsumersAndWalletsByWalletTypeIdByCutOffDate(
     string? tenantCode,long walletTypeId,int skip, int take,DateTime cutoffDate, List<string>? consumerCodesList = null)
        {
            var query =
                from wt in _session.Query<ETLWalletModel>()
                join cwt in _session.Query<ETLConsumerWalletModel>()
                    on wt.WalletId equals cwt.WalletId
                join cmr in _session.Query<ETLConsumerModel>()
                    on cwt.ConsumerCode equals cmr.ConsumerCode
                join pn in _session.Query<ETLPersonModel>()
                    on cmr.PersonId equals pn.PersonId
                join ca in _session.Query<ETLConsumerAccountModel>()
                on cmr.ConsumerCode equals ca.ConsumerCode into caLeft
                from ca in caLeft.DefaultIfEmpty()
                where
                    wt.WalletTypeId == walletTypeId &&
                    wt.TenantCode == tenantCode &&
                    wt.ActiveStartTs <= cutoffDate &&
                    wt.ActiveEndTs >= cutoffDate &&
                    wt.DeleteNbr == 0 &&
                    cwt.TenantCode == tenantCode &&
                    cwt.DeleteNbr == 0 &&
                    cmr.TenantCode == tenantCode &&
                    cmr.DeleteNbr == 0 &&
                    pn.DeleteNbr == 0 &&
                    pn.SyntheticUser == false
                orderby cmr.ConsumerId
                select new ETLConsumerWalletAggregate
                {
                    Consumer = cmr,
                    Wallet = wt,
                    Person = pn,
                    ConsumerAccount = ca
                };

            if (consumerCodesList?.Any() == true)
            {
                query = query.Where(x =>
                    consumerCodesList.Contains(x.Consumer.ConsumerCode));
            }

            return query.Skip(skip).Take(take);
        }


        /// <summary>
        /// Retrieves a list of consumers for given tenant.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public IQueryable<ETLConsumerModel> GetConsumers(string? tenantCode)
        {
            const string methodName = nameof(GetConsumers);

            try
            {
                _logger.LogInformation($"[{_className}] : [{methodName}] : Start processing {methodName}");

                IQueryable<ETLConsumerModel>? consumers = null;
                if (!string.IsNullOrEmpty(tenantCode))
                {
                    consumers = from cmr in _session.Query<ETLConsumerModel>()
                                where cmr.TenantCode == tenantCode && cmr.DeleteNbr == 0
                                select new ETLConsumerModel()
                                {
                                    ConsumerCode = cmr.ConsumerCode,
                                };
                }
                else
                {
                    consumers = from cmr in _session.Query<ETLConsumerModel>()
                                where cmr.DeleteNbr == 0
                                select new ETLConsumerModel()
                                {
                                    ConsumerCode = cmr.ConsumerCode,
                                    TenantCode = cmr.TenantCode
                                };

                }

                return consumers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_className}] : [{methodName}] : Error processing {methodName}, request: {tenantCode}", _className, methodName);
                throw;
            }
            finally
            {
                _logger.LogInformation($"[{_className}] : [{methodName}] : Completed processing {methodName}");
            }
        }

        /// <summary>
        /// Retrieves a batch of consumers for the given tenant.
        /// </summary>
        /// <param name="tenantCode">The tenant code to filter consumers, or null to retrieve all consumers.</param>
        /// <param name="start">The starting index for batching.</param>
        /// <param name="batchSize">The number of records to retrieve in each batch.</param>
        /// <returns>An IQueryable collection of ETLConsumerModel representing the requested batch.</returns>
        public IQueryable<ETLConsumerModel> GetConsumers(string? tenantCode, int start, int batchSize)
        {
            const string methodName = nameof(GetConsumers);

            try
            {
                var query = _session.Query<ETLConsumerModel>().Where(cmr => cmr.DeleteNbr == 0);

                if (!string.IsNullOrEmpty(tenantCode))
                {
                    query = query.Where(cmr => cmr.TenantCode == tenantCode)
                                 .Select(cmr => new ETLConsumerModel
                                 {
                                     ConsumerCode = cmr.ConsumerCode,
                                     TenantCode = cmr.TenantCode
                                 });
                }
                else
                {
                    query = query.Select(cmr => new ETLConsumerModel
                    {
                        ConsumerCode = cmr.ConsumerCode,
                        TenantCode = cmr.TenantCode
                    });
                }

                return query.Skip(start).Take(batchSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_className}] : [{methodName}] : Error processing {methodName} for TenantCode: {tenantCode}, Start: {start}, BatchSize: {batchSize}");
                throw;
            }
        }


        /// <summary>
        /// Retrieves a list of consumers and persons for given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public IQueryable<ETLConsumerAndPersonModel> GetConsumersAndPersonsByTenantCode(string? tenantCode, int skip, int take, List<string>? consumerCodesList = null)
        {
            var query = from cmr in _session.Query<ETLConsumerModel>()
                        join pn in _session.Query<ETLPersonModel>() on cmr.PersonId equals pn.PersonId
                        where cmr.TenantCode == tenantCode &&
                              cmr.DeleteNbr == 0 &&
                              pn.DeleteNbr == 0
                        orderby cmr.ConsumerId
                        select new ETLConsumerAndPersonModel()
                        {
                            Consumer = cmr,
                            Person = pn
                        };

            //filter to get only consumers from consumerCodesList 
            if (consumerCodesList != null && consumerCodesList.Count > 0)
            {
                query = from cw in query
                        where consumerCodesList.Contains(cw.Consumer.ConsumerCode)
                        select cw;
            }

            return query.Skip(skip).Take(take);
        }

        /// <summary>
        /// GetNonSyntheticConsumer
        /// </summary>
        /// <param name="anonymousCode"></param>
        /// <returns></returns>
        public async Task<ETLConsumerModel> GetNonSyntheticConsumer(string anonymousCode)
        {
            const string methodName = nameof(GetNonSyntheticConsumer);
            _logger.LogInformation($"[{_className}] : [{methodName}] : Started processing... \n anonymousCode: {anonymousCode}");
            try
            {
                var query = from cmr in _session.Query<ETLConsumerModel>()
                            join pn in _session.Query<ETLPersonModel>() on cmr.PersonId equals pn.PersonId
                            where cmr.AnonymousCode == anonymousCode &&
                                    cmr.DeleteNbr == 0 &&
                                    pn.DeleteNbr == 0 &&
                                    pn.SyntheticUser == false
                            select cmr;

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_className}] : [{methodName}]: Failed processing");
                throw;
            }
            finally
            {
                _logger.LogInformation($"[{_className}] : [{methodName}]: Completed processing.");
            }
        }
        public async Task<List<MemberInsurancePeriodDto>> GetInsurancePeriod(List<MemberGroupByKey> mem_nbrs)
        {
            const string methodName = nameof(GetInsurancePeriod);
            _logger.LogInformation("[{Class}] : [{Method}] : Started processing... to get consumer mem_nbrs: {Count}", _className, methodName, mem_nbrs?.Count ?? 0);

            try
            {
                if (mem_nbrs == null || mem_nbrs.Count == 0)
                {
                    _logger.LogWarning("[{Class}] : [{Method}] : No member numbers provided.", _className, methodName);
                    return new List<MemberInsurancePeriodDto>();
                }

                // Distinct values for filtering in the DB query
                //var memberNbrs = mem_nbrs.Select(x => x.MemberNbr).Distinct().ToList();
                var memberIds = mem_nbrs.Select(x => x.MemberId).Distinct().ToList();
                var partnerCodes = mem_nbrs.Select(x => x.PartnerCode).Distinct().ToList();

                // Build Dictionary for fast in-memory composite filtering
                var memberKeyDict = mem_nbrs
                    .Distinct()
                    .ToDictionary(
                        x => (//x.MemberNbr, 
                        x.MemberId, x.PartnerCode),
                        x => x
                    );

                // Broad DB query using single-field filters
                var dbResults = (
                    from cmr in _session.Query<ETLConsumerModel>()
                    join tm in _session.Query<ETLTenantModel>() on cmr.TenantCode equals tm.TenantCode
                    where cmr.DeleteNbr == 0
                          //&& memberNbrs.Contains(cmr.MemberNbr)
                          && memberIds.Contains(cmr.MemberId)
                          && partnerCodes.Contains(tm.PartnerCode)
                    select new
                    {
                        //cmr.MemberNbr,
                        cmr.MemberId,
                        cmr.EligibleStartTs,
                        cmr.EligibleEndTs,
                        tm.PartnerCode,
                        cmr.ConsumerCode
                    }).ToList();

                // Filter based on dictionary keys
                var result = dbResults
                    .Where(x => memberKeyDict.ContainsKey((//x.MemberNbr,
                    x.MemberId, x.PartnerCode)))
                    .Select(x => new MemberInsurancePeriodDto
                    {
                        //MemberNbr = x.MemberNbr,
                        MemberId = x.MemberId,
                        PartnerCode = x.PartnerCode,
                        EligibleStartTs = x.EligibleStartTs,
                        EligibleEndTs = x.EligibleEndTs,
                        ConsumerCode = x.ConsumerCode
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] : [{Method}] : Failed processing.", _className, methodName);
                return new List<MemberInsurancePeriodDto>();
            }
            finally
            {
                _logger.LogInformation("[{Class}] : [{Method}] : Completed processing.", _className, methodName);
            }
        }
        public async Task<List<ETLConsumerModel>> UpdateInsurancePeriodsAsync(List<MemberInsurancePeriodDto> periodDtos)
        {
            const string methodName = nameof(UpdateInsurancePeriodsAsync);

            if (periodDtos == null || periodDtos.Count == 0)
                return null;

            var validDtos = periodDtos
                .Where(x => !string.IsNullOrWhiteSpace(x.ConsumerCode))
                .ToList();

            if (validDtos.Count == 0)
                return null;

            var consumerCodesToUpdate = validDtos
                .Select(p => p.ConsumerCode)
                .Distinct()
                .ToList();

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var consumers = await _session.Query<ETLConsumerModel>()
                .Where(c => consumerCodesToUpdate.Contains(c.ConsumerCode))
                .ToListAsync();

            var periodMap = validDtos
                .GroupBy(p => p.ConsumerCode)
                .ToDictionary(g => g.Key, g => g.First());

            var updatedConsumers = new List<ETLConsumerModel>();
            foreach (var consumer in consumers)
            {
                using var tx = _session.BeginTransaction();
                try
                {
                    if (periodMap.TryGetValue(consumer.ConsumerCode, out var dto))
                    {

                        consumer.EligibleStartTs = dto.EligibleStartTs;
                        consumer.EligibleEndTs = dto.EligibleEndTs;
                        consumer.UpdateTs = now;
                        consumer.UpdateUser = Constants.UpdateUser;


                        await _session.UpdateAsync(consumer);
                        updatedConsumers.Add(consumer);
                        await tx.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    _session.Clear();
                    _logger.LogError(ex, "[{Class}] : [{Method}] : Failed processing. for consumer_Code{code}", _className, methodName, consumer.ConsumerCode);
                }
            }
            return updatedConsumers;
        }

        public IQueryable<ETLConsumerAndConsumerWalleModel> GetConsumersWalletsByWalletTypeId(string? tenantCode, long walletTypeId, int skip, int take, List<string>? consumerCodesList = null)
        {
            throw new NotImplementedException();
        }
    }
}