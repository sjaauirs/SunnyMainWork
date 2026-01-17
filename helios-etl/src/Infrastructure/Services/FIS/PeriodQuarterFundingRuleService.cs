using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class PeriodQuarterFundingRuleService : IFundingRuleExecService
    {
        private readonly ILogger<PeriodQuarterFundingRuleService> _logger;
        private readonly IFundTransferService _fundTransferService;
        private readonly NHibernate.ISession _session;
        const string className = nameof(PeriodQuarterFundingRuleService);

        public PeriodQuarterFundingRuleService(ILogger<PeriodQuarterFundingRuleService> logger,
            IFundTransferService fundTransferService, NHibernate.ISession session)
        {
            _logger = logger;
            _fundTransferService = fundTransferService;
            _session = session;
        }

        public async Task ExecuteFundingRuleAsync(FISFundingRuleDto fundingRule, FISFundTransferRequestDto fundTransferRequest)
        {
            if (!await IsFundingAlreadyExecutedInCurrentQuarter(fundTransferRequest.TenantCode, fundTransferRequest.ConsumerCode, fundingRule))
            {
                await ExecuteFundTransferAsync(fundTransferRequest);
            }
        }

        public async Task ExecuteFundTransferAsync(FISFundTransferRequestDto fISFundTransferRequest)
        {
            await _fundTransferService.ExecuteFundTransferAsync(fISFundTransferRequest);
        }

        /// <summary>
        /// Verify the funding rule is already executed in the current quarter or not
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="fundingRule"></param>
        /// <returns></returns>
        private async Task<bool> IsFundingAlreadyExecutedInCurrentQuarter(string? tenantCode, string? consumerCode, FISFundingRuleDto fundingRule)
        {
            const string methodName = nameof(IsFundingAlreadyExecutedInCurrentQuarter);
            // Get the current date and month
            var currentMonth = DateTime.UtcNow.Month;

            // Calculate the start month of the current quarter
            int quarterStartMonth = (currentMonth - 1) / 3 * 3 + 1;

            // Get the months of the current quarter
            int[] quaterMonths = new int[3];
            for (int i = 0; i < 3; i++)
            {
                quaterMonths[i] = quarterStartMonth + i;
            }

            var fundingHistory = await _session.Query<ETLFundingHistoryModel>().Where(x => x.TenantCode == tenantCode &&
            x.ConsumerCode == consumerCode && x.FundRuleNumber == fundingRule.RuleNumber && quaterMonths.Contains(x.FundTs.Month) &&
            x.FundTs.Year == DateTime.UtcNow.Year && x.DeleteNbr == 0).FirstOrDefaultAsync();

            if (fundingHistory != null)
            {
                _logger.LogInformation($"{className}.{methodName}: Fund already executed in current quarter, " +
                    $"TenantCode:{tenantCode}, ConsumerCode: {consumerCode}, RuleNumber: {fundingRule.RuleNumber}, fundDate:{fundingHistory.FundTs}");
                return true;
            }

            return false;
        }
    }
}