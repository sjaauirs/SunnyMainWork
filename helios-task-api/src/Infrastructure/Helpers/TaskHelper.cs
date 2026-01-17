using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Globalization;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers
{
    public static class TaskHelper
    {
        const string className = nameof(TaskHelper);
        public static bool IsTaskCompletedInPreviousQuarter(DateTime completionDate, int? periodRestartDate)
        {
            DateTime currentDate = DateTime.UtcNow;
            if (completionDate > currentDate || periodRestartDate == null)
            {
                return false;
            }

            int completionQuarter = GetQuarter(completionDate, (int)periodRestartDate);
            int currentQuarter = GetQuarter(currentDate, (int)periodRestartDate);

            int completionYear = completionDate.Year;
            int currentYear = currentDate.Year;

            // Calculate the difference in quarters between the completion date and the current date
            int totalCompletionQuarters = ((completionYear - 1) * 4) + completionQuarter;
            int totalCurrentQuarters = ((currentYear - 1) * 4) + currentQuarter;
            int quartersDifference = totalCurrentQuarters - totalCompletionQuarters;

            return quartersDifference >= 1;
        }

        public static bool IsValidMonthlyReccurance(DateTime taskCompletionDate, PeriodicDto? periodicDto)
        {
            if (periodicDto?.periodRestartDate == null)
            {
                return false;
            }

            DateTime startOfCurrentRecurrence;
            DateTime endOfCurrentRecurrence;
            DateTime currentDate = DateTime.UtcNow;

            if (currentDate.Day >= periodicDto.periodRestartDate)
            {
                startOfCurrentRecurrence = new DateTime(currentDate.Year, currentDate.Month, periodicDto.periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
                endOfCurrentRecurrence = startOfCurrentRecurrence.AddMonths(1).AddTicks(-1);
            }
            else
            {
                startOfCurrentRecurrence = new DateTime(currentDate.Year, currentDate.Month, periodicDto.periodRestartDate, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
                endOfCurrentRecurrence = new DateTime(currentDate.Year, currentDate.Month, periodicDto.periodRestartDate, 0, 0, 0, DateTimeKind.Utc).AddTicks(-1);
            }

            bool isTaskCompletedInCurrentRecurrence = taskCompletionDate >= startOfCurrentRecurrence && taskCompletionDate < endOfCurrentRecurrence;

            // if tasknot completed within current occurnace then returning true
            bool isvalidMonthlyReccurance = !isTaskCompletedInCurrentRecurrence && periodicDto.period == Constant.Month && taskCompletionDate > DateTime.MinValue;

            return isvalidMonthlyReccurance;
        }

        public static int GetQuarter(DateTime givenDate, int periodRestartDate)
        {

            // Define the start dates for each quarter in UTC
            DateTime quarter1Start = new DateTime(givenDate.Year, 1, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            DateTime quarter2Start = new DateTime(givenDate.Year, 4, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            DateTime quarter3Start = new DateTime(givenDate.Year, 7, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            DateTime quarter4Start = new DateTime(givenDate.Year, 10, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);

            // Determine the quarter based on the given UTC date
            if (givenDate < quarter1Start)
                return 0;
            else if (givenDate >= quarter1Start && givenDate < quarter2Start)
                return 1;
            else if (givenDate >= quarter2Start && givenDate < quarter3Start)
                return 2;
            else if (givenDate >= quarter3Start && givenDate < quarter4Start)
                return 3;
            else
                return 4;
        }

        public static int GetCurrentMonthUtc(int periodRestartDate)
        {
            DateTime nowUtc = DateTime.UtcNow;

            if (nowUtc.Day < periodRestartDate)
            {
                if (nowUtc.Month == 1)
                {
                    return 12;
                }
                else
                {
                    return nowUtc.Month - 1;
                }
            }
            else
            {
                return nowUtc.Month;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskReward"></param>
        /// <param name="consumerTask"></param>
        /// <returns></returns>
        public static bool IsValidRecurring(TaskRewardModel taskReward, ConsumerTaskModel consumerTask)
        {
            if (consumerTask.TaskStatus.ToLower() == Constants.Completed.ToLower())
            {
                var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(taskReward.RecurrenceDefinitionJson ?? string.Empty);
                if (TaskHelper.IsValidMonthlyReccurance(consumerTask.TaskCompleteTs, recurrenceDetails?.periodic))
                {
                    return true;
                }

                // Verify quarterly recurrence
                else if (recurrenceDetails != null && recurrenceDetails.periodic?.period == Constant.QuarterlyPeriod &&
                    consumerTask.TaskCompleteTs > DateTime.MinValue &&
                    TaskHelper.IsTaskCompletedInPreviousQuarter(consumerTask.TaskCompleteTs, recurrenceDetails.periodic?.periodRestartDate))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// IsValidScheduleRecurring
        /// </summary>
        /// <param name="taskReward"></param>
        /// <param name="consumerTask"></param>
        /// <returns></returns>
        public static bool IsValidScheduleRecurring(RecurringDto? recurrenceDetails, bool isRecurring, ConsumerTaskModel consumerTask)
        {
            if (consumerTask.TaskStatus.Equals(Constants.Completed, StringComparison.CurrentCultureIgnoreCase) && isRecurring && recurrenceDetails != null)
            {
                if (recurrenceDetails?.recurrenceType?.ToUpper() == Constant.Schedule && recurrenceDetails.Schedules != null)
                {
                    var currentYear = DateTime.UtcNow.Year;
                    // Filter the schedules based on valid start and expiry dates
                    var validSchedules = recurrenceDetails.Schedules
                        .Where(s => s.StartDate != null && s.ExpiryDate != null)
                        .Select(s => new
                        {
                            StartDate = DateTime.SpecifyKind(
                                DateTime.ParseExact($"{currentYear}-{s.StartDate}", Constant.DateFormat, CultureInfo.InvariantCulture),
                                DateTimeKind.Utc),
                            ExpiryDate = DateTime.SpecifyKind(
                                DateTime.ParseExact($"{currentYear}-{s.ExpiryDate}", Constant.DateFormat, CultureInfo.InvariantCulture)
                                    .AddDays(1).AddMilliseconds(-1),
                                DateTimeKind.Utc)
                        });

                    // Current date
                    var currentDate = DateTime.UtcNow;

                    // Check if task complete date falls within any valid schedule range
                    if (validSchedules.Any(schedule =>
                        !(consumerTask.TaskCompleteTs >= schedule.StartDate && consumerTask.TaskCompleteTs <= schedule.ExpiryDate)
                        && (currentDate >= schedule.StartDate && currentDate <= schedule.ExpiryDate)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        

        /// <summary>
        /// VerifyTaskValidOccurrences
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="consumerTasks"></param>
        /// <param name="recurrenceDetails"></param>
        /// <returns></returns>
        public static bool VerifyTaskValidOccurrences(int taskId, IList<ConsumerTaskModel>? consumerTasks, RecurringDto? recurrenceDetails)
        {
            // If consumerTasks is null or if the recurrence type is null or not a recurring schedule,
            // return true, indicating that no validation is needed for such tasks.
            // Also return true if recurrenceDetails.periodic is null and the recurrence type is not a schedule.
            if (consumerTasks is null || recurrenceDetails?.recurrenceType is null || (recurrenceDetails.periodic is null &&
                !recurrenceDetails.recurrenceType.Equals(Constant.Schedule, StringComparison.CurrentCultureIgnoreCase)))
            {
                return true;
            }
            // If the recurrence type is a schedule, return false
            else if (recurrenceDetails.recurrenceType.Equals(Constant.Schedule, StringComparison.CurrentCultureIgnoreCase))
            {
                var consumerTask = consumerTasks.OrderByDescending(x => x.ConsumerTaskId).FirstOrDefault(x => x.TaskId == taskId);
                var isValid  =  IsValidScheduleRecurring( recurrenceDetails, true, consumerTask);
                return isValid;

            }

            // Calculate the start and end dates for the current recurrence period
            // based on the period restart date and recurrence period (e.g., monthly, quarterly).
            var (startDate, endDate) = GetPeriodStartAndEndDates(DateTime.UtcNow.Date, recurrenceDetails.periodic.periodRestartDate, recurrenceDetails.periodic.period);

            // Filter the consumerTasks list to:
            // - Match the provided taskId.
            // - Have a status of "Completed" (case-insensitive comparison).
            // - Have a completion timestamp within the specified start and end dates.
            int completedTaskCount = consumerTasks
                .Where(x => x.TaskId == taskId &&
                            x.TaskStatus.Equals(Constants.Completed, StringComparison.CurrentCultureIgnoreCase) &&
                            x.TaskCompleteTs >= startDate && x.TaskCompleteTs <= endDate)
                .Count();

            // Return true if the number of completed occurrences is less than the allowed MaxOccurrences,
            // indicating that adding a new occurrence is valid.
            return completedTaskCount < recurrenceDetails.periodic.MaxOccurrences;
        }

        /// <summary>
        /// GetPeriodStartAndEndDates
        /// </summary>
        /// <param name="periodRestartDate"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static (DateTime StartDate, DateTime EndDate) GetPeriodStartAndEndDates(DateTime currentDate, int periodRestartDate, string? period)
        {
            // Determine period length and start month based on recurrence period
            bool isQuarterly = string.Equals(period, Constant.QuarterlyPeriod, StringComparison.CurrentCultureIgnoreCase);
            int periodLength = isQuarterly ? 3 : 1;
            int startMonth = isQuarterly ? ((currentDate.Month - 1) / 3) * 3 + 1 : currentDate.Month;

            // Calculate start date within the determined start month, using the provided restart day
            int day = Math.Min(periodRestartDate, DateTime.DaysInMonth(currentDate.Year, startMonth));
            DateTime startDate = new DateTime(currentDate.Year, startMonth, day, 0, 0, 0, DateTimeKind.Utc);

            // Adjust to previous period if startDate is in the future
            if (startDate > currentDate)
                startDate = startDate.AddMonths(-periodLength);

            // End date is the last moment of the recurrence period
            DateTime endDate = startDate.AddMonths(periodLength).Date.AddTicks(-1);

            return (startDate, endDate);
        }

        /// <summary>
        /// // Deserialize recurrence details from JSON if available
        /// </summary>
        /// <param name="recurrenceJson"></param>
        /// <returns></returns>
        public static RecurringDto? DeserializeRecurrenceDetails(string? recurrenceJson)
        {
            return string.IsNullOrEmpty(recurrenceJson) ? null : JsonConvert.DeserializeObject<RecurringDto>(recurrenceJson);
        }

        /// <summary>
        /// // Ensure the period restart date is within valid bounds (1 to 28)
        /// </summary>
        /// <param name="recurrenceDetails"></param>
        /// <returns></returns>
        public static bool IsInvalidPeriodRestartDate(RecurringDto? recurrenceDetails)
        {
            return recurrenceDetails?.periodic?.periodRestartDate <= 0 || recurrenceDetails?.periodic?.periodRestartDate > 28;
        }

        /// <summary>
        /// ValidateParentTaskEligibility
        /// </summary>
        /// <param name="parentTaskAndReward"></param>
        /// <param name="consumerTasks"></param>
        /// <returns></returns>
        public static bool ValidateParentTaskEligibility(ConsumerTaskRewardModel? parentTaskAndReward, IList<ConsumerTaskModel>? consumerTasks, ILogger logger)
        {
            bool result = false;

            if (parentTaskAndReward != null && consumerTasks != null && !string.IsNullOrEmpty(parentTaskAndReward.ConsumerTask.TenantCode))
            {
                var parentConsumerTask = parentTaskAndReward.ConsumerTask;
                var parentTaskReward = parentTaskAndReward.TaskReward;

                // Deserialize recurrence details for the parent task if available
                var parentRecurrenceDetails = DeserializeRecurrenceDetails(parentTaskReward?.RecurrenceDefinitionJson);

                // Check if the parent task is valid, recurring, and has a specific recurrence type
                if (parentConsumerTask != null && parentTaskReward != null && parentTaskReward.IsRecurring && !string.IsNullOrEmpty(parentRecurrenceDetails?.recurrenceType) &&
                   !string.IsNullOrEmpty(parentRecurrenceDetails?.periodic?.period) && parentRecurrenceDetails.recurrenceType.Equals(Constant.Periodic, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Get all recent consumer tasks for the parent task
                    var parentCurrentConsumerTasks = consumerTasks.Where(x => x.TaskId == parentConsumerTask.TaskId).OrderByDescending(x => x.ConsumerTaskId).ToList();

                    // Validate that the period restart date of the parent task is within valid bounds
                    if (IsInvalidPeriodRestartDate(parentRecurrenceDetails))
                    {
                        logger.LogError("{className}.FilterAvailableCase: PeriodRestartDate Should Be Greater Than 0 and Less Than 29 For TenantCode:{TenantCode} and TaskId:{TaskId}", className, parentTaskAndReward.ConsumerTask.TenantCode, parentTaskAndReward.ConsumerTask.TaskId);
                        return result;
                    }
                    // Check if the task qualifies under monthly recurrence
                    else if (IsValidMonthlyReccurance(parentConsumerTask.TaskCompleteTs, parentRecurrenceDetails.periodic) ||
                             VerifyTaskValidOccurrences((int)parentConsumerTask.TaskId, parentCurrentConsumerTasks, parentRecurrenceDetails))
                    {
                        result = true;
                    }
                    // Check if the task qualifies under quarterly recurrence
                    else if (parentRecurrenceDetails.periodic.period == Constant.QuarterlyPeriod && parentConsumerTask.TaskCompleteTs > DateTime.MinValue &&
                             (IsTaskCompletedInPreviousQuarter(parentConsumerTask.TaskCompleteTs, parentRecurrenceDetails.periodic.periodRestartDate) ||
                              VerifyTaskValidOccurrences((int)parentConsumerTask.TaskId, parentCurrentConsumerTasks, parentRecurrenceDetails)))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public static bool HasValidSchedule(ScheduleDto[]? schedules)
        {
            if (schedules == null || schedules.Length == 0)
                return false;

            DateTime currentDateUtc = DateTime.UtcNow;

            return schedules.Any(schedule =>
            {
                try
                {
                    int currentYear = currentDateUtc.Year;

                    var startDate = DateTime.SpecifyKind(
                        DateTime.ParseExact($"{currentYear}-{schedule.StartDate}", Constant.DateFormat, CultureInfo.InvariantCulture),
                        DateTimeKind.Utc);

                    var expiryDate = DateTime.SpecifyKind(
                        DateTime.ParseExact($"{currentYear}-{schedule.ExpiryDate}", Constant.DateFormat, CultureInfo.InvariantCulture)
                                  .AddDays(1).AddMilliseconds(-1), // inclusive end of day
                        DateTimeKind.Utc);

                    return startDate <= currentDateUtc && currentDateUtc <= expiryDate;
                }
                catch (FormatException)
                {
                    return false;
                }
            });
        }


        /// <summary>
        /// FilterAvailableTasksAsync
        /// </summary>
        /// <param name="taskRewardDetails"></param>
        /// <param name="consumerTasks"></param>
        /// <param name="consumerTaskRepo"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task<List<TaskRewardDetailDto>> FilterAvailableTasksAsync(List<TaskRewardDetailDto> taskRewardDetails, IList<ConsumerTaskModel> consumerTasks, IConsumerTaskRepo consumerTaskRepo, ILogger logger)
        {
            // Sort consumer tasks by ConsumerTaskId in descending order (latest first) for easier access to the most recent tasks
            consumerTasks = consumerTasks.OrderByDescending(x => x.ConsumerTaskId).ToList();
            // Return the original taskRewardDetails if consumerTasks list is empty or null
            if (consumerTasks is null) return taskRewardDetails;

            // Extract TaskIds from the consumer tasks
            var consumerTaskIds = consumerTasks.Select(x => x.TaskId).ToList();

            // Filter tasks in taskRewardDetails that are not already in consumerTaskIds
            List<TaskRewardDetailDto> availableTasks = AvailableTasks(taskRewardDetails, consumerTaskIds);

            // Iterate through each task reward detail to check eligibility
            foreach (var taskRewardDetail in taskRewardDetails)
            {
                // Get all tasks matching the current task's TaskId and sort by the most recent
                var currConsumerTasks = consumerTasks.Where(x => x.TaskId == taskRewardDetail.Task.TaskId).OrderByDescending(x => x.ConsumerTaskId).ToList();
                // Skip if no matching consumer tasks exist
                if (currConsumerTasks == null || currConsumerTasks.Count == 0) continue;

                // Get the most recent consumer task
                var consumerTask = currConsumerTasks[0];

                // Check if the most recent task is completed, recurring, and meets certain conditions
                if (consumerTask.TaskStatus.Equals(Constants.Completed, StringComparison.CurrentCultureIgnoreCase) && taskRewardDetail.TaskReward != null &&
                    (taskRewardDetail.TaskReward.IsRecurring || consumerTask.ParentConsumerTaskId != null))
                {
                    // Deserialize recurrence details from JSON, if available
                    var recurrenceDetails = DeserializeRecurrenceDetails(taskRewardDetail.TaskReward.RecurrenceDefinitionJson);

                    // Check if the task has a valid recurring schedule
                    if (IsValidScheduleRecurring(recurrenceDetails, taskRewardDetail.TaskReward.IsRecurring, consumerTask))
                    {
                        availableTasks.Add(taskRewardDetail);
                    }
                    // Ensure the period restart date is within valid bounds (1 to 28)
                    else if (IsInvalidPeriodRestartDate(recurrenceDetails))
                    {
                        logger.LogError("{className}.FilterAvailableCase: PeriodRestartDate Should Be Greater Than 0 and Less Than 29 For TenantCode:{TenantCode} and TaskId:{TaskId}", className, taskRewardDetail?.TaskDetail?.TenantCode, taskRewardDetail?.TaskDetail?.TaskId);
                    }
                    // Check if the task is a recurring child task with a valid parent task
                    else if (consumerTask != null && !string.IsNullOrEmpty(consumerTask.TenantCode) && consumerTask.ParentConsumerTaskId != null)
                    {
                        // Fetch the parent task and reward details
                        var parentTaskAndReward = await consumerTaskRepo.GetConsumerTaskWithReward(consumerTask.TenantCode, (long)consumerTask.ParentConsumerTaskId, Constants.Completed);

                        if (ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, logger))
                        {
                            availableTasks.Add(taskRewardDetail);
                        }
                    }
                    // Validate monthly recurrence for the current task
                    else if (consumerTask?.TaskCompleteTs > DateTime.MinValue &&
                            IsValidMonthlyReccurance(consumerTask.TaskCompleteTs, recurrenceDetails?.periodic) ||
                            VerifyTaskValidOccurrences((int)taskRewardDetail.Task.TaskId, consumerTasks, recurrenceDetails))
                    {
                        availableTasks.Add(taskRewardDetail);
                    }
                    // Validate quarterly recurrence for the current task
                    else if (recurrenceDetails != null && recurrenceDetails.periodic?.period == Constant.QuarterlyPeriod && consumerTask?.TaskCompleteTs > DateTime.MinValue &&
                            (IsTaskCompletedInPreviousQuarter(consumerTask.TaskCompleteTs, recurrenceDetails.periodic?.periodRestartDate) ||
                            VerifyTaskValidOccurrences((int)taskRewardDetail.Task.TaskId, consumerTasks, recurrenceDetails)))
                    {
                        availableTasks.Add(taskRewardDetail);
                    }
                }
            }

            // Return the list of available tasks after filtering
            return availableTasks;
        }

        private static List<TaskRewardDetailDto> AvailableTasks(List<TaskRewardDetailDto> taskRewardDetails, List<long> consumerTaskIds)
        {
            if (taskRewardDetails == null || !taskRewardDetails.Any())
                return new List<TaskRewardDetailDto>();

            if (consumerTaskIds == null)
                consumerTaskIds = new List<long>();

            var availableTasks = new List<TaskRewardDetailDto>();

            foreach (var task in taskRewardDetails)
            {
                if (task?.Task == null || task.TaskReward == null)
                    continue;

                if (consumerTaskIds.Contains(task.Task.TaskId))
                    continue; 

                var recurrenceDetails = DeserializeRecurrenceDetails(task.TaskReward.RecurrenceDefinitionJson);
                if (recurrenceDetails == null || recurrenceDetails.recurrenceType != Constant.Schedule ||
                    HasValidSchedule(recurrenceDetails.Schedules))
                {
                    availableTasks.Add(task);
                }
            }
            return availableTasks;
        }

        public static T DeepClone<T>(T obj)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}
