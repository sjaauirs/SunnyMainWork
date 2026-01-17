extern alias SunnyRewards_Task;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Util;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System.Globalization;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class ProcessRecurringTasksService : IProcessRecurringTasksService
    {
        private readonly ILogger<ProcessRecurringTasksService> _logger;
        private readonly IAdminClient _adminClient;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IConsumerTaskRepo _consumerTaskRepo;


        private const string className = nameof(ProcessRecurringTasksService);

        public ProcessRecurringTasksService(ILogger<ProcessRecurringTasksService> logger, IAdminClient adminClient, IConsumerRepo consumerRepo, IConsumerTaskRepo consumerTaskRepo)
        {
            _logger = logger;
            _adminClient = adminClient;
            _consumerRepo = consumerRepo;
            _consumerTaskRepo = consumerTaskRepo;

        }
        public async Task RecurringTaskCreationProcess(string tenant)
        {
            const string methodName = nameof(RecurringTaskCreationProcess);
            _logger.LogInformation("{ClassName}.{MethodName} - Started ProcessRecurringTasks ...", className, methodName);
            try
            {
                var consumers = await _consumerRepo.FindAsync(x => x.DeleteNbr == 0 && x.TenantCode == tenant);
                if (!consumers.Any())
                {
                    _logger.LogError("{ClassName}.{MethodName} - No Consumer found for tenantCode {tenant}", className, methodName, tenant);
                    return;
                }
                foreach (var consumer in consumers)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} -Starting process for CreateRecurringTask for Consumer {code} having tenantCode {tenant}", className, methodName, consumer.ConsumerCode,
                        tenant);

                    await CreateRecurringTask(tenant, consumer.ConsumerCode);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} Error while requesting  for CreateRecurringTask .,ErrorCode:{Code}, ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);

                return;
            }
        }
        private async Task<BaseResponseDto> CreateRecurringTask(string tenantCode, string consumerCode)
        {
            const string methodName = nameof(CreateRecurringTask);

            var customerRequestDto = new AvailableRecurringTasksRequestDto()
            {
                TenantCode = tenantCode,
                ConsumerCode = consumerCode,
                TaskAvailabilityTs = DateTime.UtcNow,
            };
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to get AvailableRecurringTask request dto: {dto} ...", className, methodName, customerRequestDto.ToJson());

                var response = await _adminClient.Post<AvailableRecurringTaskResponseDto>(Constants.RecurringTaskApi, customerRequestDto);

                if (!String.IsNullOrEmpty(response.ErrorCode) || response.AvailableTasks == null || response.AvailableTasks.Count <= 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} : Error Response  from AvailableRecurringTask  API For Request:{Request}.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, customerRequestDto.ToJson(), response.ErrorCode, response.ErrorMessage);
                    return new BaseResponseDto
                    {
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorMessage
                    };
                }
                var recurringtaskRecords = response.AvailableTasks.Select(x => x.TaskReward)?.Where(x => x.IsRecurring)?.ToList();

                var consumerTask = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == consumerCode, true);
                if (consumerTask == null || consumerTask.Count <= 0 || recurringtaskRecords == null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} : no consumer task or recurring task found for consumerCode:{Request}", className, methodName, consumerCode);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound.ToString(),
                        ErrorMessage = "no consumer task or recurring task found for consumer"
                    };
                }
                foreach (var task in recurringtaskRecords)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Finding consumer task existing record for a given recurringtask ", className, methodName);

                    var consumerTaskRecord = consumerTask?.Where(x => x.TaskId == task?.TaskId).OrderByDescending(x => x.TaskCompleteTs).FirstOrDefault();
                    if (consumerTaskRecord == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} : Error consumerTask not present for consumer:{Request}", className, methodName, consumerTask?.ToJson());
                        continue;
                    }
                    if (String.IsNullOrEmpty(task?.RecurrenceDefinitionJson))
                    {
                        _logger.LogError("{ClassName}.{MethodName} : Error RecurrenceDefinitionJson not present for taskreward:{Request}", className, methodName, task?.ToJson());
                        continue;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName} - Deserialising RecurrenceSettings for a given recurringtask ", className, methodName);

                    var recurrenceDefinition = JsonConvert.DeserializeObject<RecurrenceSettingsDto>(task.RecurrenceDefinitionJson);

                    if (recurrenceDefinition == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} : Error Unable to parse recurrenceDefinition  taskreward:{Request}", className, methodName, task?.ToJson());
                        continue;
                    }
                    int currentYear = DateTime.UtcNow.Year;
                    ConsumerTaskResponseUpdateDto consumerTaskResponse = new ConsumerTaskResponseUpdateDto();

                    if (recurrenceDefinition.Schedules != null)
                    {
                        foreach (var Schedule in recurrenceDefinition.Schedules)
                        {
                            _logger.LogInformation("{ClassName}.{MethodName} - Analysing request to CreateConsumerTask for Schedules ...", className, methodName);

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
                                if (currentDate >= startDate && currentDate < endDate)
                                {
                                    _logger.LogInformation("{ClassName}.{MethodName} :Moving for consumer task Creation for Schedules {start} and {enddate}", className, methodName, startDateString
                                        , endDateString);

                                    consumerTaskResponse = await CreateConsumerTask(task, consumerCode);
                                    if (consumerTaskResponse.ConsumerTask == null && consumerTaskResponse.ErrorCode != null)
                                    {
                                        _logger.LogError("{ClassName}.{MethodName} : Error Response  from CreateConsumerTask for recurrenceDefinition as Schedules For task reward:{Request} and consumer Code {code}" +
                                            ",ErrorCode:{Code}, ERROR: {Message}", className, methodName, task.ToJson(), consumerCode, response.ErrorCode, response.ErrorMessage);
                                    }

                                }
                                else
                                {
                                    _logger.LogError("{ClassName}.{MethodName} : recurrenceDefinition setting criteria is not fullfilled for given  Schedules {start} and {enddate}", className, methodName, startDateString
                                        , endDateString);
                                }
                            }

                        }
                    }
                    else if (recurrenceDefinition.Periodic != null)
                    {
                        int currentDate = DateTime.UtcNow.Day;
                        int currentMonth = DateTime.UtcNow.Month;
                        switch (recurrenceDefinition.Periodic.Period)
                        {
                            case Constants.MonthlyPeriodType:

                                _logger.LogInformation("{ClassName}.{MethodName} - Analysing request to get CreateConsumerTask for MonthlyPeriodType ...", className, methodName);

                                if (currentDate >= recurrenceDefinition.Periodic.PeriodRestartDate)
                                {
                                    _logger.LogInformation("{ClassName}.{MethodName} :Moving for consumer task Creation for monthly", className, methodName);

                                    if (consumerTaskRecord.TaskCompleteTs.Month == currentMonth && consumerTaskRecord.TaskCompleteTs.Year == currentYear)
                                    {
                                        if (consumerTaskRecord.TaskCompleteTs.Day < recurrenceDefinition.Periodic.PeriodRestartDate)
                                            consumerTaskResponse = await CreateConsumerTask(task, consumerCode);
                                        else
                                            _logger.LogError("{ClassName}.{MethodName} -consumer completed the task in same month", className, methodName);

                                    }
                                    else
                                        consumerTaskResponse = await CreateConsumerTask(task, consumerCode);
                                    if (consumerTaskResponse.ConsumerTask == null || consumerTaskResponse.ErrorCode != null)
                                    {
                                        _logger.LogError("{ClassName}.{MethodName} : Error Response  from CreateConsumerTask for recurrenceDefinition as MonthlyPeriodType" +
                                            " For task reward:{Request} and consumer Code {code}" +
                                            ",ErrorCode:{Code}, ERROR: {Message}", className, methodName, task.ToJson(), consumerCode, response.ErrorCode, response.ErrorMessage);
                                    }
                                }
                                else
                                {
                                    _logger.LogError("{ClassName}.{MethodName} : recurrenceDefinition setting criteria is not fullfilled", className, methodName);
                                }
                                break;

                            case Constants.QuarterlyPeriodType:
                                int currentQuarter = GetCurrentQuarter(DateTime.Now);

                                _logger.LogInformation("{ClassName}.{MethodName} - Analysing request to get CreateConsumerTask for QuarterlyPeriodType ...", className, methodName);

                                // Get the task's quarter
                                int taskQuarter = GetCurrentQuarter(consumerTaskRecord.TaskCompleteTs);
                                if (currentDate >= recurrenceDefinition.Periodic.PeriodRestartDate)
                                {
                                    _logger.LogInformation("{ClassName}.{MethodName} :Moving for consumer task Creation for QuarterlyPeriodType", className, methodName);

                                    if (taskQuarter == currentQuarter && consumerTaskRecord.TaskCompleteTs.Year == currentYear)
                                    {
                                        if (consumerTaskRecord.TaskCompleteTs.Day < recurrenceDefinition.Periodic.PeriodRestartDate)
                                            consumerTaskResponse = await CreateConsumerTask(task, consumerCode);
                                        else
                                            _logger.LogError("{ClassName}.{MethodName} -consumer completed the task in same quater", className, methodName);

                                    }
                                    else
                                        consumerTaskResponse = await CreateConsumerTask(task, consumerCode);
                                    if (consumerTaskResponse.ConsumerTask == null && consumerTaskResponse.ErrorCode != null)
                                    {
                                        _logger.LogError("{ClassName}.{MethodName} : Error Response  from CreateConsumerTask for recurrenceDefinition as QuarterlyPeriodType" +
                                            " For task reward:{Request} and consumer Code {code}" +
                                            ",ErrorCode:{Code}, ERROR: {Message}", className, methodName, task.ToJson(), consumerCode, response.ErrorCode, response.ErrorMessage);
                                    }
                                }
                                else
                                {
                                    _logger.LogError("{ClassName}.{MethodName} : recurrenceDefinition setting criteria is not fullfilled", className, methodName);
                                }
                                break;

                        }

                    }
                    else
                        _logger.LogError("{ClassName}.{MethodName} : recurrenceDefinition is invalid  for task reward:{Request}", className, methodName, task?.ToJson());


                }
                return new BaseResponseDto();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} Error while requesting  from AvailableRecurringTask API.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError.ToString(),
                    ErrorMessage = ex.Message
                };
            }

        }

        private static int GetCurrentQuarter(DateTime date)
        {
            int month = date.Month;
            if (month >= 1 && month <= 3)
                return 1; // Q1
            else if (month >= 4 && month <= 6)
                return 2; // Q2
            else if (month >= 7 && month <= 9)
                return 3; // Q3
            else
                return 4; // Q4
        }
        private async Task<ConsumerTaskResponseUpdateDto> CreateConsumerTask(TaskRewardDto rewardDto, string consumerCode)
        {
            const string methodName = nameof(CreateConsumerTask);

            var requestDto = new CreateConsumerTaskDto()
            {
                TaskId = rewardDto.TaskId,
                ConsumerCode = consumerCode,
                TaskStatus = Constants.TaskStatusInProgress,
                TenantCode = rewardDto.TenantCode,
                AutoEnrolled = true
            };

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to get CreateConsumerTask {dto} ...", className, methodName, requestDto.ToJson());

                var response = await _adminClient.Post<ConsumerTaskResponseUpdateDto>(Constants.CreateConsumerTask, requestDto);

                if (!String.IsNullOrEmpty(response.ErrorCode.ToString()) || response.ConsumerTask == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} : Error Response  from CreateConsumerTask  API For Request:{Request}.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, requestDto.ToJson(), response.ErrorCode, response.ErrorMessage);
                    return new ConsumerTaskResponseUpdateDto
                    {
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorMessage
                    };
                }
                _logger.LogInformation("{ClassName}.{MethodName} :consumer task created successfully For Request:{Request}, response: {response},ErrorCode:{Code}, ERROR: {Message}", className, methodName, requestDto.ToJson(), response.ConsumerTask.ToJson(), response.ErrorCode, response.ErrorMessage);

                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError("{ClassName}.{MethodName} : Error Response  from CreateConsumerTask  API For Request:{Request}, ERROR: {Message}", className, methodName, requestDto.ToJson(), ex.Message);
                return new ConsumerTaskResponseUpdateDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<ConsumerTaskResponseUpdateDto> CreateConsumerTask(CreateConsumerTaskDto requestDto)
        {
            const string methodName = nameof(CreateConsumerTask);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to get CreateConsumerTask {dto} ...", className, methodName, requestDto.ToJson());

                var response = await _adminClient.Post<ConsumerTaskResponseUpdateDto>(Constants.CreateConsumerTask, requestDto);

                if (!String.IsNullOrEmpty(response.ErrorCode.ToString()) || response.ConsumerTask == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} : Error Response  from CreateConsumerTask  API For Request:{Request}.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, requestDto.ToJson(), response.ErrorCode, response.ErrorMessage);
                    return new ConsumerTaskResponseUpdateDto
                    {
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorMessage
                    };
                }
                _logger.LogInformation("{ClassName}.{MethodName} :consumer task created successfully For Request:{Request}, response: {response},ErrorCode:{Code}, ERROR: {Message}", className, methodName, requestDto.ToJson(), response.ConsumerTask.ToJson(), response.ErrorCode, response.ErrorMessage);

                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError("{ClassName}.{MethodName} : Error Response  from CreateConsumerTask  API For Request:{Request}, ERROR: {Message}", className, methodName, requestDto.ToJson(), ex.Message);
                return new ConsumerTaskResponseUpdateDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }

        }
    }
}
