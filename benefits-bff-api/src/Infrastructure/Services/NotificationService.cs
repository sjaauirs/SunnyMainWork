using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _notificationServiceLogger;
        private readonly INotificationClient _notificationClient;
        private const string className = nameof(NotificationService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notificationServiceLogger"></param>
        /// <param name="notificationClient"></param>
        public NotificationService(ILogger<NotificationService> notificationServiceLogger, INotificationClient notificationClient)
        {
            _notificationServiceLogger = notificationServiceLogger;
            _notificationClient = notificationClient;
        }

        public async Task<GetTenantNotificationCategoryResponseDto> GetNotificationCategoryByTenant(string tenantCode)
        {
            const string methodName = nameof(GetNotificationCategoryByTenant);
            try
            {
                var endpoint = $"tenant-notification-category/get-all-tenant-notification-category-by-tenant-code?tenantCode=";

                var parameters = new Dictionary<string, string>
                {
                    { "tenantCode", HttpUtility.UrlEncode(tenantCode) }
                };

                var notificationCategoryList = await _notificationClient.GetId<GetTenantNotificationCategoryResponseDto>(
                    endpoint,
                    parameters
                );

                _notificationServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Notification Category Details Successfully For TenantCode : {tenantCode}", className, methodName, tenantCode);

                var categoriesList = await GetAllNotificationCategories();

                if (notificationCategoryList.TenantNotificationCategoryList != null && notificationCategoryList.TenantNotificationCategoryList.Count > 0)
                {
                    foreach (var tenantCategory in notificationCategoryList.TenantNotificationCategoryList)
                    {
                        tenantCategory.NotificationCategoryName = categoriesList.NotificationCategoriesList!
                        .FirstOrDefault(c => c.NotificationCategoryId == tenantCategory.NotificationCategoryId)?.CategoryName;
                    }
                }

                return notificationCategoryList;
            }
            catch(Exception ex)
            {
                _notificationServiceLogger.LogError("{ClassName}.{MethodName} - Retrieving Notification Category Details failed For TenantCode : {tenantCode}", className, methodName, tenantCode);
                throw;
            }
        }

        private async Task<GetAllNotificationCategoriesResponseDto> GetAllNotificationCategories()
        {
            const string methodName = nameof(GetNotificationCategoryByTenant);
            try
            {
                var endpoint = $"notification-category/all-categories";

                var parameters = new Dictionary<string, string>();

                var categoryList = await _notificationClient.GetId<GetAllNotificationCategoriesResponseDto>(
                    endpoint,
                    parameters
                );

                _notificationServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved All Notification Categories Successfully", className, methodName);

                return categoryList;
            }
            catch (Exception ex)
            {
                _notificationServiceLogger.LogError("{ClassName}.{MethodName} - Retrieving All Notification Category Details failed", className, methodName);
                throw;
            }
        }

        public async Task<ConsumerNotificationPrefResponseDto> GetConsumerNotificationPref(string tenantCode, string consumerCode)
        {
            const string methodName = nameof(GetConsumerNotificationPref);
            try
            {
                var endpoint = $"consumer-notification-pref?consumerCode={consumerCode}&tenantCode={tenantCode}";
                var parameters = new Dictionary<string, string>();
                var notificationPref = await _notificationClient.GetId<ConsumerNotificationPrefResponseDto>(
                    endpoint,
                    parameters
                );
                _notificationServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Consumer Notification Preference Details Successfully For TenantCode : {tenantCode} and ConsumerCode : {consumerCode}", className, methodName, tenantCode, consumerCode);
                return notificationPref;
            }
            catch (Exception ex)
            {
                _notificationServiceLogger.LogError("{ClassName}.{MethodName} - Retrieving Consumer Notification Preference Details failed For TenantCode : {tenantCode} and ConsumerCode : {consumerCode}", className, methodName, tenantCode, consumerCode);
                throw;
            }
        }

        public async Task<ConsumerNotificationPrefResponseDto> CreateConsumerNotificationPref(CreateConsumerNotificationPrefRequestDto createConsumerNotificationPrefRequestDto)
        {
            const string methodName = nameof(CreateConsumerNotificationPref);
            try
            {
                var notificationPref = await _notificationClient.Post<ConsumerNotificationPrefResponseDto>(
                    "consumer-notification-pref/create-consumer-notification-pref",
                    createConsumerNotificationPrefRequestDto
                );
                if (notificationPref.ErrorCode != null)
                {
                    _notificationServiceLogger.LogError("{ClassName}.{MethodName} - Creating Consumer Notification Preference Details failed with error: {error}", className, methodName, notificationPref.ErrorMessage);
                    return notificationPref;
                }
                _notificationServiceLogger.LogInformation("{ClassName}.{MethodName} - Create Consumer Notification Preference Details Successfully For TenantCode : {tenantCode} and ConsumerCode : {consumerCode}", className, methodName, createConsumerNotificationPrefRequestDto.TenantCode, createConsumerNotificationPrefRequestDto.ConsumerCode);
                return notificationPref;
            }
            catch (Exception ex)
            {
                _notificationServiceLogger.LogError("{ClassName}.{MethodName} - Creating Consumer Notification Preference Details failed For TenantCode : {tenantCode} and ConsumerCode : {consumerCode}", className, methodName, createConsumerNotificationPrefRequestDto.TenantCode, createConsumerNotificationPrefRequestDto.ConsumerCode);
                throw;
            }
        }

        public async Task<ConsumerNotificationPrefResponseDto> UpdateCustomerNotificationPref(UpdateConsumerNotificationPrefRequestDto updateConsumerNotificationPrefRequestDto)
        {
            const string methodName = nameof(UpdateCustomerNotificationPref);
            try
            {
                var notificationPref = await _notificationClient.Put<ConsumerNotificationPrefResponseDto>(
                    "consumer-notification-pref/update-consumer-notification-pref",
                    updateConsumerNotificationPrefRequestDto
                );
                if (notificationPref.ErrorCode != null)
                {
                    _notificationServiceLogger.LogError("{ClassName}.{MethodName} - Updating Consumer Notification Preference Details failed with error: {error}", className, methodName, notificationPref.ErrorMessage);
                    return notificationPref;
                }
                _notificationServiceLogger.LogInformation("{ClassName}.{MethodName} - Update Consumer Notification Preference Details Successfully For ConsumerNotificationPreferenceId : {ConsumerNotificationPreferenceId}", className, methodName, updateConsumerNotificationPrefRequestDto.ConsumerNotificationPreferenceId);
                return notificationPref;
            }
            catch (Exception ex)
            {
                _notificationServiceLogger.LogError("{ClassName}.{MethodName} - Updating Consumer Notification Preference Details failed For ConsumerNotificationPreferenceId : {ConsumerNotificationPreferenceId}", className, methodName, updateConsumerNotificationPrefRequestDto.ConsumerNotificationPreferenceId);
                throw;
            }
        }
    }
}
