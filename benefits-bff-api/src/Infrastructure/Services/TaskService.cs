using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly ILogger<TaskService> _logger;
        private readonly ITaskClient _taskClient;
        private readonly ICohortClient _cohortClient;
        private readonly IConfiguration _configuration;
        private const string className = nameof(TaskService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TaskServiceLogger"></param>
        /// <param name="taskClient"></param>
        public TaskService(ILogger<TaskService> TaskServiceLogger,
            ITaskClient taskClient,
            ICohortClient cohortClient,
            IConfiguration configuration)
        {
            _logger = TaskServiceLogger;
            _taskClient = taskClient;
            _cohortClient = cohortClient;
            _configuration = configuration;
        }

        public async Task<ConsumerTaskResponseDto> GetConsumerTasks(string consumerCode , string tenantCode, TenantDto tenant, string? languageCode)
        {
            const string methodName = nameof(GetConsumerTasks);
            ConsumerTaskResponseDto consumerTaskResponseDto = new ConsumerTaskResponseDto();
            var consumerTaskRequestDto = new ConsumerTaskRequestDto()
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                LanguageCode = languageCode
            };
            var allConsumerTasks = await _taskClient.Post<ConsumerTaskResponseDto>("get-all-consumer-tasks", consumerTaskRequestDto);
            _logger.LogInformation("{className}.{methodName}: Retrieved AllConsumerTasks Successfully for ConsumerCode : {ConsumerCode}, TenantCode : {TenantCode}", className, methodName,
                    consumerTaskRequestDto?.ConsumerCode, consumerTaskRequestDto?.TenantCode);
            if (!tenant.RecommendedTask)
            {
                allConsumerTasks.AvailableTasks = allConsumerTasks.AvailableTasks?.OrderByDescending(x => x.TaskReward?.Priority).ToList();

                consumerTaskResponseDto = allConsumerTasks!;
            }
            else
            {
                var getConsumerRecommendedTasksRequestDto = new GetConsumerRecommendedTasksRequestDto()
                {
                    TenantCode = tenant.TenantCode ?? string.Empty,
                    ConsumerCode = consumerTaskRequestDto?.ConsumerCode ?? string.Empty
                };
                var recommendedTasks = await GetCohortRecommendedTask(getConsumerRecommendedTasksRequestDto, allConsumerTasks.AvailableTasks);
                consumerTaskResponseDto = new ConsumerTaskResponseDto()
                {
                    AvailableTasks = recommendedTasks?.OrderByDescending(x => x.TaskReward?.Priority).ToList(),
                    PendingTasks = allConsumerTasks.PendingTasks, // copy pending and completed from other API call
                    CompletedTasks = allConsumerTasks.CompletedTasks
                };
            }
            await FilterCostcoMembershipActionsAsync(tenant, consumerTaskResponseDto);
            return consumerTaskResponseDto;
        }


        private async Task<List<TaskRewardDetailDto>?> GetCohortRecommendedTask(GetConsumerRecommendedTasksRequestDto getConsumerRecommendedTasksRequestDto,
            List<TaskRewardDetailDto>? unfilteredAvailableTasks)
        {
            const string methodName = nameof(GetCohortRecommendedTask);
            var cohortResponse = await _cohortClient.Post<GetConsumerRecommendedTasksResponseDto>("cohort/consumer-recommended-tasks",
                getConsumerRecommendedTasksRequestDto);
            var taskRewards = cohortResponse.TaskRewards;

            var taskRewardCodes = taskRewards?.Select(tr => tr.TaskRewardCode ?? string.Empty).ToList() ?? new List<string>();

            _logger.LogInformation("{className}.{methodName}: Retrieved TaskRewardCodes Successfully for ConsumerCode : {ConsumerCode}, TenantCode : {TenantCode}", className, methodName,
                getConsumerRecommendedTasksRequestDto.ConsumerCode, getConsumerRecommendedTasksRequestDto.TenantCode);

            var recommendedTasks = unfilteredAvailableTasks?.Where(x => taskRewardCodes.Contains(x.TaskReward.TaskRewardCode)).OrderByDescending(x => x.TaskReward?.Priority).ToList();

            return recommendedTasks;
        }

        private async System.Threading.Tasks.Task FilterCostcoMembershipActionsAsync(TenantDto? tenant, ConsumerTaskResponseDto consumerTaskResponse)
        {
            if (consumerTaskResponse == null || CheckCostcoMemberhipSupport(tenant)) return;

            var membershipRewardTypeCode = await GetMembershipDollarsRewardType();
            if (membershipRewardTypeCode?.RewardTypeDto == null) return;

            var rewardTypeId = membershipRewardTypeCode.RewardTypeDto.RewardTypeId;
            consumerTaskResponse.AvailableTasks = FilterTasksByRewardType(consumerTaskResponse.AvailableTasks, rewardTypeId);
            consumerTaskResponse.PendingTasks = FilterTasksByRewardType(consumerTaskResponse.PendingTasks, rewardTypeId);
            consumerTaskResponse.CompletedTasks = FilterTasksByRewardType(consumerTaskResponse.CompletedTasks, rewardTypeId);
        }


        private static bool CheckCostcoMemberhipSupport(TenantDto? tenant)
        {
            if (tenant == null || string.IsNullOrEmpty(tenant.TenantAttribute)) return false;

            var tenantAttributes = JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute) ;
            if (tenantAttributes == null) return false;
            return tenantAttributes.CostcoMemberShipSupport;
        }

        /// <summary>
        /// Gets the type of the membership dollars reward.
        /// </summary>
        /// <returns></returns>
        private async Task<RewardTypeResponseDto> GetMembershipDollarsRewardType()
        {
            var rewardTypeCode = _configuration.GetValue<string>("MEMBERSHIP_DOLLARS_Reward_TypeCode") ?? string.Empty;
            return await _taskClient.Post<RewardTypeResponseDto>("reward-type-code", new RewardTypeCodeRequestDto
            {
                RewardTypeCode = rewardTypeCode
            });
        }

        /// <summary>
        /// Filters the type of the tasks by reward.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <param name="rewardTypeId">The reward type identifier.</param>
        /// <returns></returns>
        private List<TaskRewardDetailDto>? FilterTasksByRewardType(List<TaskRewardDetailDto>? tasks, long rewardTypeId)
        {
            return tasks?.Where(task => task.TaskReward?.RewardTypeId != rewardTypeId).ToList();
        }

    }
}




