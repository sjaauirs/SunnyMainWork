using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class TaskCompletionCheckerService : ITaskCompletionCheckerService
    {
        private readonly ILogger<TaskCompletionCheckerService> _logger;
        private readonly IConsumerTaskRepo _consumerTaskRepo;



        private const string className = nameof(TaskCompletionCheckerService);

        public string? CurrentConsumerCode { get; set; }

        public TaskCompletionCheckerService(ILogger<TaskCompletionCheckerService> logger, IConsumerTaskRepo consumerTaskRepo)
        {
            _logger = logger;
            _consumerTaskRepo = consumerTaskRepo;

        }
        public bool CheckConsumerTaskCompleted(object tenantCode, object consumerCode, object taskExternalCode)
        {
            const string methodName = nameof(CheckConsumerTaskCompleted);
            try
            {
                if (string.IsNullOrWhiteSpace(consumerCode.ToString())) return false;
                if (string.IsNullOrWhiteSpace(CurrentConsumerCode)) return false;
                if (string.IsNullOrWhiteSpace(tenantCode.ToString())) return false;
                if (string.IsNullOrWhiteSpace(taskExternalCode.ToString())) return false;
                _logger.LogInformation("{ClassName}.{MethodName} - Started process to  CheckConsumerTaskCompleted={ConsumerCode}", className, methodName, consumerCode.ToString());

                var consumerTask = _consumerTaskRepo.GetConsumerTask(consumerCode.ToString(), tenantCode.ToString(), taskExternalCode.ToString()).Result;
                if (consumerTask != null)
                    return consumerTask.TaskStatus == AdminConstants.Completed && consumerCode.ToString() == CurrentConsumerCode;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while checking consumer status Details. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);

                return false;
            }
        }

        public bool CheckConsumerTaskEnrolled(object tenantCode, object consumerCode, object taskExternalCode)
        {
            const string methodName = nameof(CheckConsumerTaskEnrolled);
            try
            {
                if (string.IsNullOrWhiteSpace(consumerCode.ToString())) return false;
                if (string.IsNullOrWhiteSpace(CurrentConsumerCode)) return false;
                if (string.IsNullOrWhiteSpace(tenantCode.ToString())) return false;
                if (string.IsNullOrWhiteSpace(taskExternalCode.ToString())) return false;

                _logger.LogInformation("{ClassName}.{MethodName} - Started process to  CheckConsumerTaskEnrolled={ConsumerCode}", className, methodName, consumerCode.ToString());

                var consumerTask = _consumerTaskRepo.GetConsumerTask(consumerCode.ToString(), tenantCode.ToString(), taskExternalCode.ToString()).Result;
                if (consumerTask != null)
                    return consumerTask.TaskStatus == AdminConstants.Enrolled && consumerCode.ToString() == CurrentConsumerCode;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while checking consumer status Details. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);

                return false;
            }
        }
    }
}
