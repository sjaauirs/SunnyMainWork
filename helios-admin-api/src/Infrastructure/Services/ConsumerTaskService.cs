using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Enums;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Net;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerTaskService : IConsumerTaskService
    {
        private static int RETRY_MIN_WAIT_MS = 5; // min amount of milliseconds to wait before retrying
        private static int RETRY_MAX_WAIT_MS = 50; // max amount of milliseconds to wait before retrying

        private readonly ILogger<ConsumerTaskService> _consumerTaskServiceLogger;
        private readonly IWalletClient _walletClient;
        private readonly ITaskClient _taskClient;
        private readonly ITenantClient _tenantClient;
        private readonly IUserClient _userClient;
        private readonly IConfiguration _config;
        private readonly IFisClient _fisClient;
        public readonly ICohortClient _cohortClient;
        private readonly ICohortConsumerTaskService _cohortConsumerTaskService;
        private readonly Random _random = new Random();
        private readonly NHibernate.ISession _session;
        private readonly ITaskCommonHelper _taskCommonHelper;
        private readonly ICmsClient _cmsClient;
        private readonly IWalletTypeService _walletTypeService;
        private readonly IEventService _eventService;
        const string className = nameof(ConsumerTaskService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskServiceLogger"></param>
        /// <param name="walletClient"></param>
        /// <param name="taskClient"></param>
        /// <param name="tenantClient"></param>
        /// <param name="userClient"></param>
        /// <param name="config"></param>
        public ConsumerTaskService(
            ILogger<ConsumerTaskService> consumerTaskServiceLogger,
            IWalletClient walletClient,
            ITaskClient taskClient,
            ITenantClient tenantClient,
            IUserClient userClient,
            IConfiguration config,
            IFisClient fisClient, ICohortConsumerTaskService cohortConsumerTaskService, NHibernate.ISession session,
            ICohortClient cohortClient, ITaskCommonHelper taskCommonHelper, ICmsClient cmsClient, IWalletTypeService walletTypeService, IEventService eventService)
        {
            _consumerTaskServiceLogger = consumerTaskServiceLogger;
            _walletClient = walletClient;
            _taskClient = taskClient;
            _tenantClient = tenantClient;
            _userClient = userClient;
            _config = config;
            _fisClient = fisClient;
            _cohortConsumerTaskService = cohortConsumerTaskService;
            _session = session;
            _cohortClient = cohortClient;
            _taskCommonHelper = taskCommonHelper;
            _cmsClient = cmsClient;
            _walletTypeService = walletTypeService;
            _eventService = eventService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskUpdateRequestDto"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskUpdateResponseDto> UpdateConsumerTask(TaskUpdateRequestDto taskUpdateRequestDto)
        {
            const string methodName = nameof(UpdateConsumerTask);
            try
            {
                var updatedTask = new ConsumerTaskUpdateResponseDto();
                string? consumerCode = null;
                ConsumerDto? consumerDetails = null;
                PersonDto? personDetails = null;
                TenantDto? tenant = null;
                if (string.IsNullOrEmpty(taskUpdateRequestDto.ConsumerCode))
                {
                    if (string.IsNullOrEmpty(taskUpdateRequestDto.PartnerCode) || string.IsNullOrEmpty(taskUpdateRequestDto.MemberId))
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: ConsumerCode, PartnerCode+MemNbr cannot be null/empty, Error Code:{errorCode}", className, methodName, StatusCodes.Status400BadRequest);
                        return CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid arguments");
                    }
                    else
                    {
                        // find tenant
                        tenant = await GetTenantByPartnerCode(taskUpdateRequestDto.PartnerCode);
                        if (tenant == null || tenant.TenantCode == null)
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: Tenant not found for partner: {partnerCode}, Error Code:{errorCode}", className, methodName, taskUpdateRequestDto.PartnerCode, StatusCodes.Status404NotFound);
                            return CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
                        }
                        else
                        {
                            // find consumer
                            var consumer = await GetConsumerByMemId(tenant.TenantCode, taskUpdateRequestDto.MemberId);
                            if (consumer == null || consumer.ConsumerCode == null)
                            {
                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer not found for tenant: {tenant}, memnbr: {memnbr}, Error Code:{errorCode}", className, methodName,
                                    tenant.TenantCode, taskUpdateRequestDto.MemberId, StatusCodes.Status404NotFound);
                                return CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
                            }
                            else
                            {
                                consumerCode = consumer.ConsumerCode;
                            }
                            consumerDetails = consumer;
                        }
                    }
                }
                else
                {
                    consumerCode = taskUpdateRequestDto.ConsumerCode;
                }

                string? tenantCode = null;
                if (tenant != null)
                {
                    tenantCode = tenant.TenantCode;
                }
                else
                {
                    // need to get tenantCode from Consumer to create/update Consumer Task
                    var consumerResponse = await GetConsumer(new BaseRequestDto { ConsumerCode = consumerCode });
                    if (consumerResponse.Consumer == null || consumerResponse.Consumer.ConsumerCode == null || consumerResponse.Consumer.TenantCode == null)
                    {
                        return CreateErrorResponse(HttpStatusCode.NotFound, consumerResponse.ErrorMessage);
                    }
                    consumerDetails = consumerResponse.Consumer;
                    tenantCode = consumerDetails.TenantCode;
                }
                //get consumer details by consumerCode
                if (consumerDetails == null)
                {
                    var consumerResponse = await GetConsumer(new BaseRequestDto { ConsumerCode = consumerCode });
                    if (consumerResponse.Consumer == null || consumerResponse.Consumer.ConsumerCode == null || consumerResponse.Consumer.TenantCode == null)
                    {
                        return CreateErrorResponse(HttpStatusCode.NotFound, consumerResponse.ErrorMessage);
                    }
                    consumerDetails = consumerResponse.Consumer;
                }

                var personResponse = await GetPersonByPersonId(consumerDetails.PersonId);
                if (personResponse == null || personResponse.PersonCode == null)
                {
                    return CreateErrorResponse(HttpStatusCode.NotFound, "Person not found");
                }
                personDetails = personResponse;

                long consumerTaskId = 0;
                string taskExternalCode = string.Empty;
                if (!string.IsNullOrEmpty(taskUpdateRequestDto.TaskName))
                {
                    taskExternalCode = new IdGenerator().GenerateIdentifier(taskUpdateRequestDto.TaskName);
                }

                // try getting ConsumerTask by calling Task Service
                var findConsumerTaskRequest = new FindConsumerTasksByIdRequestDto()
                {
                    ConsumerCode = consumerCode,
                    TaskId = taskUpdateRequestDto.TaskId,
                    TaskCode = taskUpdateRequestDto.TaskCode,
                    TenantCode = tenantCode,
                    TaskExternalCode = taskExternalCode,
                };

                // Find or auto-enroll consumer task
                var findConsumerTaskResp = await FindOrEnrollConsumerTask(taskUpdateRequestDto, findConsumerTaskRequest, consumerCode, tenantCode!);
                if (findConsumerTaskResp?.ConsumerTask == null)
                {
                    return CreateErrorResponse(HttpStatusCode.UnprocessableEntity, "Consumer Task not found or Failed to auto enroll task");
                }

                if (findConsumerTaskResp.ConsumerTask != null)
                {
                    // make sure current consumer task status is right if incoming is COMPLETED
                    if (taskUpdateRequestDto.TaskStatus?.ToLower() == Constants.Completed.ToLower() &&
                        findConsumerTaskResp.ConsumerTask.TaskStatus?.ToLower() != Constants.InProgress.ToLower())
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not in right state, consumerTaskId: {consumerTaskId}, curr-state: {currState},Error Code:{errorCode}", className, methodName,
                            findConsumerTaskResp.ConsumerTask.ConsumerTaskId, findConsumerTaskResp.ConsumerTask.TaskStatus, StatusCodes.Status409Conflict);
                        return CreateErrorResponse(HttpStatusCode.Conflict, "Incorrect consumer task state");
                    }

                    consumerTaskId = findConsumerTaskResp.ConsumerTask.ConsumerTaskId;
                }
                var tenantData = await GetTenantByCode(tenantCode!);
                var tenantAttribute = !string.IsNullOrEmpty(tenantData.TenantAttribute)
                    ? JsonConvert.DeserializeObject<TenantAttrs>(tenantData.TenantAttribute)
                    : new TenantAttrs();
                // Invoke Consumer Task PUT to Update the Consumer Task
                var consumerTaskDto = new UpdateConsumerTaskDto()
                {
                    ConsumerTaskId = consumerTaskId,
                    ConsumerCode = consumerCode ?? string.Empty,
                    TenantCode = tenantCode,
                    TaskId = taskUpdateRequestDto.TaskId,
                    TaskStatus = taskUpdateRequestDto.TaskStatus,
                    TaskCode = taskUpdateRequestDto.TaskCode ?? string.Empty,
                    TaskExternalCode = taskExternalCode,
                    SpinWheelTaskEnabled = tenantAttribute?.SpinWheelTaskEnabled == true,
                    TaskCompletionEvidenceDocument = taskUpdateRequestDto.TaskCompletionEvidenceDocument,
                    TaskCompleteTs = taskUpdateRequestDto.TaskCompletedTs,
                    ProgressDetail = findConsumerTaskResp?.ConsumerTask?.ProgressDetail,
                    SkipValidation = taskUpdateRequestDto.SkipValidation
                };
                var consumerDto = new ConsumerDto { ConsumerCode = consumerTaskDto.ConsumerCode, TenantCode = consumerTaskDto.TenantCode }; // Use the string as a key

                if (!taskUpdateRequestDto.SkipValidation)
                {
                    var taskCompletionValidation = await ValidateTaskCompletionDate(taskUpdateRequestDto, findConsumerTaskResp);

                    if (taskCompletionValidation != null || taskCompletionValidation?.ErrorCode != null)
                    {
                        return taskCompletionValidation;
                    }
                }

                if (taskUpdateRequestDto.TaskStatus?.ToLower() == Constants.Completed.ToLower())
                {
                    consumerTaskDto.Progress = 100;
                    if (findConsumerTaskResp.TaskRewardDetail != null || findConsumerTaskResp.TaskRewardDetail?.TaskReward != null)
                    {

                        bool isPreScriptCheckComplete = await _cohortConsumerTaskService.TaskCompletionPrePostScriptCheck(findConsumerTaskResp, consumerDto, nameof(ScriptTypes.TASK_COMPLETE_PRE));
                        if (!isPreScriptCheckComplete)
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: The Task Complete pre script encountered some error while execution", className, methodName);

                            return CreateErrorResponse(HttpStatusCode.NotModified, "Error: The Task Complete pre script encountered some error while execution");

                        }
                    }
                    else
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: unable to process Task Complete pre script", className, methodName);

                        return CreateErrorResponse(HttpStatusCode.NotModified, "Error: The Task Complete pre script not processed");
                    }
                }

                // Handle Image task

                if (taskUpdateRequestDto.Image != null)
                {
                    var responseDto = await HandleImageTask(taskUpdateRequestDto, consumerCode, tenantCode, findConsumerTaskResp, consumerTaskDto);
                    if (responseDto.ErrorCode != null)
                    {
                        return responseDto;
                    }
                }


                updatedTask.ConsumerTask = await _taskClient.PutFormData<ConsumerTaskDto>("update-consumer-task", consumerTaskDto);

                if (taskUpdateRequestDto?.TaskStatus?.ToLower() == Constants.Completed.ToLower())
                {
                    await _cohortConsumerTaskService.TaskCompletionPrePostScriptCheck(findConsumerTaskResp, consumerDto,
                        nameof(ScriptTypes.TASK_COMPLETE_POST), personDetails);

                }
                if (updatedTask.ConsumerTask.ConsumerTaskId <= 0)
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Could not create/update consumer task, tenantCode: {tenantCode}, consumerCode: {consumerCode}, taskId: {taskId}, taskCode: {taskCode}, taskStatus: {taskStatus}, Error Code:{errorCode}", className, methodName,
                        tenantCode, consumerCode, taskUpdateRequestDto?.TaskId, taskUpdateRequestDto?.TaskCode, taskUpdateRequestDto?.TaskStatus, StatusCodes.Status500InternalServerError);
                    return CreateErrorResponse(HttpStatusCode.InternalServerError, "Error create/update");
                }

                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Updated Consumer Task successfully for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName,
                    consumerTaskDto.ConsumerCode, consumerTaskDto.TenantCode);

                // Get Reward Amount
                var rewardAmount = JsonConvert.DeserializeObject<Reward>(findConsumerTaskResp.TaskRewardDetail?.TaskReward?.Reward ?? string.Empty);
                bool isMonetaryReward = false;
                bool isCostcoMembershipReward = false;
                List<PostRewardResponseDto> postRewardResponses = new List<PostRewardResponseDto>();
                List<ConsumerTaskRewardInfoDto> consumerTaskRewardInfos = new List<ConsumerTaskRewardInfoDto>();
                var splitWalletConfig = new TaskRewardWalletSplitConfigDto();

                // Check if the Task is Complete only then Invoke Wallet API to Reward the U ser and Increase Wallet Balance
                if (taskUpdateRequestDto?.TaskStatus?.ToLower() == Constants.Completed.ToLower())
                {
                    // if not already retrieved, Get ConsumerTask by Calling Task Service
                    // should be available by now due to auto-enrolment
                    if (findConsumerTaskResp.ConsumerTask == null)
                    {
                        findConsumerTaskResp = await _taskClient.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", findConsumerTaskRequest);
                        if (findConsumerTaskResp.ConsumerTask == null)
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not found, consumerCode: {consumerCode}, taskId: {taskId}, taskCode: {taskCode}, taskStatus: {taskStatus}, Error Code:{errorCode}", className, methodName,
                                findConsumerTaskRequest.ConsumerCode, findConsumerTaskRequest.TaskId, findConsumerTaskRequest.TaskCode, findConsumerTaskRequest.TaskStatus, StatusCodes.Status404NotFound);
                            return CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
                        }
                    }

                    var rewardTypeRequestDto = new RewardTypeRequestDto()
                    {
                        TaskId = taskUpdateRequestDto.TaskId,
                        TenantCode = tenantCode,
                        TaskCode = taskUpdateRequestDto.TaskCode,
                    };
                    var rewardTypeResponse = await _taskClient.Post<RewardTypeResponseDto>("reward-type", rewardTypeRequestDto);

                    var postRewardRequestDto = new PostRewardRequestDto()
                    {
                        TenantCode = tenantCode,
                        ConsumerCode = consumerCode,
                        TaskRewardCode = findConsumerTaskResp.TaskRewardDetail?.TaskReward?.TaskRewardCode,
                        RewardAmount = rewardAmount?.RewardAmount ?? 0,
                        RewardDescription = findConsumerTaskResp?.TaskRewardDetail?.TaskDetail?.TaskHeader
                    };
                    if (tenant == null && !string.IsNullOrEmpty(tenantCode))
                    {
                        tenant = tenantData ?? await GetTenantByCode(tenantCode);
                        var tenantAttrs = JsonConvert.DeserializeObject<TenantAttrs>(tenant.TenantAttribute);
                        postRewardRequestDto.SplitRewardOverflow = tenantAttrs?.ConsumerWallet?.SplitRewardOverflow ?? false;
                    }
                    if (rewardTypeResponse.RewardTypeDto != null)
                    {
                        var rewardTypeName = rewardTypeResponse.RewardTypeDto.RewardTypeName;
                        if (rewardTypeName == Constant.RewardTypeName_MONETARY_DOLLARS)
                        {
                           var isLiveTransferToRewardsPurseEnabled = CheckSupportLiveTransferToRewardsPurseflag(tenantData!);
                            splitWalletConfig = await _walletTypeService.GetTaskRewardMonetaryDollarWalletSplit(
                                findConsumerTaskResp?.TaskRewardDetail!.TaskReward!, consumerCode, tenantCode , isLiveTransferToRewardsPurseEnabled);
                            splitWalletConfig.WalletSplitConfig = await _walletTypeService.CreateMissingWalletsAsync(consumerCode, tenantCode,isLiveTransferToRewardsPurseEnabled,
                                splitWalletConfig.WalletSplitConfig);
                            isMonetaryReward = true;
                        }
                        else if (rewardTypeName == Constant.RewardTypeName_SWEEPSTAKES_ENTRIES)
                        {
                            splitWalletConfig = new TaskRewardWalletSplitConfigDto
                            {
                                WalletSplitConfig = new List<WalletSplitConfig>
                                {
                                    new WalletSplitConfig
                                    {
                                        WalletTypeCode = _config.GetSection("Sweepstakes_Entries_Wallet_Type_Code").Value,
                                        MasterWalletTypeCode = _config.GetSection("Sweepstakes_Entries_Wallet_Type_Code").Value,
                                        Percentage = 100
                                    }
                                }
                            };
                        }
                        else if (rewardTypeName == Constant.RewardTypeName_MEMBERSHIP_DOLLARS)
                        {
                            splitWalletConfig = new TaskRewardWalletSplitConfigDto
                            {
                                WalletSplitConfig = new List<WalletSplitConfig>
                                {
                                    new WalletSplitConfig
                                    {
                                        WalletTypeCode = _config.GetSection("Health_Actions_Membership_Reward_Wallet_Type_Code").Value,
                                        MasterWalletTypeCode= _config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value,
                                        RedemptionWalletTypeCode = GetCostcoRedemptionWalletTypeCode(),
                                        PurseWalletTypeCode = GetCostcoPurseWalletTypeCode(),
                                        Percentage = 100
                                    }
                                }
                            };
                            isCostcoMembershipReward = true;

                        }

                        foreach (var splitWallet in splitWalletConfig.WalletSplitConfig)
                        {
                            postRewardRequestDto.MasterWalletTypeCode = splitWallet.MasterWalletTypeCode;
                            postRewardRequestDto.ConsumerWalletTypeCode = splitWallet.WalletTypeCode;
                            postRewardRequestDto.RewardAmount = (rewardAmount?.RewardAmount ?? 0) * (splitWallet.Percentage / 100);
                            _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Initiating Wallet Reward, " +
                                "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, tenantCode: {tenantCode}, consumerCode: {consumerCode}, " +
                                "taskRewardCode: {taskRewardCode}, rewardAmount: {rewardAmount}", className, methodName,
                                postRewardRequestDto.MasterWalletTypeCode, postRewardRequestDto.ConsumerWalletTypeCode, postRewardRequestDto.TenantCode, postRewardRequestDto.ConsumerCode,
                                postRewardRequestDto.TaskRewardCode, postRewardRequestDto.RewardAmount);

                            var splitRewardResponse = await _walletClient.Post<PostRewardResponseDto>("wallet/reward", postRewardRequestDto);
                            postRewardResponses.Add(splitRewardResponse);
                            if (splitRewardResponse.ErrorMessage != null && splitRewardResponse.ErrorCode != (int?)HeliosErrorCode.ConsumerLimitReached)
                                break;
                            if (splitRewardResponse.ConsumerTaskRewardInfo != null)
                                consumerTaskRewardInfos.Add(splitRewardResponse.ConsumerTaskRewardInfo!);
                            splitWallet.TaskRewardCurrency = splitRewardResponse.ConsumerTaskRewardInfo?.Currency;
                        }

                        if (postRewardResponses.Where(x => (x.ErrorCode != null && x.ErrorCode != (int?)HeliosErrorCode.ConsumerLimitReached)
                        || x.TransactionDetail == null || x.TransactionDetail.TransactionDetailId <= 0).Count() > 0)
                        {
                            // Wallet error - so revert consumer task back to IN_PROGRESS
                            var consumerTask = findConsumerTaskResp.ConsumerTask;
                            consumerTask.TaskStatus = Constants.InProgress;
                            _ = await _taskClient.PutFormData<ConsumerTaskDto>("update-consumer-task", consumerTask);

                            var rewardResponse = postRewardResponses.FirstOrDefault(x => x.ErrorCode != null);

                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: Wallet Reward error, MasterWalletTypeCode: {masterWalletTypeCode}, ConsumerWalletTypeCode: {consumerWalletTypeCode}, tenantCode: {tenantCode}, consumerCode: {consumerCode}, taskRewardCode: {taskRewardCode}, rewardAmount: {rewardAmount},Error Code:{errorCode}", className, methodName,
                                postRewardRequestDto.MasterWalletTypeCode, postRewardRequestDto.ConsumerWalletTypeCode, postRewardRequestDto.TenantCode, postRewardRequestDto.ConsumerCode, postRewardRequestDto.TaskRewardCode, postRewardRequestDto.RewardAmount, rewardResponse.ErrorCode);

                            return new ConsumerTaskUpdateResponseDto()
                            {
                                ErrorCode = rewardResponse.ErrorCode,
                                ErrorMessage = rewardResponse.ErrorMessage
                            };
                        }
                        else
                        {
                            // Update consumerTask record with transaction code once the task is completed and rewarded
                            var consumerTask = findConsumerTaskResp.ConsumerTask;
                            consumerTask.WalletTransactionCode = string.Join(",", postRewardResponses.Select(x => x?.AddEntry?.TransactionCode));
                            consumerTask.RewardInfoJson = consumerTaskRewardInfos.Count > 0 ? JsonConvert.SerializeObject(consumerTaskRewardInfos) : null;
                            var response = await _taskClient.Put<BaseResponseDto>("update-consumer-task-details", consumerTask);
                            if (response.ErrorCode != null)
                            {
                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Error while updating the transaction code to consumer task, tenantCode: {tenantCode}, consumerCode: {consumerCode},Error Code:{errorCode}",
                                    className, methodName, postRewardRequestDto.TenantCode, postRewardRequestDto.ConsumerCode, response.ErrorCode);
                                return new ConsumerTaskUpdateResponseDto()
                                {
                                    ErrorCode = response.ErrorCode,
                                    ErrorMessage = response.ErrorMessage
                                };
                            }
                        }

                        _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Wallet Reward successful, " +
                            "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, tenantCode: {tenantCode}, consumerCode: {consumerCode}, " +
                            "taskRewardCode: {taskRewardCode}, rewardAmount: {rewardAmount}", className, methodName,
                            postRewardRequestDto.MasterWalletTypeCode, postRewardRequestDto.ConsumerWalletTypeCode, postRewardRequestDto.TenantCode, postRewardRequestDto.ConsumerCode,
                            postRewardRequestDto.TaskRewardCode, postRewardRequestDto.RewardAmount);
                    }
                    else
                    {
                        _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Wallet RewardType not found for given for TaskId.Task Code: {taskCode}, Task Id: {Task Id}," +
                            " Consumer Code:{consumerCode}, Error Code:{errorCode}", className, methodName, taskUpdateRequestDto.TaskCode, taskUpdateRequestDto.TaskId, postRewardRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    }
                }
                GetConsumerAccountRequestDto getConsumerAccountRequest = new GetConsumerAccountRequestDto
                {
                    ConsumerCode = consumerCode,
                    TenantCode = tenantCode
                };
                bool isValidConsumerAccount = await IsValidConsumerAccount(getConsumerAccountRequest);
                if (CheckSupportLiveTransferToRewardsPurseflag(tenantData!) && isMonetaryReward && isValidConsumerAccount)
                {
                    await TransferTaskRewardAmountToRewardPurse(tenantCode, consumerCode, rewardAmount, splitWalletConfig);
                }
                if (isValidConsumerAccount && isCostcoMembershipReward)
                {
                    _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Initiating transfer to Costco Purse for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                        className, methodName, consumerCode, tenantCode);
                    await TransferTaskRewardAmountToCostcoPurse(tenantCode!, consumerCode, rewardAmount, splitWalletConfig);
                }
                return updatedTask;
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        private static bool CheckSupportLiveTransferToRewardsPurseflag(TenantDto tenantDto)
        {

            var tenantOption = !string.IsNullOrEmpty(tenantDto.TenantOption)
                ? JsonConvert.DeserializeObject<TenantOption>(tenantDto.TenantOption)
                : new TenantOption();

            if (tenantOption?.Apps?.Any(x => string.Equals(x, Constant.Benefits, StringComparison.OrdinalIgnoreCase)) == true)
            {
                var tenantAttributes = !string.IsNullOrEmpty(tenantDto.TenantAttribute)
                    ? JsonConvert.DeserializeObject<TenantAttrs>(tenantDto.TenantAttribute)
                    : new TenantAttrs();
                return tenantAttributes?.SupportLiveTransferToRewardsPurse ?? false;
            }

            return false;
        }

        private async Task<ConsumerTaskUpdateResponseDto> HandleImageTask(TaskUpdateRequestDto taskUpdateRequestDto, string? consumerCode, string? tenantCode,
            FindConsumerTasksByIdResponseDto findConsumerTaskResp, UpdateConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(HandleImageTask);
            // Retrieve task reward details
            var taskReward = findConsumerTaskResp.TaskRewardDetail?.TaskReward;
            if (taskReward == null)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName}: Task reward is null with TenantCode:{Code},TaskId:{TaskId}", className, methodName, tenantCode,
                    findConsumerTaskResp?.ConsumerTask?.TaskId);
                return CreateErrorResponse(HttpStatusCode.UnprocessableEntity, "Task reward is null");
            }

            // De-serialize task completion criteria JSON if available
            var taskCompletionCreteria = !string.IsNullOrEmpty(taskReward.TaskCompletionCriteriaJson)
                ? JsonConvert.DeserializeObject<ImageTaskCompletionCriteraDto>(taskReward.TaskCompletionCriteriaJson)
                : new ImageTaskCompletionCriteraDto();

            // Validate that task completion criteria and image criteria are available
            if (taskCompletionCreteria == null || taskCompletionCreteria.ImageCriteria == null)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName}:Image task completion criteria is null for TenantCode:{Code},TaskId:{TaskId} ",
                    className, methodName, taskReward.TenantCode, taskReward.TaskId);
                return CreateErrorResponse(HttpStatusCode.UnprocessableEntity, "Image task completion criteria is null");
            }

            // De-serialize image task progress details if available
            var imageTaskProgress = !string.IsNullOrEmpty(findConsumerTaskResp?.ConsumerTask?.ProgressDetail)
                ? JsonConvert.DeserializeObject<ImageTaskProgressDto>(findConsumerTaskResp.ConsumerTask.ProgressDetail)
                : new ImageTaskProgressDto();

            string errorMessage = "Required image threshold is reached unable to process image";
            if (imageTaskProgress?.ImageCriteriaProgress?.UploadImageCount >= taskCompletionCreteria.ImageCriteria.RequiredImageCount)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName}:{Error} ", className, methodName, errorMessage);
                return CreateErrorResponse(HttpStatusCode.UnprocessableEntity, errorMessage);
            }

            // Process the uploaded image and update the task progress
            var uploadImageResponseDto = await ProcessUploadImage(consumerCode, tenantCode, taskUpdateRequestDto);
            if (uploadImageResponseDto.ErrorCode != StatusCodes.Status200OK)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName}:Error occurred uploading image for ConsumerCode:{ConsumerCode},TenantCode:{TenantCode} ERROR:{ERROR}",
                    className, methodName, consumerCode, tenantCode, uploadImageResponseDto.ErrorMessage);
                return CreateErrorResponse(StatusCodes.Status500InternalServerError, uploadImageResponseDto.ErrorMessage);
            }
            imageTaskProgress = UpdateImageTaskProgress(uploadImageResponseDto.ConsumerImage.ImagePath, imageTaskProgress);

            // Update the task progress detail with the new progress
            consumerTaskDto.ProgressDetail = JsonConvert.SerializeObject(imageTaskProgress);

            // If required images are uploaded, mark the task as Completed; otherwise, set it as InProgress
            if (imageTaskProgress?.ImageCriteriaProgress?.UploadImageCount == taskCompletionCreteria.ImageCriteria.RequiredImageCount)
            {
                consumerTaskDto.TaskStatus = Constants.Completed;
                consumerTaskDto.Progress = 100;
                taskUpdateRequestDto.TaskStatus = Constants.Completed;
                consumerTaskDto.TaskCompleteTs = DateTime.UtcNow;
            }
            else
            {
                consumerTaskDto.TaskStatus = (consumerTaskDto?.TaskStatus?.ToLower() == Constants.InProgress.ToLower()
                    ? consumerTaskDto.TaskStatus
                    : Constants.InProgress);
                var progress = (imageTaskProgress?.ImageCriteriaProgress.UploadImageCount /
                    (double)taskCompletionCreteria.ImageCriteria.RequiredImageCount) * 100;
                consumerTaskDto.Progress = (int)progress;
                consumerTaskDto.TaskCompleteTs = DateTime.MinValue;
            }
            return new ConsumerTaskUpdateResponseDto();
        }

        private async Task<FindConsumerTasksByIdResponseDto?> FindOrEnrollConsumerTask(TaskUpdateRequestDto request, FindConsumerTasksByIdRequestDto findConsumerTaskRequest, string consumerCode, string tenantCode)
        {
            const string className = nameof(ConsumerTaskService);
            const string methodName = nameof(FindOrEnrollConsumerTask);

            _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Finding or enrolling consumer task. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, TaskId: {TaskId}, TaskCode: {TaskCode}",
                className, methodName, consumerCode, tenantCode, request.TaskId, request.TaskCode);

            _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Sending request to fetch consumer task. TaskExternalCode: {TaskExternalCode}",
                className, methodName, findConsumerTaskRequest.TaskExternalCode);

            var response = await _taskClient.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", findConsumerTaskRequest);

            if (response?.ConsumerTask != null)
            {
                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Consumer task found. ConsumerTaskId: {ConsumerTaskId}",
                   className, methodName, response.ConsumerTask.ConsumerTaskId);
                return response;
            }
            else if (response?.ConsumerTask == null && request.IsAutoEnrollEnabled)
            {
                _consumerTaskServiceLogger.LogWarning("{className}.{methodName}: Consumer task not found for TaskId: {TaskId}, TaskCode: {TaskCode}.",
                    className, methodName, request.TaskId, request.TaskCode);
                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Auto-enroll is enabled. Enrolling consumer task for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}.",
                    className, methodName, consumerCode, tenantCode);

                var createConsumerTaskDto = new CreateConsumerTaskDto
                {
                    TaskId = request.TaskId,
                    TaskStatus = Constants.InProgress,
                    TenantCode = tenantCode,
                    ConsumerCode = consumerCode,
                    AutoEnrolled = true,
                    SkipValidation = request.SkipValidation
                };
                var createResponse = await PostConsumerTasks(createConsumerTaskDto);

                if (createResponse?.ConsumerTask?.ConsumerTaskId > 0)
                {
                    _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Consumer task successfully enrolled. Fetching newly created task.",
                        className, methodName);

                    return await _taskClient.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", findConsumerTaskRequest);
                }
                else
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Failed to enroll consumer task., ErrorCode: {ErrorCode}, ErrorMessage:{ErrorMessage}, Actual Task Update Request Data:{request}, Auto Enroll Request Data: {createConsumerTaskDto} ",
                        className, methodName, createResponse?.ErrorCode, createResponse?.ErrorMessage, request.ToJson(), createConsumerTaskDto.ToJson());
                }

            }
            else if (response?.ConsumerTask == null || response?.TaskRewardDetail == null)
            {
                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task found., ErrorCode: {ErrorCode}, Actual Task Update Request Data:{request}, Find Task Request Data: {createConsumerTaskDto} ",
                    className, methodName, HttpStatusCode.NotFound, request.ToJson(), findConsumerTaskRequest.ToJson());
            }


            return response;
        }

        private static ImageTaskProgressDto UpdateImageTaskProgress(string imagePath, ImageTaskProgressDto? imageTaskProgress)
        {
            if (imageTaskProgress == null || imageTaskProgress.ImageCriteriaProgress == null)
            {
                imageTaskProgress = new ImageTaskProgressDto()
                {
                    DetailType = Constant.ImageCriteria,
                    ImageCriteriaProgress = new ImageCriteriaProgressDto()
                    {
                        UploadImageCount = 1,
                        UploadImagePaths = new List<string>() { imagePath ?? string.Empty }
                    }
                };
            }
            else
            {
                imageTaskProgress.ImageCriteriaProgress.UploadImageCount += 1;
                imageTaskProgress.ImageCriteriaProgress.UploadImagePaths.Add(imagePath);
            }

            return imageTaskProgress;
        }

        private async Task<UploadImageResponseDto> ProcessUploadImage(string? consumerCode, string? tenantCode, TaskUpdateRequestDto taskUpdateRequestDto)
        {
            try
            {
                var uploadImageRequestDto = new UploadImageRequestDto()
                {
                    TenantCode = tenantCode ?? string.Empty,
                    ConsumerCode = consumerCode ?? string.Empty,
                    ImageName = taskUpdateRequestDto.ImageName,
                    ImageType = taskUpdateRequestDto.ImageType,
                    ImageFile = taskUpdateRequestDto.Image,
                };
                var uploadImageResponseDto = await _cmsClient.PostFormData<UploadImageResponseDto>(Constant.UploadImage, uploadImageRequestDto);
                return uploadImageResponseDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<ConsumerTaskUpdateResponseDto?> ValidateTaskCompletionDate(TaskUpdateRequestDto taskUpdateRequestDto, FindConsumerTasksByIdResponseDto consumerTaskResponse)
        {
            var taskCompleteDate = taskUpdateRequestDto.TaskCompletedTs?.Date;
            var validStartDate = consumerTaskResponse.TaskRewardDetail?.TaskReward?.ValidStartTs?.Date;
            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(consumerTaskResponse.TaskRewardDetail?.TaskReward?.RecurrenceDefinitionJson ?? string.Empty);
            var startDateOfRecurrence = await GetStartDateOfRecurrence(recurrenceDetails) ?? consumerTaskResponse.TaskRewardDetail?.ConsumerTask?.TaskStartTs?.Date;

            if (taskCompleteDate > DateTime.UtcNow.Date)
            {
                return LogAndCreateErrorResponse("TaskCompletedTs cannot be set to a future date.", taskUpdateRequestDto, taskCompleteDate, StatusCodes.Status422UnprocessableEntity);
            }
            if (consumerTaskResponse.TaskRewardDetail?.TaskReward?.IsRecurring == true && taskCompleteDate < startDateOfRecurrence)
            {
                return LogAndCreateErrorResponse("TaskCompleteTs should not be less than task recurrence start date.", taskUpdateRequestDto, taskCompleteDate, StatusCodes.Status422UnprocessableEntity);
            }
            if (consumerTaskResponse.TaskRewardDetail?.TaskReward?.IsRecurring == false && taskCompleteDate < validStartDate)
            {
                return LogAndCreateErrorResponse("TaskCompleteTs should not be less than task valid start date.", taskUpdateRequestDto, taskCompleteDate, StatusCodes.Status422UnprocessableEntity);
            }
            return null;
        }
        private async Task<DateTime?> GetStartDateOfRecurrence(RecurringDto? recurrenceDetails)
        {
            if (recurrenceDetails == null)
            {
                return null;
            }

            if (recurrenceDetails.recurrenceType == Constant.Periodic && recurrenceDetails.periodic?.period != null)
            {
                // based on the period restart date and recurrence type (e.g., monthly, quarterly).
                var (periodStartDate, _) = await _taskCommonHelper.GetPeriodStartAndEndDatesAsync(recurrenceDetails.periodic.periodRestartDate, recurrenceDetails.periodic.period);
                return periodStartDate;
            }
            else if (recurrenceDetails.Schedules != null && recurrenceDetails.recurrenceType == Constant.Schedule)
            {
                var (scheduleStartDate, _) = await _taskCommonHelper.FindMatchingScheduleStartDateAndExpiryDateAsync(recurrenceDetails.Schedules);
                return scheduleStartDate;
            }

            return null;
        }

        private ConsumerTaskUpdateResponseDto LogAndCreateErrorResponse(string message, TaskUpdateRequestDto taskUpdateRequestDto, DateTime? comparisonDate, int errorCode)
        {
            LogError(nameof(UpdateConsumerTask), message, errorCode, taskUpdateRequestDto, comparisonDate);
            return CreateErrorResponse(errorCode, message);
        }

        private void LogError(string methodName, string message, int errorCode, TaskUpdateRequestDto? taskUpdateRequestDto = null, DateTime? comparisonDate = null)
        {
            _consumerTaskServiceLogger.LogError("{className}.{methodName}: {message} TaskId: {taskId}, ConsumerTaskId: {consumerTaskId}, TaskCompleteTs: {taskCompleteTs}, ComparisonDate: {comparisonDate}, Error Code: {errorCode}",

                className, methodName, message, taskUpdateRequestDto?.TaskId, taskUpdateRequestDto?.TaskCode, taskUpdateRequestDto?.TaskCompletedTs?.Date, comparisonDate, errorCode);
        }

        private ConsumerTaskUpdateResponseDto CreateErrorResponse(int errorCode, string errorMessage)
        {
            return new ConsumerTaskUpdateResponseDto { ErrorCode = errorCode, ErrorMessage = errorMessage };
        }

        private async System.Threading.Tasks.Task TransferTaskRewardAmountToRewardPurse(string tenantCode, string? consumerCode,
            Reward? rewardAmount, TaskRewardWalletSplitConfigDto walletSplitConfig)
        {
            try
            {
                var redemptionRef = Guid.NewGuid().ToString("N");
                foreach (var splitWallet in walletSplitConfig.WalletSplitConfig)
                {
                    if (splitWallet.TaskRewardCurrency != Constant.Currency_USD)
                    {
                        _consumerTaskServiceLogger.LogWarning("{className}.UpdateTask.TransferTaskRewardAmountToRewardPurse: Skipping transfer to reward purse for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, " +
                            "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, RewardAmount: {RewardAmount} as currency is not USD",
                            className, consumerCode, tenantCode, splitWallet.MasterWalletTypeCode, splitWallet.WalletTypeCode,
                            rewardAmount);
                        continue;
                    }

                    if(splitWallet.PurseWalletTypeCode == null)
                    {
                        _consumerTaskServiceLogger.LogWarning(" {className} UpdateTask.TransferTaskRewardAmountToRewardPurse: Purse wallet type not set Skipping transfer to reward purse for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, " +
                            "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, RewardAmount: {RewardAmount} as PurseWalletTypeCode is null",
                            className, consumerCode, tenantCode, splitWallet.MasterWalletTypeCode, splitWallet.WalletTypeCode,
                            rewardAmount);
                        // raise a Event for missing purse wallet type code
                         var consumerErrorEventDto = new ConsumerErrorEventDto()
                        {
                            Header = new PostedEventData()
                            {
                                EventType = "CONSUMER_ERROR",
                                EventSubtype = "PURSE_WALLET_TYPE_CODE_NOT_FOUND"
                            },
                            Message = new ConsumerErrorEventBodyDto()
                            {
                                ReqDetail = splitWallet?.ToJson()??"",
                                Detail = $"{className}--TransferTaskRewardAmountToRewardPurse - Purse wallet type code not found for consumer {consumerCode} even after creation.",
                            }
                        };
                       
                        await _eventService.PostErrorEvent(consumerErrorEventDto);

                        continue;
                    }

                    var rewardAmountForSplit = (rewardAmount?.RewardAmount ?? 0) * (splitWallet.Percentage / 100);

                    _consumerTaskServiceLogger.LogInformation("{className}.UpdateTask: Initiating transfer to reward purse for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, " +
                        "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, RewardAmount: {RewardAmount}",
                        className, consumerCode, tenantCode, splitWallet.MasterWalletTypeCode, splitWallet.WalletTypeCode,
                        rewardAmountForSplit);

                    var postRedeemStartRequestDto = new Core.Domain.Dtos.PostRedeemStartRequestDto
                    {
                        ConsumerWalletTypeCode = splitWallet.WalletTypeCode,
                        RedemptionWalletTypeCode = splitWallet.RedemptionWalletTypeCode,
                        TenantCode = tenantCode,
                        ConsumerCode = consumerCode,
                        RedemptionVendorCode = splitWallet.RedemptionVendorCode,
                        RedemptionAmount = rewardAmountForSplit,
                        RedemptionRef = redemptionRef,
                        RedemptionItemDescription = Constant.RedemptionItemDescription_Wallet_name.Replace(Constant.WalletNamePlaceholder, splitWallet.WalletName),
                        Notes=Constant.LiveTransferRedemptionNotes
                    };
                    var redeemStartResponse = await _walletClient.Post<PostRedeemStartResponseDto>(Constant.WalletRedeemStartAPIUrl, postRedeemStartRequestDto);
                    if (redeemStartResponse.ErrorCode != null)
                    {
                        _consumerTaskServiceLogger.LogInformation("{className}.UpdateTask.TransferTaskRewardAmountToRewardPurse: An error occurred while redeeming wallet balance of Consumer: {ConsumerCode}, Error: {Message}, Error Code:{errorCode}", className,
                            postRedeemStartRequestDto.ConsumerCode, redeemStartResponse.ErrorMessage, redeemStartResponse.ErrorCode);
                        return;
                    }
                    var loadValueRequestDto = new LoadValueRequestDto
                    {
                        TenantCode = tenantCode,
                        ConsumerCode = consumerCode,
                        PurseWalletType = splitWallet.PurseWalletTypeCode,
                        Amount = rewardAmountForSplit,
                        Currency = Constant.Currency_USD,
                        MerchantName = SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant.MerchantNameForFundTransfer
                    };

                    var loadValueResponse = await PerformLoadValueWithRetries(loadValueRequestDto, consumerCode);

                    if (loadValueResponse == null || loadValueResponse.ErrorCode != null)
                    {
                        var postRedeemFailRequestDto = new PostRedeemFailRequestDto()
                        {
                            TenantCode = tenantCode,
                            ConsumerCode = consumerCode,
                            RedemptionVendorCode = postRedeemStartRequestDto.RedemptionVendorCode,
                            RedemptionAmount = rewardAmountForSplit,
                            RedemptionRef = redemptionRef,
                            Notes = Constant.LiveTransferRedemptionNotes
                        };
                        var redeemFailResponse = await _walletClient.Post<PostRedeemFailResponseDto>(Constant.WalletRedeemFailAPIUrl, postRedeemFailRequestDto);
                    }
                    if (loadValueResponse != null && (loadValueResponse?.ErrorCode == null || loadValueResponse?.ErrorCode == StatusCodes.Status200OK))
                    {
                        var postRedeemCompleteRequestDto = new PostRedeemCompleteRequestDto()
                        {
                            ConsumerCode = postRedeemStartRequestDto.ConsumerCode,
                            RedemptionVendorCode = postRedeemStartRequestDto.RedemptionVendorCode,
                            RedemptionRef = postRedeemStartRequestDto.RedemptionRef
                        };

                        var redeemSuccessResponse = await _walletClient.Post<PostRedeemCompleteResponseDto>(Constant.WalletRedeemCompleteAPIUrl, postRedeemCompleteRequestDto);
                        _consumerTaskServiceLogger.LogInformation("{className}.UpdateTask: Successfully redeem completed for Consumer: {ConsumerCode}", className, postRedeemStartRequestDto.ConsumerCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.UpdateTask: Error occurred while transferring reward amount to reward purse, ErrorMessage - {errorMessage}", className, ex.Message);
            }

        }

        private string GetHealthyLivingPurseWalletTypeCode()
        {
            return _config.GetSection("Healthy_Living_Wallet_Type_Code").Value;
        }

        private string GetHealthyLivingRedemptionWalletTypeCode()
        {
            return _config.GetSection("Healthy_Living_Redemption_Wallet_Type_Code").Value;
        }

        private async System.Threading.Tasks.Task TransferTaskRewardAmountToCostcoPurse(string tenantCode, string? consumerCode,
            Reward? rewardAmount, TaskRewardWalletSplitConfigDto walletSplitConfig)
        {
            const string methodName = nameof(TransferTaskRewardAmountToCostcoPurse);
            try
            {
                var redemptionRef = Guid.NewGuid().ToString("N");
                foreach (var splitWallet in walletSplitConfig.WalletSplitConfig)
                {
                    if (splitWallet.TaskRewardCurrency != Constant.Currency_USD)
                    {
                        _consumerTaskServiceLogger.LogWarning("{className}.UpdateTask.TransferTaskRewardAmountToRewardPurse: Skipping transfer to reward purse for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, " +
                            "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, RewardAmount: {RewardAmount} as currency is not USD",
                            className, consumerCode, tenantCode, splitWallet.MasterWalletTypeCode, splitWallet.WalletTypeCode,
                            rewardAmount);
                        continue;
                    }

                    var rewardAmountForSplit = (rewardAmount?.RewardAmount ?? 0) * (splitWallet.Percentage / 100);

                    _consumerTaskServiceLogger.LogInformation("{className}.UpdateTask: Initiating transfer to reward purse for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, " +
                        "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, RewardAmount: {RewardAmount}",
                        className, consumerCode, tenantCode, splitWallet.MasterWalletTypeCode, splitWallet.WalletTypeCode,
                        rewardAmountForSplit);

                    var postRedeemStartRequestDto = new Core.Domain.Dtos.PostRedeemStartRequestDto
                    {
                        ConsumerWalletTypeCode = splitWallet.WalletTypeCode,
                        RedemptionWalletTypeCode = splitWallet.RedemptionWalletTypeCode,
                        TenantCode = tenantCode,
                        ConsumerCode = consumerCode,
                        RedemptionVendorCode = Constant.RedemptionVendorCode_SuspenseWalletCostco,
                        RedemptionAmount = rewardAmountForSplit,
                        RedemptionRef = redemptionRef,
                        RedemptionItemDescription = Constant.CostcoRedemptionItemDescription_ValueLoad
                    };
                    var redeemStartResponse = await _walletClient.Post<PostRedeemStartResponseDto>(Constant.WalletRedeemStartAPIUrl, postRedeemStartRequestDto);
                    if (redeemStartResponse.ErrorCode != null)
                    {
                        _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: An error occurred while redeeming wallet balance of Consumer: {ConsumerCode}, Error: {Message}, Error Code:{errorCode}", className, methodName,
                            postRedeemStartRequestDto.ConsumerCode, redeemStartResponse.ErrorMessage, redeemStartResponse.ErrorCode);
                        return;
                    }
                    var loadValueRequestDto = new LoadValueRequestDto
                    {
                        TenantCode = tenantCode,
                        ConsumerCode = consumerCode,
                        PurseWalletType = splitWallet.PurseWalletTypeCode,
                        Amount = rewardAmountForSplit,
                        Currency = Constant.Currency_USD,
                        MerchantName = SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant.MerchantNameForFundTransfer
                    };

                    var loadValueResponse = await PerformLoadValueWithRetries(loadValueRequestDto, consumerCode);

                    if (loadValueResponse == null || loadValueResponse.ErrorCode != null)
                    {
                        var postRedeemFailRequestDto = new PostRedeemFailRequestDto()
                        {
                            TenantCode = tenantCode,
                            ConsumerCode = consumerCode,
                            RedemptionVendorCode = Constant.RedemptionVendorCode_SuspenseWalletCostco,
                            RedemptionAmount = rewardAmount?.RewardAmount ?? 0,
                            RedemptionRef = redemptionRef,
                            Notes = null
                        };
                        var redeemFailResponse = await _walletClient.Post<PostRedeemFailResponseDto>(Constant.WalletRedeemFailAPIUrl, postRedeemFailRequestDto);
                    }
                    if (loadValueResponse != null && (loadValueResponse?.ErrorCode == null || loadValueResponse?.ErrorCode == StatusCodes.Status200OK))
                    {
                        var postRedeemCompleteRequestDto = new PostRedeemCompleteRequestDto()
                        {
                            ConsumerCode = postRedeemStartRequestDto.ConsumerCode,
                            RedemptionVendorCode = postRedeemStartRequestDto.RedemptionVendorCode,
                            RedemptionRef = postRedeemStartRequestDto.RedemptionRef
                        };

                        var redeemSuccessResponse = await _walletClient.Post<PostRedeemCompleteResponseDto>(Constant.WalletRedeemCompleteAPIUrl, postRedeemCompleteRequestDto);
                        _consumerTaskServiceLogger.LogInformation("{className}.UpdateTask: Successfully redeem completed for Consumer: {ConsumerCode}", className, postRedeemStartRequestDto.ConsumerCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.UpdateTask: Error occurred while transferring reward amount to costco purse, ErrorMessage - {errorMessage}", className, ex.Message);
            }

        }

        public async Task<bool> IsValidConsumerAccount(GetConsumerAccountRequestDto request)
        {
            const string methodName = nameof(IsValidConsumerAccount);

            try
            {
                var response = await _fisClient.Post<GetConsumerAccountResponseDto>(SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant.GetConsumerAccount, request);

                if (response == null)
                {
                    _consumerTaskServiceLogger.LogError("{ClassName}.{Method}: FIS client returned null response for consumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
                    return false;
                }

                if (response.ErrorCode != null)
                {
                    _consumerTaskServiceLogger.LogError("{ClassName}.{Method}: FIS returned error code {ErrorCode} for consumerCode: {ConsumerCode}", className, methodName, response.ErrorCode, request.ConsumerCode);
                    return false;
                }

                var account = response.ConsumerAccount;
                if (account == null)
                {
                    _consumerTaskServiceLogger.LogError("{ClassName}.{Method}: Consumer account is null for consumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
                    return false;
                }

                if (string.IsNullOrEmpty(account.ProxyNumber) || account.ProxyNumber == SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant.ProxyNumberDefaultValue)
                {
                    _consumerTaskServiceLogger.LogInformation("{ClassName}.{Method}: Invalid proxy number '{ProxyNumber}' for consumerCode: {ConsumerCode}", className, methodName, account.ProxyNumber, request.ConsumerCode);
                    return false;
                }

                if (account.CardIssueStatus != CardIssueStatus.ISSUED.ToString())
                {
                    _consumerTaskServiceLogger.LogInformation("{ClassName}.{Method}: card is not Issued for consumerCode: {ConsumerCode}, card state : {CardIssueStatus}", className, methodName, request.ConsumerCode, account.CardIssueStatus);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{ClassName}.{Method}: Exception occurred while checking consumer account for consumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
                return false;
            }
        }


        private async Task<LoadValueResponseDto?> PerformLoadValueWithRetries(LoadValueRequestDto loadValueRequestDto, string? consumerCode)
        {
            var maxTries = Constant.MaxTries_Count;
            LoadValueResponseDto? loadValueResponse = null;
            while (maxTries > 0)
            {
                try
                {
                    loadValueResponse = await _fisClient.Post<LoadValueResponseDto>(Constant.FISValueLoadAPIUrl, loadValueRequestDto);
                    if (loadValueResponse.ErrorCode == null)
                    {
                        break;
                    }

                    _consumerTaskServiceLogger.LogError("{className}.PerformLoadValueWithRetries: Response ErrorCode: {errCode} in Load value retrying count left={maxTries}, ConsumerCode: {consumerCode}", className, loadValueResponse.ErrorCode, maxTries,
                        consumerCode);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
                catch (Exception ex)
                {
                    _consumerTaskServiceLogger.LogError(ex, "{className}.PerformLoadValueWithRetries: Error occurred while Load value, retrying count left={maxTries}", className, maxTries);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
            }

            return loadValueResponse;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="subtaskUpdateRequestDto"></param>
        /// <returns></returns>
        public async Task<UpdateSubtaskResponseDto> UpdateCompleteSubtask(SubtaskUpdateRequestDto subtaskUpdateRequestDto)
        {
            const string methodName = nameof(UpdateCompleteSubtask);
            try
            {
                var updatedSubtask = new UpdateSubtaskResponseDto();
                string? consumerCode = null;
                TenantDto? tenant = null;

                if (string.IsNullOrEmpty(subtaskUpdateRequestDto.ConsumerCode))
                {
                    if (string.IsNullOrEmpty(subtaskUpdateRequestDto.PartnerCode) || string.IsNullOrEmpty(subtaskUpdateRequestDto.MemId))
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: ConsumerCode, PartnerCode+MemNbr cannot be null/empty. Error Code:{errorCode}", className, methodName, StatusCodes.Status400BadRequest);
                        return CreateCompleteErrorResponse(HttpStatusCode.BadRequest, "Invalid arguments");
                    }
                    else
                    {
                        // find tenant
                        tenant = await GetTenantByPartnerCode(subtaskUpdateRequestDto.PartnerCode);
                        if (tenant == null || tenant.TenantCode == null)
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: Tenant not found for partner: {partnerCode}, Error Code:{errorCode}", className, methodName, subtaskUpdateRequestDto.PartnerCode, StatusCodes.Status404NotFound);
                            return CreateCompleteErrorResponse(HttpStatusCode.NotFound, "Not found");
                        }
                        else
                        {
                            // find consumer
                            var consumer = await GetConsumerByMemId(tenant.TenantCode, subtaskUpdateRequestDto.MemId);
                            if (consumer == null || consumer.ConsumerCode == null)
                            {
                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer not found for tenant: {tenant}, memnbr: {memnbr}, Error Code:{errorCode}", className, methodName,
                                    tenant.TenantCode, subtaskUpdateRequestDto.MemId, StatusCodes.Status404NotFound);
                                return CreateCompleteErrorResponse(HttpStatusCode.NotFound, "Not found");
                            }
                            else
                            {
                                consumerCode = consumer.ConsumerCode;
                            }
                        }
                    }
                }
                else
                {
                    consumerCode = subtaskUpdateRequestDto.ConsumerCode;
                }

                string? tenantCode = null;
                if (tenant != null)
                {
                    tenantCode = tenant.TenantCode;
                }
                else
                {
                    // need to get tenantCode from Consumer to create/update Consumer Task
                    var consumerResp = await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", new GetConsumerRequestDto
                    {
                        ConsumerCode = consumerCode
                    });
                    if (consumerResp.Consumer != null && consumerResp.Consumer.ConsumerCode != null && consumerResp.Consumer.TenantCode != null)
                    {
                        tenantCode = consumerResp.Consumer.TenantCode;
                    }
                    else
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer not found for consumerCode: {consumerCode}, Error Code:{errorCode}", className, methodName, consumerCode, StatusCodes.Status404NotFound);
                        return CreateCompleteErrorResponse(HttpStatusCode.NotFound, "Not found");
                    }
                }

                long consumerTaskId = 0;
                string taskExternalCode = string.Empty;
                if (!string.IsNullOrEmpty(subtaskUpdateRequestDto.TaskName))
                {
                    taskExternalCode = new IdGenerator().GenerateIdentifier(subtaskUpdateRequestDto.TaskName);
                }

                // try getting ConsumerTask by calling Task Service
                var findConsumerTaskRequest = new FindConsumerTasksByIdRequestDto()
                {
                    ConsumerCode = consumerCode,
                    TaskId = subtaskUpdateRequestDto.TaskId,
                    TaskCode = subtaskUpdateRequestDto.TaskCode,
                    TenantCode = tenantCode,
                    TaskExternalCode = taskExternalCode
                };
                var findConsumerTaskResp = await _taskClient.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", findConsumerTaskRequest);
                if (findConsumerTaskResp.ConsumerTask != null)
                {
                    // make sure current consumer task status is right if incoming is COMPLETED
                    if (subtaskUpdateRequestDto.TaskStatus?.ToLower() == Constants.Completed.ToLower() &&
                        findConsumerTaskResp.ConsumerTask.TaskStatus?.ToLower() != Constants.InProgress.ToLower())
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not in right state, consumerTaskId: {consumerTaskId}, curr-state: {currState}, Error Code:{errorCode}", className, methodName,
                            findConsumerTaskResp.ConsumerTask.ConsumerTaskId, findConsumerTaskResp.ConsumerTask.TaskStatus, StatusCodes.Status409Conflict);
                        return CreateCompleteErrorResponse(HttpStatusCode.Conflict, "Incorrect consumer task state");
                    }

                    consumerTaskId = findConsumerTaskResp.ConsumerTask.ConsumerTaskId;
                }

                // Invoke Consumer Task PUT to Update the Consumer Task
                var consumerTaskDto = new UpdateSubtaskRequestDto()
                {
                    ConsumerTaskId = consumerTaskId,
                    ConsumerCode = consumerCode ?? string.Empty,
                    TenantCode = tenantCode,
                    TaskId = subtaskUpdateRequestDto.TaskId,
                    TaskStatus = subtaskUpdateRequestDto.TaskStatus,
                    CompletedTaskId = subtaskUpdateRequestDto.CompleteConsumerTaskId ?? 0
                };
                if (subtaskUpdateRequestDto.TaskStatus?.ToLower() == Constants.Completed.ToLower())
                {
                    consumerTaskDto.Progress = 100;
                }

                var completeSubtaskResponse = await _taskClient.Put<UpdateSubtaskResponseDto>("complete-subtask", consumerTaskDto);

                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Updated CompleteSubtask successfully for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName,
                    consumerTaskDto.ConsumerCode, consumerTaskDto.TenantCode);

                // Check if the Task is Complete only then Invoke Wallet API to Reward the U ser and Increase Wallet Balance
                if (subtaskUpdateRequestDto.TaskStatus?.ToLower() == Constants.Completed.ToLower())
                {
                    // if not already retrieved, Get ConsumerTask by Calling Task Service
                    // should be available by now due to auto-enrolment
                    if (findConsumerTaskResp.ConsumerTask == null)
                    {
                        findConsumerTaskResp = await _taskClient.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", findConsumerTaskRequest);
                        if (findConsumerTaskResp.ConsumerTask == null)
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not found, " +
                                "consumerCode: {consumerCode}, taskId: {taskId}, taskCode: {taskCode}, taskStatus: {taskStatus}, Error Code:{errorCode}", className, methodName,
                                findConsumerTaskRequest.ConsumerCode, findConsumerTaskRequest.TaskId,
                                findConsumerTaskRequest.TaskCode, findConsumerTaskRequest.TaskStatus, StatusCodes.Status404NotFound);
                            return CreateCompleteErrorResponse(HttpStatusCode.NotFound, "Not found");
                        }
                    }

                    // Get Reward Amount
                    var rewardAmount = JsonConvert.DeserializeObject<Reward>(findConsumerTaskResp.TaskRewardDetail?.TaskReward?.Reward ?? string.Empty);

                    var rewardTypeRequestDto = new RewardTypeRequestDto()
                    {
                        TaskId = subtaskUpdateRequestDto.TaskId,
                        TenantCode = tenantCode
                    };
                    var rewardTypeResponse = await _taskClient.Post<RewardTypeResponseDto>("reward-type", rewardTypeRequestDto);
                    if (rewardTypeResponse.RewardTypeDto != null)
                    {
                        var rewardTypeName = rewardTypeResponse.RewardTypeDto.RewardTypeName;
                        var postRewardRequestDto = new PostRewardRequestDto()
                        {
                            TenantCode = tenantCode,
                            ConsumerCode = consumerCode,
                            TaskRewardCode = findConsumerTaskResp.TaskRewardDetail?.TaskReward?.TaskRewardCode,
                            RewardAmount = completeSubtaskResponse?.AdditionalAmount ?? 0,
                            RewardDescription = findConsumerTaskResp?.TaskRewardDetail?.TaskDetail?.TaskHeader
                        };
                        if (rewardTypeName == Constant.RewardTypeName_MONETARY_DOLLARS)
                        {
                            postRewardRequestDto.MasterWalletTypeCode = _config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value;
                            postRewardRequestDto.ConsumerWalletTypeCode = postRewardRequestDto.MasterWalletTypeCode;
                        }
                        else if (rewardTypeName == Constant.RewardTypeName_SWEEPSTAKES_ENTRIES)
                        {
                            postRewardRequestDto.MasterWalletTypeCode = _config.GetSection("Sweepstakes_Entries_Wallet_Type_Code").Value;
                            postRewardRequestDto.ConsumerWalletTypeCode = postRewardRequestDto.MasterWalletTypeCode;
                        }
                        else if (rewardTypeName == Constant.RewardTypeName_MEMBERSHIP_DOLLARS)
                        {
                            postRewardRequestDto.MasterWalletTypeCode = _config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value;
                            postRewardRequestDto.ConsumerWalletTypeCode = _config.GetSection("Health_Actions_Membership_Reward_Wallet_Type_Code").Value;
                        }
                        var rewardResponse = await _walletClient.Post<PostRewardResponseDto>("wallet/reward", postRewardRequestDto);
                        if (rewardResponse.ErrorCode != null || rewardResponse.TransactionDetail == null ||
                            rewardResponse.TransactionDetail.TransactionDetailId <= 0)
                        {
                            if (rewardResponse.ErrorCode != (int?)HeliosErrorCode.ConsumerLimitReached)
                            {
                                // Wallet error - so revert consumer task back to IN_PROGRESS
                                var consumerTask = findConsumerTaskResp.ConsumerTask;
                                consumerTask.TaskStatus = Constants.InProgress;
                                _ = await _taskClient.PutFormData<ConsumerTaskDto>("update-consumer-task", consumerTask);

                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Wallet Reward error, " +
                                    "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, tenantCode: {tenantCode}, consumerCode: {consumerCode}, " +
                                    "taskRewardCode: {taskRewardCode}, rewardAmount: {rewardAmount}, Error Code:{errorCode}", className, methodName,
                                    postRewardRequestDto.MasterWalletTypeCode, postRewardRequestDto.ConsumerWalletTypeCode, postRewardRequestDto.TenantCode, postRewardRequestDto.ConsumerCode,
                                    postRewardRequestDto.TaskRewardCode, postRewardRequestDto.RewardAmount, rewardResponse.ErrorCode);

                                return new UpdateSubtaskResponseDto()
                                {
                                    ErrorCode = rewardResponse.ErrorCode,
                                    ErrorMessage = rewardResponse.ErrorMessage
                                };
                            }
                        }
                        else
                        {
                            // Update consumerTask record with transaction code once the task is completed and rewarded
                            var consumerTask = findConsumerTaskResp.ConsumerTask;
                            consumerTask.WalletTransactionCode = rewardResponse?.AddEntry?.TransactionCode;
                            var response = await _taskClient.Put<BaseResponseDto>("update-consumer-task-details", consumerTask);
                            if (response.ErrorCode != null)
                            {
                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Error while updating the transaction code to consumer task, tenantCode: {tenantCode}, consumerCode: {consumerCode},Error Code:{errorCode}",
                                    className, methodName, postRewardRequestDto.TenantCode, postRewardRequestDto.ConsumerCode, response.ErrorCode);
                                return new UpdateSubtaskResponseDto()
                                {
                                    ErrorCode = response.ErrorCode,
                                    ErrorMessage = response.ErrorMessage
                                };
                            }
                        }

                        _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Wallet Reward sucessful, " +
                            "MasterWalletTypeCode: {MasterWalletTypeCode}, ConsumerWalletTypeCode: {ConsumerWalletTypeCode}, tenantCode: {tenantCode}, consumerCode: {consumerCode}, " +
                            "taskRewardCode: {taskRewardCode}, rewardAmount: {rewardAmount}", className, methodName,
                            postRewardRequestDto.MasterWalletTypeCode, postRewardRequestDto.ConsumerWalletTypeCode, postRewardRequestDto.TenantCode, postRewardRequestDto.ConsumerCode,
                            postRewardRequestDto.TaskRewardCode, postRewardRequestDto.RewardAmount);
                    }
                    updatedSubtask.AdditionalAmount = completeSubtaskResponse?.AdditionalAmount ?? 0;
                }
                return updatedSubtask;
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }
        public async Task<ConsumerTaskResponseUpdateDto> PostConsumerTasks(CreateConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(PostConsumerTasks);
            var consumerTask = new ConsumerTaskResponseUpdateDto();
            try
            {
                var getConsumerRequestDto = new GetConsumerRequestDto()
                {
                    ConsumerCode = consumerTaskDto.ConsumerCode ?? string.Empty,
                };

                var consumer = await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", getConsumerRequestDto);
                if (consumer.Consumer == null)
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer is Null. Tenant Code:{tenantCode}, Error Code:{errorCode}", className, methodName, consumerTaskDto.TenantCode, StatusCodes.Status404NotFound);
                    return consumerTask;
                }

                consumerTask = await _taskClient.Post<ConsumerTaskResponseUpdateDto>("consumer-task", consumerTaskDto);
                if (consumerTask?.ConsumerTask != null && consumerTask?.ConsumerTask?.ConsumerTaskId > 0)
                {
                    _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Retrieved ConsumerTasks Successfully for " +
                        "TenantCode : {TenantCode}, TaskId : {TaskId}", className, methodName, consumerTaskDto.TenantCode, consumerTaskDto.TaskId);

                    return consumerTask;
                }
                return consumerTask;
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        #region Private
        private async Task<TenantDto?> GetTenantByPartnerCode(string partnerCode)
        {
            var getTenantRequestDto = new GetTenantByPartnerCodeRequestDto()
            {
                PartnerCode = partnerCode,
            };
            var tenantResponse = await _tenantClient.Post<GetTenantByPartnerCodeResponseDto>("tenant/get-by-partner-code", getTenantRequestDto);
            _consumerTaskServiceLogger.LogInformation("Retrieved Tenant Data Successfully for TenantCode : {partnerCode}", partnerCode);

            return tenantResponse.Tenant;
        }
        public async Task<bool?> GetTenantByTenantCode(string tenantCode)
        {
            var flag = false;
            var getTenantRequestDto = new TenantDto()
            {
                TenantCode = tenantCode,
            };

            var tenantResponse = await _tenantClient.Post<TenantDto>("tenant/get-by-tenant-code", getTenantRequestDto);
            _consumerTaskServiceLogger.LogInformation("Retrieved Tenant Data Successfully for TenantCode : {partnerCode}", tenantCode);
            if (tenantResponse != null && tenantResponse.TenantCode != null)
            {
                var tenantAttribute = JsonConvert.DeserializeObject<TenantAttrs>(tenantResponse.TenantAttribute);
                flag = tenantAttribute.SpinWheelTaskEnabled;
            }
            return flag;
        }
        private async Task<ConsumerDto?> GetConsumerByMemId(string tenantCode, string memId)
        {
            var getConsumerRequestDto = new GetConsumerByMemIdRequestDto()
            {
                TenantCode = tenantCode,
                MemberId = memId
            };
            var consumerResponse = await _userClient.Post<GetConsumerByMemIdResponseDto>("consumer/get-consumer-by-memid", getConsumerRequestDto);
            _consumerTaskServiceLogger.LogInformation("Retrieved Consumer successfully for tenant: {tenantCode}, Memnbr: {memNbr}", tenantCode, memId);

            return consumerResponse.Consumer;
        }
        private static ConsumerTaskUpdateResponseDto CreateErrorResponse(HttpStatusCode errorCode, string errorMessage)
        {
            return new ConsumerTaskUpdateResponseDto()
            {
                ErrorCode = (int)errorCode,
                ErrorMessage = errorMessage
            };
        }
        private static UpdateSubtaskResponseDto CreateCompleteErrorResponse(HttpStatusCode errorCode, string errorMessage)
        {
            return new UpdateSubtaskResponseDto()
            {
                ErrorCode = (int)errorCode,
                ErrorMessage = errorMessage
            };
        }
        private string? GetRewardWalletTypeCode()
        {
            return _config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value;
        }
        private string? GetRewardRedemptionWalletTypeCode()
        {
            return _config.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value;
        }
        private string? GetRewardPurseWalletTypeCode()
        {
            return _config.GetSection("Health_Actions_Reward_Purse_Wallet_Type_Code").Value;
        }
        private string? GetCostcoPurseWalletTypeCode()
        {
            return _config.GetSection("Costco_Purse_Wallet_Type_Code").Value;
        }

        private string? GetMembershipWalletTypeCode()
        {
            return _config.GetSection("Health_Actions_Membership_Reward_Wallet_Type_Code").Value;
        }
        private string? GetCostcoRedemptionWalletTypeCode()
        {
            return _config.GetSection("Costco_Redemption_Wallet_Type_Code").Value;
        }


        #endregion


        public async Task<AvailableRecurringTaskResponseDto> GetAvailableRecurringTask(AvailableRecurringTasksRequestDto availableRecurringTasksRequestDto)
        {
            if (availableRecurringTasksRequestDto.TaskAvailabilityTs == null)
            {
                availableRecurringTasksRequestDto.TaskAvailabilityTs = DateTime.UtcNow;
            }
            var methodName = nameof(GetAvailableRecurringTask);

            var tenantResponse = await GetTenantByConsumerCode(availableRecurringTasksRequestDto.ConsumerCode);

            if (tenantResponse == null || tenantResponse.Tenant == null)
            {
                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Tenant Not Found for consumer Code : {ConsumerCode}", className, methodName, availableRecurringTasksRequestDto.ConsumerCode);
                return new AvailableRecurringTaskResponseDto()
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = "Invalid Cossumer"
                };
            }

            var tenant = tenantResponse.Tenant;

            if (availableRecurringTasksRequestDto.TenantCode != tenant.TenantCode)
            {
                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Tenant Mispatch for consumer: {ConsumerCode}", className, methodName, availableRecurringTasksRequestDto.ConsumerCode);
                return new AvailableRecurringTaskResponseDto()
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = "Tenant Mismatch"
                };
            }

            var consumerTaskRequestDto = new ConsumerTaskRequestDto()
            {
                ConsumerCode = availableRecurringTasksRequestDto.ConsumerCode,
                TenantCode = availableRecurringTasksRequestDto.TenantCode,
                LanguageCode = availableRecurringTasksRequestDto.LanguageCode
            };
            var allConsumerTasks = await _taskClient.Post<ConsumerTaskResponseDto>("get-all-consumer-tasks", consumerTaskRequestDto);
            _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Retrieved AllConsumerTasks Successfully for ConsumerCode : {ConsumerCode}, TenantCode : {TenantCode}", className, methodName,
                    consumerTaskRequestDto?.ConsumerCode, consumerTaskRequestDto?.TenantCode);

            if (!tenant.RecommendedTask)
            {
                allConsumerTasks.AvailableTasks = allConsumerTasks.AvailableTasks?.OrderByDescending(x => x.TaskReward?.Priority).ToList();
            }
            else
            {
                var getConsumerRecommendedTasksRequestDto = new GetConsumerRecommendedTasksRequestDto()
                {
                    TenantCode = availableRecurringTasksRequestDto.TenantCode,
                    ConsumerCode = availableRecurringTasksRequestDto.ConsumerCode
                };
                var recommendedTasks = await GetCohortRecommendedTask(getConsumerRecommendedTasksRequestDto, allConsumerTasks.AvailableTasks);
                allConsumerTasks.AvailableTasks = recommendedTasks?.OrderByDescending(x => x.TaskReward?.Priority).ToList();
            }

            if (allConsumerTasks == null || allConsumerTasks.AvailableTasks == null)
            {
                return new AvailableRecurringTaskResponseDto() { };
            }

            // filter allConsumerTasks
            var recurringTasks = allConsumerTasks.AvailableTasks.Where(x => x.TaskReward?.IsRecurring == true).ToList();

            var filterRecurringTask = FilterValidTaskRewards(recurringTasks, availableRecurringTasksRequestDto.TaskAvailabilityTs.Value);

            _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Retrieved AllConsumerTasks Successfully for ConsumerCode : {ConsumerCode}, TenantCode : {TenantCode}", className, methodName,
                    consumerTaskRequestDto?.ConsumerCode, consumerTaskRequestDto?.TenantCode);


            return new AvailableRecurringTaskResponseDto()
            {
                AvailableTasks = filterRecurringTask
            };
        }

        private async Task<GetTenantResponseDto> GetTenantByConsumerCode(string ConsumerCode)
        {
            const string methodName = nameof(GetTenantByConsumerCode);
            var Consumer = new BaseRequestDto()
            {
                ConsumerCode = ConsumerCode,
            };
            var consumerDto = await GetConsumer(Consumer);
            var tenantData = await GetTenantByCode(consumerDto?.Consumer?.TenantCode ?? string.Empty);
            _consumerTaskServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Tenant Data Successfully TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}",
                className, methodName, ConsumerCode, tenantData.TenantCode);

            return new GetTenantResponseDto()
            {
                Tenant = tenantData
            };
        }

        private async Task<GetConsumerResponseDto> GetConsumer(BaseRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(GetConsumer);
            var consumer = await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", consumerSummaryRequestDto);
            if (consumer.Consumer == null)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName} - Consumer Details Not Found For ConsumerCode : {ConsumerCode}", className, methodName, consumerSummaryRequestDto.ConsumerCode);
                return new GetConsumerResponseDto();
            }
            _consumerTaskServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Consumer Details Successfully For ConsumerCode : {ConsumerCode}", className, methodName, consumerSummaryRequestDto.ConsumerCode);

            return consumer;
        }

        private async Task<TenantDto> GetTenantByCode(string tenantCode)
        {
            const string methodName = nameof(GetTenantByTenantCode);
            var getTenantCodeRequestDto = new GetTenantCodeRequestDto()
            {
                TenantCode = tenantCode,
            };
            var tenantResponse = await _tenantClient.Post<TenantDto>("tenant/get-by-tenant-code", getTenantCodeRequestDto);
            if (tenantResponse.TenantCode == null)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName} - TenantDetails Not Found for TenantCode : {TenantCode}", className, methodName, getTenantCodeRequestDto.TenantCode);
                return new TenantDto();
            }
            _consumerTaskServiceLogger.LogInformation("Retrieved Tenant Successfully for TenantCode : {TenantCode}", getTenantCodeRequestDto.TenantCode);

            return tenantResponse;
        }

        private async Task<List<TaskRewardDetailDto>?> GetCohortRecommendedTask(GetConsumerRecommendedTasksRequestDto getConsumerRecommendedTasksRequestDto,
            List<TaskRewardDetailDto>? unfilteredAvailableTasks)
        {
            const string methodName = nameof(GetCohortRecommendedTask);
            var cohortResponse = await _cohortClient.Post<GetConsumerRecommendedTasksResponseDto>("cohort/consumer-recommended-tasks",
                getConsumerRecommendedTasksRequestDto);
            var taskRewards = cohortResponse.TaskRewards;

            var taskRewardCodes = taskRewards?.Select(tr => tr.TaskRewardCode ?? string.Empty).ToList() ?? new List<string>();

            _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Retrieved TaskRewardCodes Successfully for ConsumerCode : {ConsumerCode}, TenantCode : {TenantCode}", className, methodName,
                getConsumerRecommendedTasksRequestDto.ConsumerCode, getConsumerRecommendedTasksRequestDto.TenantCode);

            var recommendedTasks = unfilteredAvailableTasks?.Where(x => taskRewardCodes.Contains(x.TaskReward.TaskRewardCode)).OrderByDescending(x => x.TaskReward?.Priority).ToList();

            return recommendedTasks;
        }

        private List<TaskRewardDetailDto> FilterValidTaskRewards(List<TaskRewardDetailDto> taskRewardDtoList, DateTime taskAvailabilityTs)
        {
            var validTaskRewards = new List<TaskRewardDetailDto>();
            foreach (var item in taskRewardDtoList)
            {
                if (taskAvailabilityTs < item?.TaskReward?.ValidStartTs || taskAvailabilityTs > item?.TaskReward?.Expiry)
                {
                    continue;
                }
                validTaskRewards.Add(item);
            }
            return validTaskRewards;
        }

        public async Task<ConsumersByTaskIdResponseDto> GetConsumersByCompletedTask(GetConsumerTaskByTaskId requestDto)
        {
            const string methodName = nameof(GetConsumersByCompletedTask);
            try
            {
                _consumerTaskServiceLogger.LogInformation("{Class}.{Method}: Started fetching completed tasks for TaskId: {TaskId}, TenantCode: {TenantCode}",
                    nameof(className), methodName, requestDto.TaskId, requestDto.TenantCode);

                var taskResponse = await _taskClient.Post<PageinatedCompletedConsumerTaskResponseDto>(
                    "consumers-completing-taskId-in-range", requestDto);

                if (taskResponse.ErrorCode != null || taskResponse.CompletedTasks == null)
                {
                    _consumerTaskServiceLogger.LogWarning("{Class}.{Method}: No completed tasks found or error from taskClient. ErrorCode: {ErrorCode}",
                        nameof(className), methodName, taskResponse.ErrorCode);

                    return new ConsumersByTaskIdResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No completed tasks found for the given criteria."
                    };
                }

                var consumerCodes = taskResponse.CompletedTasks
                    .Select(x => x.ConsumerCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .ToList();

                if (consumerCodes == null || consumerCodes.Count == 0)
                {
                    _consumerTaskServiceLogger.LogWarning("{Class}.{Method}: No valid consumer codes found in completed tasks.",
                        nameof(className), methodName);

                    return new ConsumersByTaskIdResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No valid consumer codes found in the task data."
                    };
                }

                var consumerRequest = new GetConsumerByConsumerCodes
                {
                    TenantCode = requestDto.TenantCode,
                    ConsumerCodes = consumerCodes!
                };

                var consumerResponse = await _userClient.Post<ConsumersAndPersonsListResponseDto>(
                    "consumer/get-consumers-by-consumer-codes", consumerRequest);

                var consumerWithTaskLst = consumerResponse.ConsumerAndPersons.Select(cp => new ConsumerWithTask
                {
                    Consumer = cp.Consumer!,
                    Person = cp.Person!,
                    ConsumerTasks = taskResponse.CompletedTasks
                    .Where(task => task.ConsumerCode == cp.Consumer!.ConsumerCode)
                    .ToList()
                }).ToList();

                return new ConsumersByTaskIdResponseDto()
                {
                    consumerwithTask = consumerWithTaskLst,
                    totalconsumersTasks = taskResponse.TotalRecords

                };
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{Class}.{Method}: Exception occurred while processing request.",
                    nameof(className), methodName);

                throw new InvalidOperationException("Failed to get consumers by completed task.", ex);
            }
        }

        /// <summary>
        /// Get person by personId
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        private async Task<PersonDto> GetPersonByPersonId(long personId)
        {
            const string methodName = nameof(GetConsumer);
            var person = await _userClient.Get<PersonDto>($"person/{personId}", null);
            if (person == null || person.PersonId == 0)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName} - Person Details Not Found For personId : {personId}",
                    className, methodName, personId);
                return new PersonDto();
            }
            _consumerTaskServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Person Details Successfully For Person : {Person}",
                className, methodName, personId);

            return person;
        }

    }

}

