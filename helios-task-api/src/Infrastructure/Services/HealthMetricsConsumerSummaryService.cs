using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using System.Globalization;
using SunnyRewards.Helios.Common.Core.Extensions;
using Google.Api.Gax;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class HealthMetricsConsumerSummaryService : IHealthMetricsConsumerSummaryService
    {
        private readonly ILogger<HealthMetricsConsumerSummaryService> _logger;
        private readonly ITaskRewardRepo _taskRewardRepo;
        const string className = nameof(HealthMetricsConsumerSummaryService);
        public IDictionary<string, DateTime?> HealthMetricsQueryStartTsMap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="taskRewardRepo"></param>
        /// <param name="session"></param>
        public HealthMetricsConsumerSummaryService(ILogger<HealthMetricsConsumerSummaryService> logger,
        IMapper mapper, ITaskRewardRepo taskRewardRepo, NHibernate.ISession session)
        {
            _logger = logger;
            _taskRewardRepo = taskRewardRepo;
            HealthMetricsQueryStartTsMap = new Dictionary<string, DateTime?>();

        }
        public async Task<HealthMetricsSummaryDto> getHealthMetrics(string tenantCode)
        {
            const string methodName = nameof(getHealthMetrics);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - starting request to get health metrics ...", className, methodName);

                // Execute Steps Challenge Query
                var stepsQueryResult = await ExecuteHealthChallengeQuery(tenantCode, "Steps", criteriaJson =>
                    criteriaJson?.HealthCriteria?.HealthTaskType == Constant.HealthCriteriaStepsType &&
                    criteriaJson?.HealthCriteria?.RequiredSteps > 0);

                HealthMetricsQueryStartTsMap["STEPS"] = stepsQueryResult;

                // Execute Sleep Challenge Query
                var sleepQueryResult = await ExecuteHealthChallengeQuery(tenantCode, "Sleep", criteriaJson =>
                    criteriaJson?.HealthCriteria?.HealthTaskType == Constant.HealthCriteriaSleepType &&
                    criteriaJson?.HealthCriteria?.RequiredSleep?.MinSleepDuration > 0);

                HealthMetricsQueryStartTsMap["SLEEP"] = sleepQueryResult;

                return new HealthMetricsSummaryDto()
                {
                    HealthMetricsQueryStartTsMap = HealthMetricsQueryStartTsMap
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: ERROR Msg:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        private async Task<DateTime?> ExecuteHealthChallengeQuery(string tenantCode, string healthTaskType, Func<TaskCompletionCriteriaJson, bool> criteriaCheck)
        {
            const string methodName = nameof(ExecuteHealthChallengeQuery);
            _logger.LogInformation("{ClassName}.{MethodName} - Request to Calculate {HealthTaskType} for tenantCode: {Code} ...", className, methodName, healthTaskType, tenantCode);

            var taskReward = await _taskRewardRepo.FindAsync(x => x.TenantCode == tenantCode && x.TaskCompletionCriteriaJson != null && x.RecurrenceDefinitionJson != null && x.DeleteNbr == 0);

            if (taskReward != null)
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Request to Calculate {HealthTaskType} for task reward: {Code} ...", className, methodName, healthTaskType, taskReward.ToJson());

                foreach (var taskRewardDto in taskReward)
                {
                    var criteriaJson = JsonConvert.DeserializeObject<TaskCompletionCriteriaJson>(taskRewardDto.TaskCompletionCriteriaJson);
                    var recurrenceJson = JsonConvert.DeserializeObject<RecurringDto>(taskRewardDto.RecurrenceDefinitionJson);

                    if (criteriaJson != null && criteriaJson.CompletionCriteriaType == Constant.HealthCriteriaType && criteriaJson.HealthCriteria != null
                        && criteriaCheck(criteriaJson) && recurrenceJson != null)
                    {
                        return CalculateStartDateTime(recurrenceJson);
                    }

                    _logger.LogError("{ClassName}.{MethodName} - Request to Calculate {HealthTaskType} did not fulfill the criteria for task reward : {Code} ...", className, methodName, healthTaskType, taskReward.ToJson());
                }
            }

            _logger.LogError("{ClassName}.{MethodName} - Request to Calculate {HealthTaskType} encountered error as task reward data is null for tenantcode: {Code} ...", className, methodName, healthTaskType, tenantCode);
            return null;
        }

        private DateTime? CalculateStartDateTime(RecurringDto recurrenceDefinition)
        {
            const string methodName = nameof(CalculateStartDateTime);

            int currentYear = DateTime.UtcNow.Year;

            if (recurrenceDefinition.Schedules != null)
            {
                foreach (var Schedule in recurrenceDefinition.Schedules)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Analysing request to CalculateStartDateTime for Schedules ...", className, methodName);

                    if (!string.IsNullOrEmpty(Schedule?.StartDate) && !string.IsNullOrEmpty(Schedule?.ExpiryDate))
                    {
                        string startDateString = Schedule.StartDate;
                        string endDateString = Schedule.ExpiryDate;
                        DateTime currentDate = DateTime.ParseExact(DateTime.UtcNow.ToString("MM-dd-yyyy"), "MM-dd-yyyy", CultureInfo.InvariantCulture);
                        string sDate = $"{startDateString}-{currentYear}";
                        string eDate = $"{endDateString}-{currentYear}";

                        // Parse the combined string into a DateTime
                        DateTime startDate = DateTime.ParseExact(sDate, "MM-dd-yyyy", CultureInfo.InvariantCulture);
                        DateTime endDate = DateTime.ParseExact(eDate, "MM-dd-yyyy", CultureInfo.InvariantCulture);

                        return startDate;

                    }
                    _logger.LogError("{ClassName}.{MethodName} - Invalid  start and end date to CalculateStartDateTime for Schedules :{Schedule}", className, methodName, Schedule?.ToJson());


                }
            }
            else if (recurrenceDefinition.periodic != null && recurrenceDefinition.periodic.periodRestartDate > 0)
            {
                var currentDate = DateTime.UtcNow;
                int currentMonth = DateTime.UtcNow.Month;
                switch (recurrenceDefinition.periodic.period)
                {
                    case Constant.MonthlyPeriodType:

                        _logger.LogInformation("{ClassName}.{MethodName} - Analysing request to CalculateStartDateTime for MonthlyPeriodType ...", className, methodName);

                        var quarter1StartDate = new DateTime(currentYear, currentMonth, recurrenceDefinition.periodic.periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
                        return quarter1StartDate;


                    case Constant.QuarterlyPeriodType:
                        _logger.LogInformation("{ClassName}.{MethodName} - Analysing request to CalculateStartDateTime for QuarterlyPeriodType ...", className, methodName);

                        return GetQuarterlyRecurrenceStartDate(currentDate, recurrenceDefinition.periodic.periodRestartDate);
                }

            }
            _logger.LogError("{ClassName}.{MethodName} - Invalid  recurrenceDefinition to CalculateStartDateTime  :{recurrenceDefinition}", className, methodName, recurrenceDefinition?.ToJson());

            return null;

        }

        public static DateTime GetQuarterlyRecurrenceStartDate(DateTime givenDate, int periodRestartDate)
        {
            // Define the start dates for each quarter based on the given date's year and periodRestartDate
            var quarter1StartDate = new DateTime(givenDate.Year, 1, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var quarter2StartDate = new DateTime(givenDate.Year, quarter1StartDate.AddMonths(3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var quarter3StartDate = new DateTime(givenDate.Year, quarter2StartDate.AddMonths(3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var quarter4StartDate = new DateTime(givenDate.Year, quarter3StartDate.AddMonths(3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var lastquarterStartDate = new DateTime(quarter1StartDate.Year - 1, quarter1StartDate.AddMonths(-3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);


            // Determine the quarter based on the given date
            if (givenDate < quarter1StartDate)
            {

                // Before the first quarter of the year
                return lastquarterStartDate;
            }
            else if (givenDate >= quarter1StartDate && givenDate < quarter2StartDate)
            {

                // In the first quarter
                return quarter1StartDate;
            }
            else if (givenDate >= quarter2StartDate && givenDate < quarter3StartDate)
            {
                // In the second quarter
                return quarter2StartDate;
            }
            else if (givenDate >= quarter3StartDate && givenDate < quarter4StartDate)
            {
                // In the third quarter
                return quarter3StartDate;
            }
            else
            {

                // In the fourth quarter
                return quarter4StartDate;
            }


        }
    }
}
