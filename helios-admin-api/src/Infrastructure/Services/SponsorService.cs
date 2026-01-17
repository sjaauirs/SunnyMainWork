using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class SponsorService : ISponsorService
    {
        public readonly ILogger<SponsorService> _logger;
        private readonly ITenantClient _tenantClient;
        public const string className = nameof(SponsorService);

        public SponsorService(ILogger<SponsorService> logger, ITenantClient tenantClient)
        {
            _logger = logger;
            _tenantClient = tenantClient;
        }

        public async Task<BaseResponseDto> CreateSponsor(CreateSponsorDto sponsorRequestDto)
        {
            return await _tenantClient.Post<BaseResponseDto>(Constant.Sponsor, sponsorRequestDto);
        }

        public async Task<SponsorsResponseDto> GetSponsors()
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _tenantClient.Get<SponsorsResponseDto>(Constant.Sponsors, parameters);
        }
    }
}
