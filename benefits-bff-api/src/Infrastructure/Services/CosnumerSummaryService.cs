using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using NHibernate.Util;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;
using System.Net.Http;
using System.Web;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ConsumerSummaryService : IConsumerSummaryService
    {
        private readonly ILogger<ConsumerSummaryService> _logger;
        private const string className = nameof(ConsumerSummaryService);

        private readonly IAuth0Helper _auth0Helper;
        private readonly ILoginService _loginService;
        private readonly ITenantService _tenantService;
        private readonly IWalletService _walletService;
        private readonly ITaskService _taskService;
        private readonly ITaskClient _taskClient;
        private readonly IFisClient _fisClient;
        private readonly ITenantAccountService _tenantAccountService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ConsumerSummaryService(ILogger<ConsumerSummaryService> consumerServiceLogger,
            IAuth0Helper auth0Helper, ITaskClient taskClient,
            ILoginService loginService,
            ITenantService tenantService,
            IWalletService walletService,
            ITaskService taskService, ITenantAccountService tenantAccountService, IFisClient fisClient, IHttpContextAccessor httpContextAccessor)
        {
            _logger = consumerServiceLogger;
            _auth0Helper = auth0Helper;
            _loginService = loginService;
            _tenantService = tenantService;
            _walletService = walletService;
            _taskService = taskService;
            _tenantAccountService = tenantAccountService;
            _taskClient = taskClient;
            _fisClient = fisClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ConsumerSummaryResponseDto> GetConsumerSummary(ConsumerSummaryRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(GetConsumerSummary);
            try
            {
                if (String.IsNullOrWhiteSpace(consumerSummaryRequestDto.consumerCode))
                {
                    return new ConsumerSummaryResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid Input Data, ConsumerCode is Required" };
                }
                var apiResult = new ConsumerSummaryResponseDto();


                //Consumer
                GetConsumerByEmailResponseDto? consumerByEmail = null;
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.Items.TryGetValue(HttpContextKeys.ConsumerInfo, out var value) == true &&
                        value is GetConsumerByPersonUniqueIdentifierResponseDto cachedConsumerDetails)
                {
                    consumerByEmail = new GetConsumerByEmailResponseDto()
                    {
                        Consumer = cachedConsumerDetails.Consumer,
                        Person = cachedConsumerDetails.Person
                    };
                }
                else
                {
                    consumerByEmail = await _loginService.GetPersonAndConsumerDetails(consumerSummaryRequestDto.consumerCode);
                }

                ValidateApiResponse(consumerByEmail);
                apiResult.consumerInfo = consumerByEmail;

                //tenantResponse
                TenantDto? tenantDetails = null;
                if (httpContext?.Items.TryGetValue(HttpContextKeys.TenantInfo, out var tenantInfo) == true &&
                    tenantInfo is TenantDto cachedTenantInfo)
                {
                    tenantDetails = cachedTenantInfo;
                }
                else
                {
                    tenantDetails = await _tenantService.GetTenantByTenantCode(consumerByEmail?.Consumer?.FirstOrDefault()?.TenantCode ?? string.Empty);
                }

                if (tenantDetails == null || string.IsNullOrEmpty(tenantDetails.TenantCode))
                {
                    throw new InvalidDataException($"Tenant not found for Consumer with tenant code: {tenantDetails?.TenantCode}");
                }
                var tenantResponse = new GetTenantResponseDto
                {
                    Tenant = tenantDetails
                };
                apiResult.TenantInfo = tenantResponse;

                var tenantCode = tenantDetails.TenantCode;

                //Tenant Account Response
                var tenantAccountRequest = new TenantAccountCreateRequestDto
                {
                    TenantCode = tenantCode
                };
                var tenantAccountResponse = await _tenantAccountService.GetTenantAccount(tenantAccountRequest);
                ValidateApiResponse(tenantAccountResponse);
                apiResult.TenantAccountInfo = tenantAccountResponse;

                //walletResponse
                var findConsumerWalletRequestDto = new FindConsumerWalletRequestDto()
                {
                    ConsumerCode = consumerSummaryRequestDto.consumerCode,
                    IncludeRedeemOnlyWallets = true
                };
                var walletResponse = await _walletService.GetWallets(findConsumerWalletRequestDto, tenantResponse.Tenant);
                // ValidateApiResponse(walletResponse);
                IndexPrefixWalletName(walletResponse);
                UpdateWalletTagWithActiveInactivePrefix(walletResponse);
                apiResult.WalletInfo = walletResponse;

                //Get Consumer Account
                var getConsumerAccountRequestDto = new GetConsumerAccountRequestDto
                {
                    ConsumerCode = consumerSummaryRequestDto.consumerCode,
                    TenantCode = tenantCode
                };

                apiResult.CardIssueStatus = await GetCardIssueStatus(getConsumerAccountRequestDto);

                //if CostcoMemberShipSupport flag in tenant level is set to true, then only we will fetch the consumer tasks
                if (_tenantService.CheckCostcoMemberhipSupport(tenantResponse.Tenant))
                {
                    var consumerTasks = await _taskService.GetConsumerTasks(consumerSummaryRequestDto.consumerCode, tenantCode, tenantResponse.Tenant!, consumerSummaryRequestDto.LanguageCode);

                    //flags
                    apiResult.HasCompletedMembershipAction = consumerTasks.CompletedTasks?.Exists(x =>
                    {
                        if (x.TaskReward?.Reward == null)
                            return false;

                        try
                        {
                            // Deserialize the Reward JSON
                            var reward = JsonConvert.DeserializeObject<RewardDto>(x.TaskReward.Reward);
                            return reward?.MembershipType != null;
                        }
                        catch (JsonException)
                        {
                            // Handle invalid JSON gracefully
                            return false;
                        }
                    }) ?? false;

                    apiResult.HasPendingMembershipAction = consumerTasks.PendingTasks?.Exists(x =>
                    {
                        if (x.TaskReward?.Reward == null)
                            return false;
                        var reward = JsonConvert.DeserializeObject<RewardDto>(x.TaskReward.Reward);
                        return reward?.MembershipType != null;
                    }) ?? false;

                    //membershipTaskRewards
                    if (consumerTasks.AvailableTasks != null)
                    {
                        var membershipTaskRewards = consumerTasks.AvailableTasks
                           .Where(x =>
                           {
                               if (string.IsNullOrEmpty(x.TaskReward?.Reward))
                                   return false;

                               var reward = JsonConvert.DeserializeObject<RewardDto>(x.TaskReward.Reward);
                               return reward != null && reward.MembershipType != null;
                           })
                           .ToList();

                        apiResult.MembershipTaskRewards = membershipTaskRewards;
                    }
                }
                var healthmetrics = await GetHealthMatrics(tenantCode);

                if (healthmetrics.Count > 0)
                {
                    apiResult.HealthMetricsQueryStartTsMap = new Dictionary<string, DateTime?>(healthmetrics);

                }

                return apiResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while fetching ConsumerSummary,  ConsumerCode: {ConsumerCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, consumerSummaryRequestDto.consumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw new InvalidProgramException(ex.Message);
            }
        }

        public async Task<GetConsumerByEmailResponseDto> GetConsumerDetails(ConsumerSummaryRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(GetConsumerDetails);
            try
            {
                if (String.IsNullOrWhiteSpace(consumerSummaryRequestDto.consumerCode))
                {
                    return new GetConsumerByEmailResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid Input Data, ConsumerCode is Required" };
                }
                var apiResult = new ConsumerSummaryResponseDto();


                //Consumer
                GetConsumerByEmailResponseDto? consumerByEmail = null;
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.Items.TryGetValue(HttpContextKeys.ConsumerInfo, out var value) == true &&
                        value is GetConsumerByPersonUniqueIdentifierResponseDto cachedConsumerDetails)
                {
                    consumerByEmail = new GetConsumerByEmailResponseDto()
                    {
                        Consumer = cachedConsumerDetails.Consumer,
                        Person = cachedConsumerDetails.Person
                    };
                }
                else
                {
                    consumerByEmail = await _loginService.GetPersonAndConsumerDetails(consumerSummaryRequestDto.consumerCode);
                }

                ValidateApiResponse(consumerByEmail);


                return consumerByEmail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while fetching ConsumerDetails,  ConsumerCode: {ConsumerCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, consumerSummaryRequestDto.consumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw new InvalidProgramException(ex.Message);
            }
        }

        private void IndexPrefixWalletName(WalletResponseDto walletResponse)
        {
            if (walletResponse?.walletDetailDto == null)
                return;

            // Group wallets by WalletTypeId
            var groups = walletResponse.walletDetailDto
            .GroupBy(w => w.WalletType.WalletTypeLabel?.Trim(),
                     StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);


            foreach (var group in groups)
            {
                // Order each wallet type by ActiveEndTs DESC (latest first)
                var orderedWallets = group
                    .OrderBy(w => w.Wallet.ActiveEndTs)
                    .ToList();


                // Assign index and  prefix wallet name with year
                for (int i = 0; i < orderedWallets.Count; i++)
                {

                    var year = orderedWallets[i].Wallet.ActiveEndTs.ToString("yyyy");
                    orderedWallets[i].WalletType.WalletTypeLabel = $"{year} {orderedWallets[i].WalletType.WalletTypeLabel}";

                    // Add order
                    orderedWallets[i].Wallet.Index = i;

                    _logger.LogInformation(
                        "Ordered Wallet → TypeId: {TypeId}, WalletId: {WalletId}, ActiveEndTs: {End}, Index: {Index}",
                        orderedWallets[i].Wallet.WalletTypeId,
                        orderedWallets[i].Wallet.WalletId,
                        orderedWallets[i].Wallet.ActiveEndTs,
                        i
                    );
                }
            }
        }

        private void UpdateWalletTagWithActiveInactivePrefix(WalletResponseDto walletResponse)
        {
            bool isTagUpdated =
                  walletResponse?.walletDetailDto != null &&
                  walletResponse.walletDetailDto.Count() > 2 &&
                  walletResponse.walletDetailDto.Any(wd => wd?.Wallet?.IsDeactivated == true);

            if (!isTagUpdated)
                return;

            if (!walletResponse.walletDetailDto.Any(wd => wd?.Wallet?.IsDeactivated == true))
                return;

            walletResponse?.walletDetailDto?
                .Where(wd =>
                    wd?.Wallet != null &&
                    wd?.WalletType != null &&
                    !string.IsNullOrWhiteSpace(wd.WalletType.ConfigJson))
                .ToList()
                .ForEach(wd =>
                {
                    var configDict = JsonConvert
                        .DeserializeObject<Dictionary<string, object>>(wd.WalletType.ConfigJson);

                    if (configDict == null || !configDict.TryGetValue("tag", out var tagObj) || tagObj == null)
                        return;

                    var tag = tagObj.ToString();

                    // Apply prefix based on isDeactivated
                    tag = wd.Wallet.IsDeactivated ? $"Inactive {tag}" : $"Active {tag}";

                    configDict["tag"] = tag;

                    wd.WalletType.ConfigJson =
                        JsonConvert.SerializeObject(configDict);
                });
        }



        private void ValidateApiResponse(BaseResponseDto response)
        {
            if (response != null && response.ErrorCode != null)
            {
                _logger.LogError("Error: {ErrorCode}, Message: {ErrorMessage}", response.ErrorCode, response.ErrorMessage);
                throw new InvalidOperationException(response.ErrorMessage);
            }
        }
        private async Task<IDictionary<string, DateTime?>> GetHealthMatrics(string tenantCode)
        {
            const string methodName = nameof(GetHealthMatrics);

            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = tenantCode };
            try
            {
                var data = await _taskClient.Post<HealthMetricsSummaryDto>(CommonConstants.GetHealthMetrics, healthMetricsRequestDto);
                _logger.LogInformation("{ClassName}.{MethodName} - Retrieved Data Successfully for get-health-metrics", className, methodName);
                if (data != null && data.ErrorCode == null && data.HealthMetricsQueryStartTsMap != null && data.HealthMetricsQueryStartTsMap.Count > 0)
                {
                    return data.HealthMetricsQueryStartTsMap;
                }
                _logger.LogError("{ClassName}.{MethodName} - Retrieved Data was Unsuccessfully for get-health-metrics tenant code {tenantcode} errorCode: {errorcode} and message {error}", className, methodName,
                    tenantCode, data?.ErrorCode, data?.ErrorMessage);

                return new Dictionary<string, DateTime?>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while get-health-metrics, ErrorCode:{ErrorCode}, ERROR: {Msg}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new Dictionary<string, DateTime?>();
            }

        }

        private async Task<string> GetCardIssueStatus(GetConsumerAccountRequestDto getConsumerAccountRequestDto)
        {
            const string methodName = nameof(GetCardIssueStatus);
            try
            {
                var consumerAccountResponse = await _fisClient.Post<GetConsumerAccountResponseDto>(CommonConstants.GetConsumerAccount, getConsumerAccountRequestDto);
                if (consumerAccountResponse != null && consumerAccountResponse.ErrorCode == null && consumerAccountResponse.ConsumerAccount != null && !string.IsNullOrEmpty(consumerAccountResponse.ConsumerAccount.CardIssueStatus))
                {
                    return consumerAccountResponse.ConsumerAccount.CardIssueStatus;
                }
                _logger.LogError("{ClassName}.{MethodName} - Retrieved data unsuccessfully for get-consumer-account for tenant code {tenantcode}, consumer code {consumercode} errorCode: {errorcode}, and message {error}", className, methodName,
                    getConsumerAccountRequestDto.TenantCode, getConsumerAccountRequestDto.ConsumerCode, consumerAccountResponse?.ErrorCode, consumerAccountResponse?.ErrorMessage);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while get-consumer-account, ErrorCode:{ErrorCode}, Error: {Msg}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return string.Empty;
            }
        }

    }
}
