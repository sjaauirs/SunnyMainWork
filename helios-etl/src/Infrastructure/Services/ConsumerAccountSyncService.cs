using FirebaseAdmin.Auth.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class ConsumerAccountSyncService : IConsumerAccountSyncService
    {
        private readonly ILogger<ConsumerAccountSyncService> _logger;
        private readonly IAdminClient _adminClient;
        private const string className = nameof(ConsumerAccountSyncService);

        public ConsumerAccountSyncService(ILogger<ConsumerAccountSyncService> logger, IAdminClient adminClient)
        {
            _logger = logger;
            _adminClient = adminClient;
        }

        /// <summary>
        /// Sync the consumer account config with tenant_account config
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="tenantAccountRequestDto">The tenant account request dto.</param>
        /// <param name="consumerCodes">The consumer codes.</param>
        public async Task SyncConsumerAccountAsync(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccountRequestDto, string consumerCodes)
        {
            const string methodName = nameof(SyncConsumerAccountAsync);
            try
            {
                var tenantOption = tenantDto?.TenantOption != null ? JsonConvert.DeserializeObject<TenantOption>(tenantDto.TenantOption) : new TenantOption();
                if (tenantOption?.Apps.FindIndex(x => x?.ToUpper() == Constants.Benefits) > -1)
                {

                    if (string.IsNullOrWhiteSpace(consumerCodes))
                    {
                        LogAndThrowInvalidConsumerCode(methodName, tenantDto?.TenantCode);
                    }

                    if (consumerCodes.Trim().ToUpper() == AdminConstants.ConsumerCodesAll)
                    {
                        await SyncAllConsumers(tenantDto, tenantAccountRequestDto);
                    }
                    else
                    {
                        await SyncSpecificConsumers(tenantDto, tenantAccountRequestDto, consumerCodes);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}  - Failed processing consumer account config sync. ErrorCode:{Code},ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
            
        }

        private void LogAndThrowInvalidConsumerCode(string methodName, string? tenantCode)
        {
            _logger.LogError("{ClassName}.{MethodName} - Invalid consumer codes in job params: {TenantCode}. ErrorCode: {Code}",
                className, methodName, tenantCode, StatusCodes.Status500InternalServerError);

            throw new ETLException(ETLExceptionCodes.NullValue, $"Invalid consumers code in job params: {tenantCode}. ErrorCode: {StatusCodes.Status500InternalServerError}");
        }

        private async Task<GetConsumerAccountResponseDto> GetConsumerAccount(string? tenantCode, string? consumerCode)
        {
            var getConsumerWalletRequestDto = new GetConsumerAccountRequestDto()
            {
                TenantCode = tenantCode,
                ConsumerCode = consumerCode
            };
            return await _adminClient.Post<GetConsumerAccountResponseDto>(AdminConstants.GetConsumerAccount, getConsumerWalletRequestDto);
        }


        private async Task SyncAllConsumers(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccount)
        {
            try
            {
                int pageNumber = 1;

                while (true)
                {
                    var request = new GetConsumerByTenantRequestDto
                    {
                        TenantCode = tenantDto?.TenantCode,
                        PageNumber = pageNumber,
                        SearchTerm = string.Empty,
                        PageSize = AdminConstants.GetConsumersByTenantCodePageSize
                    };

                    var response = await _adminClient.Post<ConsumersAndPersonsListResponseDto>(AdminConstants.GetConsumersByTenantCode, request);
                    if (response?.ConsumerAndPersons == null || response.ConsumerAndPersons.Count() == 0)
                    {
                        break;
                    }

                    foreach (var consumerAndPerson in response.ConsumerAndPersons)
                    {
                        await SyncConsumerAccountConfig(tenantDto, tenantAccount, consumerAndPerson.Consumer);
                    }

                    pageNumber++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while syncing consumer wallets for tenant: {TenantCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
              className, nameof(SyncAllConsumers), tenantDto?.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }

        }

        private async Task SyncSpecificConsumers(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccount, string consumerCodes)
        {
            try
            {
                var consumerCodeArray = consumerCodes.Split(',');
                foreach (var consumerCode in consumerCodeArray)
                {
                    var request = new GetConsumerRequestDto { ConsumerCode = consumerCode };
                    var consumerData = await GetConsumerData(request);

                    if (consumerData?.Consumer != null)
                    {
                        await SyncConsumerAccountConfig(tenantDto, tenantAccount, consumerData.Consumer);
                    }
                    else
                    {
                        _logger.LogWarning("{ClassName}.{MethodName} - Consumer not found: {ConsumerCode}", className, nameof(SyncSpecificConsumers), consumerCode);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while syncing specific consumer wallets for tenant: {TenantCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
             className, nameof(SyncSpecificConsumers), tenantDto?.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private async Task SyncConsumerAccountConfig(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccount, ConsumerDto? consumer)
        {
            var methodName = nameof(SyncConsumerAccountConfig);
            try
            {
                if (tenantDto == null || string.IsNullOrEmpty(tenantDto?.TenantCode) || consumer == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - Missing required parameters", className, nameof(SyncConsumerAccountConfig));
                    return;
                }

                var consumerAccountResponse = await GetConsumerAccount(tenantDto?.TenantCode, consumer.ConsumerCode);
                if (consumerAccountResponse == null || consumerAccountResponse.ErrorCode != null || consumerAccountResponse.ConsumerAccount == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - TenantCode is null or empty for PartnerCode:{PartnerCode}", className, methodName, tenantDto?.PartnerCode);
                }

                if (!string.IsNullOrEmpty(consumerAccountResponse?.ConsumerAccount?.ConsumerAccountConfigJson))
                {
                    var consumerAccountConfig =  JsonConvert.DeserializeObject<ConsumerAccountConfig>(consumerAccountResponse?.ConsumerAccount?.ConsumerAccountConfigJson!);
                    var tenantConfig = string.IsNullOrEmpty(tenantAccount?.TenantConfigJson)
                       ? new TenantConfigDto()
                       : JsonConvert.DeserializeObject<TenantConfigDto>(tenantAccount.TenantConfigJson);
                    var tenantLevelPurses = tenantConfig?.PurseConfig?.Purses?.ToList();
                    var consumerLevelPurses = consumerAccountConfig?.PurseConfig?.Purses?.ToList();
                    var newConsuemrPurses = new List<ConsumerAccountPurse>();
                    if (tenantLevelPurses != null && tenantLevelPurses.Any() && consumerLevelPurses != null && consumerLevelPurses.Any())
                    {
                        foreach (var tenantPurse in tenantLevelPurses)
                        {
                            // check is purse exist in consumer account config json
                            var consumerPurse = consumerLevelPurses?.FirstOrDefault(config => config.PurseLabel == tenantPurse.PurseLabel);
                            if (consumerPurse != null)
                            {
                                UpdateConsumerAccountPurse(consumerPurse, tenantPurse);
                            }
                            if (consumerPurse == null)
                            {
                                var consumerAccountPurse = CreateConsumerAccountPurse(tenantPurse);
                                newConsuemrPurses.Add(consumerAccountPurse);
                            }
                        }
                        if (newConsuemrPurses.Any())
                        {
                            consumerAccountConfig?.PurseConfig?.Purses?.AddRange(newConsuemrPurses);
                        }

                        RemoveOrphanedConsumerPurses(consumerAccountConfig?.PurseConfig?.Purses, tenantLevelPurses);

                        var consumerAccountUpdateRequestDto = new ConsumerAccountUpdateRequestDto()
                        {
                            TenantCode = tenantDto?.TenantCode ?? string.Empty,
                            ConsumerCode = consumer?.ConsumerCode ?? string.Empty,
                            ConsumerAccountConfig = consumerAccountConfig
                        };
                        var consumerAccountUpdateRespone =  await _adminClient.Patch<GetConsumerResponseDto>(AdminConstants.ConsumerAccountAPIUrl, consumerAccountUpdateRequestDto);
                        if(consumerAccountUpdateRespone == null || consumerAccountUpdateRespone.ErrorCode != null)
                        {
                            _logger.LogError("{ClassName}.{MethodName} - Error occurred while syncing consumer account config for ConsuemrCode: {ConsuemrCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
                                className, methodName, tenantDto?.TenantCode, consumerAccountUpdateRespone?.ErrorCode, consumerAccountUpdateRespone?.ErrorMessage);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating consumer wallets for tenant: {TenantCode}. ErrorCode: {Code}, ErrorMessage: {ErrorMessage}",
                     className, methodName, tenantDto?.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        private ConsumerAccountPurse CreateConsumerAccountPurse(PurseDto? tenantPurse)
        {
            var consumerAccountPurse = new ConsumerAccountPurse();
            consumerAccountPurse.PurseLabel = tenantPurse?.PurseLabel;
            consumerAccountPurse.IsFilteredSpend = tenantPurse!.IsFilteredSpend;
            consumerAccountPurse.RedemptionTarget = tenantPurse.RedemptionTarget;
            consumerAccountPurse.Enabled = false;

            if (tenantPurse?.PickAPurseStatus?.ToUpper() == PickAPurseStatus.DEFAULT_INCLUDE.ToString())
            {
                consumerAccountPurse.Enabled = true;
            }
            return consumerAccountPurse;
        }

        /// <summary>
        /// It removes purses that are no longer mapped at the tenant level.
        /// </summary>
        /// <param name="consumerAccountPurses">The consumer account purses.</param>
        /// <param name="tenantPurses">The tenant purses.</param>
        private void RemoveOrphanedConsumerPurses(List<ConsumerAccountPurse>? consumerAccountPurses, List<PurseDto>? tenantPurses)
        {
            consumerAccountPurses?.RemoveAll(consumerPurse =>
            tenantPurses?.Any(tenantPurse => tenantPurse.PurseLabel == consumerPurse.PurseLabel) == false);
        }

        private void UpdateConsumerAccountPurse(ConsumerAccountPurse consumerAccountPurse, PurseDto? tenantPurse)
        {
            consumerAccountPurse.IsFilteredSpend = tenantPurse!.IsFilteredSpend;
            consumerAccountPurse.RedemptionTarget = tenantPurse.RedemptionTarget;
            if (tenantPurse?.PickAPurseStatus?.ToUpper() == PickAPurseStatus.DEFAULT_INCLUDE.ToString())
            {
                consumerAccountPurse.Enabled = true;
            }
            else if (tenantPurse?.PickAPurseStatus?.ToUpper() == PickAPurseStatus.DISABLED.ToString())
            {
                consumerAccountPurse.Enabled = false;
            }
        }

        public async Task<GetConsumerResponseDto> GetConsumerData(GetConsumerRequestDto consumerRequestDto)
        {
            return await _adminClient.Post<GetConsumerResponseDto>(AdminConstants.GetConsumer, consumerRequestDto);
        }
    }
}
