using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly ISecretHelper _secretHelper;
        private readonly IDataFeedClient _dataFeedClient;

        public TokenService(ISecretHelper secretHelper, IDataFeedClient dataFeedClient)
        {
            _secretHelper = secretHelper;
            _dataFeedClient = dataFeedClient;
        }

        public async Task<TokenResponseDto> GetXAPISessionToken(string tenantCode, CustomerRequestDto customerRequestDto)
        {
            var xApiKeySecret = await _secretHelper.GetTenantSecret(tenantCode, Constants.XApiKeySecret);

            var authHeaders = new Dictionary<string, string>
            {
                { Constants.XApiKey, xApiKeySecret }
            };

            var tokenResponse = await _dataFeedClient.Post<TokenResponseDto>(Constants.Token, customerRequestDto, authHeaders);

            return tokenResponse;
        }
    }
}
