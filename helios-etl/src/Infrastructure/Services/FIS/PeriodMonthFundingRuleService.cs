using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class PeriodMonthFundingRuleService : IFundingRuleExecService
    {
        private readonly ILogger<PeriodMonthFundingRuleService> _logger;
        private readonly IFundTransferService _fundTransferService;
        private readonly NHibernate.ISession _session;
        const string className = nameof(PeriodMonthFundingRuleService);

        public PeriodMonthFundingRuleService(ILogger<PeriodMonthFundingRuleService> logger,
            IFundTransferService fundTransferService, NHibernate.ISession session)
        {
            _logger = logger;
            _fundTransferService = fundTransferService;
            _session = session;
        }

        public async Task ExecuteFundingRuleAsync(FISFundingRuleDto fundingRule, FISFundTransferRequestDto fundTransferRequest)
        {
            if (!await IsFundingAlreadyExecutedAsync(fundTransferRequest.TenantCode, fundTransferRequest.ConsumerCode, fundingRule))
            {
                await ExecuteFundTransferAsync(fundTransferRequest);
            }
        }

        public async Task ExecuteFundTransferAsync(FISFundTransferRequestDto fISFundTransferRequest)
        {
            await _fundTransferService.ExecuteFundTransferAsync(fISFundTransferRequest);
        }

        private async Task<bool> IsFundingAlreadyExecutedAsync(string? tenantCode, string? consumerCode, FISFundingRuleDto fundingRule)
        {
            const string methodName = nameof(IsFundingAlreadyExecutedAsync);
            var fundingHistory = await _session.Query<ETLFundingHistoryModel>().Where(x => x.TenantCode == tenantCode &&
            x.ConsumerCode == consumerCode && x.FundRuleNumber == fundingRule.RuleNumber && x.FundTs.Month == DateTime.UtcNow.Month &&
            x.FundTs.Year == DateTime.UtcNow.Year && x.DeleteNbr == 0).FirstOrDefaultAsync();

            if (fundingHistory != null)
            {
                _logger.LogInformation($"{className}.{methodName}: Fund already executed, TenantCode:{tenantCode}, ConsumerCode: {consumerCode}, RuleNumber: {fundingRule.RuleNumber}");
                return true;
            }

            return false;
        }
    }
}
