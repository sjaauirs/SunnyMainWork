using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class PersonRepo : BaseRepo<ETLPersonModel>, IPersonRepo
    {
        private readonly NHibernate.ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public PersonRepo(ILogger<BaseRepo<ETLPersonModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public IQueryable<ETLPersonModel> GetConsumerPersons(string tenantCode, int skip, int take, TenantOption tenantOption, string cohortCode)
        {
            // Determine if ImmediateCardFlowType option is enabled and if manual card request is required
            var flowTypeMatch = tenantOption?.BenefitsOptions?.CardIssueFlowType?
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(cohortCode)
                        ? x.CohortCode.Contains(cohortCode, StringComparer.OrdinalIgnoreCase)
                        : tenantOption.BenefitsOptions.CardIssueFlowType.Count == 1);
            var isImmediateOption = flowTypeMatch?.FlowType == BenefitsConstants.ImmediateCardFlowType;
            var isManualCardRequestRequired = tenantOption?.BenefitsOptions?.ManualCardRequestRequired ?? false;

            // Get the base query for persons based on TenantCode and DeleteNbr
            var basePersonQuery = _session.Query<ETLPersonModel>()
                .Where(p => p.DeleteNbr == 0);
            var cohortId = _session.Query<ETLCohortModel>()
                .Where(co => co.CohortCode == cohortCode && co.DeleteNbr == 0).Select(co => co.CohortId);

            // Get the consumer codes based on conditions, shared between both branches
            IQueryable<string> eligibleConsumerCodes;

            if (isImmediateOption)
            {
                eligibleConsumerCodes = GetConsumerCodesForNotIssuedCards(tenantCode);
            }
            else
            {
                eligibleConsumerCodes = GetConsumerCodesForEligibleCards(tenantCode, isManualCardRequestRequired);
            }
            // Join the consumer data with persons and cohorts
            var personsQuery = from p in basePersonQuery
                               join c in _session.Query<ETLConsumerModel>() on p.PersonId equals c.PersonId
                               join co in _session.Query<ETLCohortConsumerModel>() on new { c.ConsumerCode, c.TenantCode }
                                    equals new { co.ConsumerCode, co.TenantCode }
                               where eligibleConsumerCodes != null && eligibleConsumerCodes.Contains(c.ConsumerCode) &&
                                     c.TenantCode == tenantCode &&
                                     c.DeleteNbr == 0 &&
                                     cohortId.Contains(co.CohortId) &&
                                     co.DeleteNbr == 0
                               select p;

            // Return paginated result
             return personsQuery.OrderBy(p => p.PersonId).Skip(skip).Take(take);
        }

        private IQueryable<string> GetConsumerCodesForEligibleCards(string tenantCode, bool isManualCardRequestRequired)
        {
            return _session.Query<ETLConsumerAccountModel>()
                .Where(c => c.TenantCode == tenantCode &&
                            c.DeleteNbr == 0 &&
                            c.ProxyNumber != null && c.ProxyNumber != "" &&
                            c.CardIssueStatus == BenefitsConstants.EligibleCardIssueStatus &&
                            (!isManualCardRequestRequired || c.CardRequestStatus == BenefitsConstants.RequestedCardRequestStatus))
                .Select(c => c.ConsumerCode)
                .Distinct();
        }

        private IQueryable<string> GetConsumerCodesForNotIssuedCards(string tenantCode)
        {
            return _session.Query<ETLConsumerAccountModel>()
                .Where(c => c.TenantCode == tenantCode &&
                            c.DeleteNbr == 0 &&
                            c.ProxyNumber != null && c.ProxyNumber != "" &&
                            c.ProxyNumber == BenefitsConstants.ProxyNumber &&
                            c.CardIssueStatus != BenefitsConstants.IssuedCardRequestStatus)
                .Select(c => c.ConsumerCode)
                .Distinct();
        }

        public async Task<ETLConsumerModel?> GetConsumerByPersonUniqueIdentifierAndTenantCode(string? personUniqueIdentifier, string? tenantCode)
        {
            var query = from p in _session.Query<ETLPersonModel>()
                        join c in _session.Query<ETLConsumerModel>() on p.PersonId equals c.PersonId
                        where
                            p.PersonUniqueIdentifier == personUniqueIdentifier &&
                            c.TenantCode == tenantCode &&
                            c.DeleteNbr == 0 &&
                            p.DeleteNbr == 0
                        select c;

            return await query.FirstOrDefaultAsync();
        }


        public async Task<ETLConsumerModel?> GetConsumerByEmailAndTenantCode(string? email, string? tenantCode)
        {
            var query = from p in _session.Query<ETLPersonModel>()
                        join c in _session.Query<ETLConsumerModel>() on p.PersonId equals c.PersonId
                        where
                            p.Email == email &&
                            c.TenantCode == tenantCode &&
                            c.DeleteNbr == 0 &&
                            p.DeleteNbr == 0
                        select c;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ETLConsumerModel?> GetConsumerByPersonIdAndTenantCode(long personId, string? tenantCode)
        {
            var query = from p in _session.Query<ETLPersonModel>()
                        join c in _session.Query<ETLConsumerModel>() on p.PersonId equals c.PersonId
                        where
                            p.PersonId == personId &&
                            c.TenantCode == tenantCode &&
                            c.DeleteNbr == 0 &&
                            p.DeleteNbr == 0
                        select c;

            return await query.FirstOrDefaultAsync();
        }
        public ETLPersonModel? GetConsumerPersonForUpdateInfo(string tenantCode, string consumerCode)
        {

            var person = (from p in _session.Query<ETLPersonModel>()
                          join c in _session.Query<ETLConsumerModel>() on p.PersonId equals c.PersonId
                          where
                              c.ConsumerCode == consumerCode &&
                              c.TenantCode == tenantCode &&
                              c.DeleteNbr == 0 &&
                              p.DeleteNbr == 0
                          select p).FirstOrDefault();

            return person;
        }
    }
}