using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers
{
    public class CommonTaskRewardService : ICommonTaskRewardService
    {
        private readonly ITaskCommonHelper _taskCommonHelper;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly ILogger<CommonTaskRewardService> _logger;
        private readonly IMapper _mapper;
        public CommonTaskRewardService(ITaskCommonHelper taskCommonHelper, IConsumerTaskRepo consumerTaskRepo, ILogger<CommonTaskRewardService> logger, IMapper mapper)
        {
            _taskCommonHelper = taskCommonHelper;
            _consumerTaskRepo = consumerTaskRepo;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// This method checks the task recurrence type base on the recurrence type it will assign the MinAllowedTaskCompleteTs
        /// </summary>
        /// <param name="taskRewardDetailDto">It will task the TaskRewardDetailsDto in that RecurrenceDefinitionJson which is having the recurrence details Serializing and processing</param>
        public async System.Threading.Tasks.Task RecurrenceTaskProcess(TaskRewardDetailDto taskRewardDetailDto)
        {
            if (string.IsNullOrEmpty(taskRewardDetailDto.TaskReward?.RecurrenceDefinitionJson))
            {
                return;
            }

            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(taskRewardDetailDto.TaskReward.RecurrenceDefinitionJson);

            if (recurrenceDetails == null) return;

            // Process periodic recurrence types
            if (recurrenceDetails.recurrenceType == Constant.Periodic && recurrenceDetails.periodic?.period != null)
            {
                // based on the period restart date and recurrence type (e.g., monthly, quarterly).
                var (periodStartDate, periodEndDate) = await _taskCommonHelper.GetPeriodStartAndEndDatesAsync(recurrenceDetails.periodic.periodRestartDate, recurrenceDetails.periodic.period);
                taskRewardDetailDto.MinAllowedTaskCompleteTs = periodStartDate;
                taskRewardDetailDto.ComputedTaskExpiryTs = periodEndDate;
            }
            // Process schedule recurrence type
            else if (recurrenceDetails.recurrenceType == Constant.Schedule)
            {
                var (scheduleStartDate, scheduleExpiryDate) = await _taskCommonHelper.FindMatchingScheduleStartDateAndExpiryDateAsync(recurrenceDetails.Schedules);
                if (scheduleStartDate == DateTime.MinValue)
                {
                    taskRewardDetailDto.MinAllowedTaskCompleteTs = taskRewardDetailDto.ConsumerTask?.TaskStartTs;
                }
                taskRewardDetailDto.MinAllowedTaskCompleteTs = scheduleStartDate;
                taskRewardDetailDto.ComputedTaskExpiryTs = scheduleExpiryDate;
            }

            taskRewardDetailDto.TaskReward!.MaxAllowedTaskCompletionTs = DateTime.UtcNow;
        }

        public async Task<List<TaskRewardDetailDto>> GetAvailableTasksAsync(List<TaskRewardDetailDto> taskRewardDetailDtos, TaskRewardCollectionRequestDto requestDto)
        {
            var consumerTasks = await _consumerTaskRepo.FindAsync(x =>
                                            x.ConsumerCode == requestDto.ConsumerCode && x.DeleteNbr == 0);

            var availableTaskList = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetailDtos, consumerTasks, _consumerTaskRepo, _logger);

            if (consumerTasks?.Any() == true)
            {
                var completedTasks = GetConsumerTasksByStatus(consumerTasks, Constants.Completed);
                var inProgressTasks = GetConsumerTasksByStatus(consumerTasks, Constants.InProgress);

                if (requestDto.IsIncludeCompletedTask)
                {
                    AddMatchingTaskRewards(completedTasks, taskRewardDetailDtos, availableTaskList);
                }

                AddMatchingTaskRewards(inProgressTasks, taskRewardDetailDtos, availableTaskList);
            }

            return availableTaskList;
        }

        private static List<ConsumerTaskModel> GetConsumerTasksByStatus(IEnumerable<ConsumerTaskModel> consumerTasks, string status)
        {
            return consumerTasks
                .Where(x => x.TaskStatus?.Equals(status, StringComparison.OrdinalIgnoreCase) == true && x.DeleteNbr == 0)
                .ToList();
        }

        private void AddMatchingTaskRewards(List<ConsumerTaskModel> consumerTasks, List<TaskRewardDetailDto> taskRewardDetailDtos, List<TaskRewardDetailDto> availableTaskList)
        {
            foreach (var consumerTask in consumerTasks)
            {
                var taskReward = taskRewardDetailDtos.FirstOrDefault(x => x?.TaskReward?.TaskId == consumerTask.TaskId);
                if (taskReward != null)
                {
                    var taskRewardClone = TaskHelper.DeepClone(taskReward);
                    taskRewardClone.ConsumerTaskDto = _mapper.Map<ConsumerTaskDto>(consumerTask);
                    availableTaskList.Add(taskRewardClone);
                }
            }
        }

    }
}

