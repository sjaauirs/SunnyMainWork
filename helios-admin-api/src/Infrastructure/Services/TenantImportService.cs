using AutoMapper;
using Google.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ImportTaskRewardDto = SunnyRewards.Helios.Task.Core.Domain.Dtos.ImportTaskRewardDto;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TenantImportService : ITenantImportService
    {
        public readonly ILogger<TenantImportService> _logger;
        private readonly IMapper _mapper;
        public readonly ITaskClient _taskClient;
        public readonly ICmsClient _cmsClient;
        public readonly IFisClient _fisClient;
        public readonly ISweepstakesClient _sweepstakesClient;
        private readonly ITenantClient _tenantClient;
        private readonly IS3Helper _s3Helper;
        private readonly IConfiguration _configuration;
        private readonly ITenantService _tenantService;
        private readonly ITenantAccountService _tenantAccountService;
        private readonly IWalletTypeTransferRuleService _walletTypeTransferRuleService;
        public readonly ICohortClient _cohortClient;
        private readonly IAdminService _adminService;
        private readonly ITaskService _taskService;
        private readonly IWalletTypeService _walletTypeService;
        private readonly IComponentService _componentService;
        private readonly IUserContextService _userContextService;


        public const string className = nameof(TaskService);
        public const int batchSize = 50;
        private ImportDto? jsonFileDto = null;

        public TenantImportService(ILogger<TenantImportService> logger, IMapper mapper, IS3Helper s3Helper,
            IConfiguration configuration, ITenantClient tenantClient, ITaskClient taskClient, ICmsClient cmsClient, ITenantService tenantService,
            ITenantAccountService tenantAccountService, ISweepstakesClient sweepstakesClient, ICohortClient cohortClient,
            IWalletTypeTransferRuleService walletTypeTransferRuleService, IFisClient fisClient,
            IAdminService adminService, ITaskService taskService, IWalletTypeService walletTypeService, IComponentService componentService, IUserContextService userContextService)
        {
            _logger = logger;
            _mapper = mapper;
            _s3Helper = s3Helper;
            _configuration = configuration;
            _tenantClient = tenantClient;
            _taskClient = taskClient;
            _cmsClient = cmsClient;
            _tenantService = tenantService;
            _tenantAccountService = tenantAccountService;
            _sweepstakesClient = sweepstakesClient;
            _cohortClient = cohortClient;
            _walletTypeTransferRuleService = walletTypeTransferRuleService;
            _adminService = adminService;
            _taskService = taskService;
            _walletTypeService = walletTypeService;
            _componentService = componentService;
            _fisClient = fisClient;
            _userContextService = userContextService;
        }
        public async Task<BaseResponseDto> TenantImport(TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(TenantImport);
            try
            {
                var allResponses = new List<BaseResponseDto>();

                _logger.LogInformation("{ClassName}.{MethodName}: Import process started for TenantCode: {TenantCode}", className, methodName, tenantImportRequestDto.tenantCode);

                string fileName = Path.GetFileName(tenantImportRequestDto.File.FileName);

                var s3Key = $"tenant_import/{tenantImportRequestDto.tenantCode}_{fileName}";
                bool uploaded = await _s3Helper.UploadFileToS3(GetAwsTmpS3BucketName(), tenantImportRequestDto.File, s3Key);
                if (uploaded)
                {

                    jsonFileDto = await _s3Helper.UnzipAndProcessJsonFromS3(s3Key, tenantImportRequestDto.tenantCode, GetAwsTmpS3BucketName());
                    if (jsonFileDto == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - No file to Import for tenent code: {TenantCode}, ErrorCode:{Code}", className, methodName, tenantImportRequestDto.tenantCode, StatusCodes.Status404NotFound);
                        return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Import File Not Found" };
                    }

                    #region Import Metadata tables
                    // Process metadata import first, as it is required for other imports
                    var metadataResponse = await ProcessMetadataImport(jsonFileDto.Metadata, tenantImportRequestDto);
                    if (metadataResponse.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}:metadata import failed. file name: {filename}, tenantCode:{tenantCode}",
                            className, methodName, fileName, tenantImportRequestDto.tenantCode);
                        return metadataResponse;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName}: Metadata import completed successfully for the request TenantCode: {TenantCode}",
                        className, methodName, tenantImportRequestDto.tenantCode);
                    #endregion

                    var tenantResponse = await ProcessTenantImportAsync(tenantImportRequestDto, jsonFileDto);
                    if (tenantResponse.ErrorCode != null)
                    {
                        return tenantResponse;
                    }
                    var enumOrder = Enum.GetNames(typeof(ImportOption))
                           .Select((name, index) => new { Name = name.ToUpper(), Index = index })
                           .ToDictionary(x => x.Name, x => x.Index);

                    // Sort the list based on the enum order
                    var sortedImportOption = tenantImportRequestDto.ImportOptions.OrderBy(x => enumOrder.ContainsKey(x.ToUpper()) ? enumOrder[x.ToUpper()] : int.MaxValue).ToList();
                    var taskResponse = new ImportTaskResponseDto();
                    var componentResponse = new ImportCmsResponseDto();
                    var questionnaireResponse = new BaseResponseDto();

                    foreach (var option in sortedImportOption)
                   {
                        switch (option.ToUpper())
                        {
                            case nameof(ImportOption.COHORT):
                                if (!tenantImportRequestDto.ImportOptions.Contains(ExportOption.TASK.ToString()) && jsonFileDto?.TaskData?.Data != null)
                                {
                                    taskResponse = await ProcessTaskImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                    if (taskResponse.ErrorCode != null && taskResponse.ErrorCode != StatusCodes.Status200OK && taskResponse.ErrorCode != StatusCodes.Status206PartialContent)
                                    {
                                        allResponses.Add(taskResponse);
                                        continue;
                                    }
                                }
                                var cohortResponse = await ProcessCohortImport(jsonFileDto.CohortData, tenantImportRequestDto);
                                allResponses.Add(cohortResponse);
                                continue;
                            case nameof(ImportOption.TASK):
                                taskResponse = await ProcessTaskImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                allResponses.Add(taskResponse);
                                var taskRewardCollectionResponse = await ProcessTaskRewardCollectionImport(jsonFileDto.TaskRewardCollectionData, tenantImportRequestDto, taskResponse.TaskRewardList);
                                allResponses.Add(taskRewardCollectionResponse);
                                if (!tenantImportRequestDto.ImportOptions.Contains(ExportOption.CMS.ToString()) && jsonFileDto?.CMSData?.Data != null)
                                {
                                    componentResponse = await ProcessCMSImport(jsonFileDto.CMSData, tenantImportRequestDto);
                                }
                                if (!tenantImportRequestDto.ImportOptions.Contains(ExportOption.QUESTIONNAIRE.ToString()) && jsonFileDto?.TaskData?.Data != null)
                                {
                                    questionnaireResponse = await ProcessQuestionnaireImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                }
                                allResponses.Add(componentResponse);
                                allResponses.Add(questionnaireResponse);
                                var adventureAndTenantAdventureResponse = await ProcessAdventuresAndTenantAdventureImport(jsonFileDto.AdventureAndTenantAdventureData, tenantImportRequestDto.tenantCode, componentResponse.ComponentList);
                                allResponses.Add(adventureAndTenantAdventureResponse);
                                continue;
                            case nameof(ImportOption.TRIVIA):
                                var triviaResponse = await ProcessTriviaImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                allResponses.Add(triviaResponse);
                                continue;
                            case nameof(ImportOption.CMS):
                                var cmsResponse = await ProcessCMSImport(jsonFileDto.CMSData, tenantImportRequestDto);
                                allResponses.Add(cmsResponse);
                                continue;
                            case nameof(ImportOption.FIS):
                                var fisResponse = await ProcessFisImport(jsonFileDto.TenantData, tenantImportRequestDto);
                                allResponses.Add(fisResponse);

                                 fisResponse = await ProcessFisTenantProgramImport(jsonFileDto.TenantData, tenantImportRequestDto);
                                allResponses.Add(fisResponse);
                                continue;
                            case nameof(ImportOption.SWEEPSTAKES):
                                var sweepstakesResponse = await ProcessSweepstakesImport(jsonFileDto.SweepstakesData, tenantImportRequestDto);
                                allResponses.Add(sweepstakesResponse);
                                continue;
                            case nameof(ImportOption.WALLET):
                                var walletResponse = await ProcessWalletImport(jsonFileDto.WalletData, tenantImportRequestDto);
                                allResponses.Add(walletResponse);
                                continue;
                            case nameof(ImportOption.ADMIN):
                                if (!tenantImportRequestDto.ImportOptions.Contains(ExportOption.TASK.ToString()) && jsonFileDto?.TaskData?.Data != null)
                                {
                                    taskResponse = await ProcessTaskImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                }
                                allResponses.Add(taskResponse);
                                var adminResponse = await ProcessAdminImport(jsonFileDto.AdminData, tenantImportRequestDto, taskResponse);
                                allResponses.Add(adminResponse);
                                continue;
                            case nameof(ImportOption.QUESTIONNAIRE):
                                questionnaireResponse = await ProcessQuestionnaireImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                allResponses.Add(questionnaireResponse);
                                continue;
                            case nameof(ImportOption.ALL):
                                var allImportResponse = await ProcessFisImport(jsonFileDto.TenantData, tenantImportRequestDto);
                                allResponses.Add(allImportResponse);
                                var allCmsResponse = await ProcessCMSImport(jsonFileDto.CMSData, tenantImportRequestDto);
                                allResponses.Add(allCmsResponse);
                                var taskImportResponse = await ProcessTaskImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                allResponses.Add(taskImportResponse);
                                allImportResponse = await ProcessTriviaImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                allResponses.Add(allImportResponse);
                                allImportResponse = await ProcessSweepstakesImport(jsonFileDto.SweepstakesData, tenantImportRequestDto);
                                allResponses.Add(allImportResponse);
                                allImportResponse = await ProcessCohortImport(jsonFileDto.CohortData, tenantImportRequestDto);
                                allResponses.Add(allImportResponse);
                                allImportResponse = await ProcessTaskRewardCollectionImport(jsonFileDto.TaskRewardCollectionData, tenantImportRequestDto, taskImportResponse.TaskRewardList);
                                allResponses.Add(allImportResponse);
                                allImportResponse = await ProcessAdventuresAndTenantAdventureImport(jsonFileDto.AdventureAndTenantAdventureData,
                                    tenantImportRequestDto.tenantCode, allCmsResponse.ComponentList);
                                allResponses.Add(allImportResponse);
                                allImportResponse = await ProcessWalletImport(jsonFileDto.WalletData, tenantImportRequestDto);
                                allResponses.Add(allImportResponse);
                                allImportResponse = await ProcessAdminImport(jsonFileDto.AdminData, tenantImportRequestDto, taskImportResponse);
                                allResponses.Add(allImportResponse);
                                allImportResponse = await ProcessQuestionnaireImport(jsonFileDto.TaskData, tenantImportRequestDto);
                                allResponses.Add(allImportResponse);


                                break;
                            default:
                                _logger.LogError("{ClassName}.{MethodName}:invalid Import option. file name: {filename}", className, methodName, fileName);

                                break;
                        }
                    }


                    if (allResponses.Any(x => x.ErrorCode != null || x.ErrorMessage != null))
                    {
                        return ProcessErrors(allResponses);
                    }
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName}: File upload fail to s3. file name: {filename}", className, methodName, fileName);
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "file not uploaded" };

                }

                _logger.LogInformation("{ClassName}.{MethodName}: Data Imported successfully, TenantCode: {TenantCode}", className, methodName, tenantImportRequestDto.tenantCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while Importing  Details. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
        private async Task<BaseResponseDto> ProcessAdventuresAndTenantAdventureImport(AdventureAndTenanImportJson adventureAndTenantAdventureData,
            string tenantCode, List<ImportComponentResponseDto> ComponentList)
        {
            const string methodName = nameof(ProcessAdventuresAndTenantAdventureImport);
            var finalResponse = new BaseResponseDto();
            var errorMessages = new List<string>();

            try
            {
                var allAdventures = adventureAndTenantAdventureData?.Data.Adventures ?? new List<AdventureDto>();
                var allTenantAdventures = adventureAndTenantAdventureData?.Data.TenantAdventures ?? new List<TenantAdventureDto>();

                if (!allAdventures.Any() || !allTenantAdventures.Any())
                {
                    _logger.LogError("{className}.{methodName}: No adventures or tenant adventures found to process.", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorMessage = "No adventures or tenant adventures provided."
                    };
                }

                // Replace old CmsComponentCode(adventureCmsComponentCode) with new ComponentCode from ComponentList
                if (ComponentList != null && ComponentList.Any())
                {
                    var componentMap = ComponentList
                        .Where(c => !string.IsNullOrWhiteSpace(c.OldComponentCode) && !string.IsNullOrWhiteSpace(c.ComponentCode))
                        .ToDictionary(c => c.OldComponentCode!, c => c.ComponentCode!, StringComparer.OrdinalIgnoreCase);

                    foreach (var adventure in allAdventures)
                    {
                        if (!string.IsNullOrWhiteSpace(adventure.CmsComponentCode) &&
                            componentMap.TryGetValue(adventure.CmsComponentCode, out var newComponentCode))
                        {
                            adventure.CmsComponentCode = newComponentCode;
                        }
                    }
                }

                // Collect unique CmsComponentCodes
                var allCmsComponentCodes = allAdventures
                    .Where(a => !string.IsNullOrWhiteSpace(a.CmsComponentCode))
                    .Select(a => a.CmsComponentCode!)
                    .Distinct()
                    .ToList();
                // Step 1: Group components by language
                var componentsByLanguage = ComponentList?.Where(x => x.ComponentCode != null && allCmsComponentCodes.Contains(x.ComponentCode))
                    .GroupBy(x => x.LanguageCode)?
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ComponentCode).Distinct().ToList())
                    ?? new Dictionary<string, List<string>>();

                // Step 2: Prepare the final CMS response object
                var cmsResponse = new GetComponentsResponseDto
                {
                    Components = new List<ComponentDto>()
                };

                // Step 3: Process each language separately
                foreach (var kvp in componentsByLanguage)
                {
                    var languageCode = kvp.Key;
                    var componentCodes = kvp.Value;

                    // Split component codes into batches
                    var codeBatches = componentCodes
                        .Select((code, index) => new { code, index })
                        .GroupBy(x => x.index / batchSize)
                        .Select(g => g.Select(x => x.code).ToList())
                        .ToList();

                    // Step 4: Send request per batch
                    foreach (var batch in codeBatches)
                    {
                        var request = new GetComponentsRequestDto
                        {
                            TenantCode = tenantCode,
                            ComponentCodes = batch,
                            LanguageCode = languageCode
                        };

                        var cmsLangResponse = await _cmsClient.Post<GetComponentsResponseDto>(
                            Constant.GetCmsComponents, request);

                        if (cmsLangResponse?.ErrorCode == null && cmsLangResponse?.Components?.Count > 0)
                        {
                            cmsResponse.Components.AddRange(cmsLangResponse.Components);
                        }
                        else
                        {
                            _logger.LogError("{className}.{methodName}: No valid component found for languagecode:{lang_code} and component codes:{comp_codes}."
                                , className, methodName, request.LanguageCode, request.ComponentCodes.ToJson());

                        }
                    }
                }
                var availableCmsCodes = cmsResponse?.Components?
                    .Where(c => !string.IsNullOrWhiteSpace(c.ComponentCode))
                    .Select(c => c.ComponentCode!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Filter adventures with valid or empty CmsComponentCode
                var validAdventures = allAdventures
                    .Where(a => string.IsNullOrWhiteSpace(a.CmsComponentCode) || availableCmsCodes.Contains(a.CmsComponentCode!))
                    .ToList();

                // Log invalid components
                var invalidComponents = allCmsComponentCodes.Except(availableCmsCodes).ToList();
                if (invalidComponents.Any())
                {
                    _logger.LogError("{className}.{methodName}: Invalid CMS component codes: {invalidComponents}",
                        className, methodName, string.Join(", ", invalidComponents));
                }

                if (!validAdventures.Any())
                {
                    _logger.LogWarning("{className}.{methodName}: No valid adventures found to import.", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "No valid adventures to import."
                    };
                }

                // Batch process adventures
                var adventureBatches = validAdventures
                    .Select((adv, index) => new { adv, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(g => g.Select(x => x.adv).ToList())
                    .ToList();

                foreach (var batch in adventureBatches)
                {
                    if (!batch.Any()) continue;

                    var importRequest = new ImportAdventureRequestDto
                    {
                        TenantCode = tenantCode,
                        Adventures = batch,
                        TenantAdventures = allTenantAdventures
                    };

                    var response = await _taskClient.Post<BaseResponseDto>(Constant.ImportAdventuresAndTenantAdventures, importRequest);

                    if (response?.ErrorCode != null)
                    {
                        var errorMessage = $"Batch failed. ErrorCode: {response.ErrorCode}, Message: {response.ErrorMessage}";
                        _logger.LogError("{className}.{methodName}: {errorMessage}", className, methodName, errorMessage);
                        errorMessages.Add(errorMessage);
                    }
                }

                if (errorMessages.Any())
                {
                    finalResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Exception occurred while processing import.", className, methodName);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Unexpected error occurred during import."
                };
            }
        }

        private async Task<BaseResponseDto> ProcessTaskRewardCollectionImport(TaskRewardCollectionImportJson taskRewardCollectionData, TenantImportRequestDto tenantImportRequestDto, List<ImportTaskRewardDto> taskRewardList)
        {
            const string methodName = nameof(ProcessTaskRewardCollectionImport);
            try
            {
                var finalResponse = new BaseResponseDto();
                var errorMessages = new List<string>();

                if (taskRewardCollectionData?.Data?.TaskRewardCollections == null ||
                    !taskRewardCollectionData.Data.TaskRewardCollections.Any() || taskRewardList == null || taskRewardList.Count == 0)
                {
                    var message = taskRewardCollectionData?.Data == null
                        ? "No data to process"
                        : "File does not contain Task Reward Collection data";

                    _logger.LogError("{ClassName}.{MethodName}: {Message}. Payload: {Payload}", className, methodName, message, taskRewardCollectionData?.ToJson());

                    return new BaseResponseDto
                    {
                        ErrorMessage = message
                    };
                }

                var allCollections = taskRewardCollectionData.Data.TaskRewardCollections;

                // Replace old TaskRewardCode with new TaskRewardCode from taskRewardList
                if (taskRewardList != null && taskRewardList.Any())
                {
                    var taskRewardMap = taskRewardList?.Where(x => !string.IsNullOrWhiteSpace(x.TaskRewardCode) && !string.IsNullOrWhiteSpace(x.NewRewardCode))
                                        .GroupBy(x => x.TaskRewardCode!, StringComparer.OrdinalIgnoreCase)
                                        .ToDictionary(g => g.Key, g => g.First().NewRewardCode!, StringComparer.OrdinalIgnoreCase)
                                         ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var collection in allCollections)
                    {
                        // Replace ParentTaskRewardCode
                        if (!string.IsNullOrWhiteSpace(collection.ParentTaskRewardCode) &&
                            taskRewardMap.TryGetValue(collection.ParentTaskRewardCode, out var newParentTaskRewardCode))
                        {
                            collection.ParentTaskRewardCode = newParentTaskRewardCode;
                        }

                        // Replace ChildTaskRewardCode
                        if (!string.IsNullOrWhiteSpace(collection.ChildTaskRewardCode) &&
                            taskRewardMap.TryGetValue(collection.ChildTaskRewardCode, out var newChildTaskRewardCode))
                        {
                            collection.ChildTaskRewardCode = newChildTaskRewardCode;
                        }
                    }
                }

                // Break into batches
                var batches = allCollections
                    .Select((item, index) => new { item, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(g => g.Select(x => x.item).ToList())
                    .ToList();

                foreach (var batch in batches)
                {
                    if (!batch.Any()) continue;

                    var request = new ImportTaskRewardCollectionRequestDto
                    {
                        TaskRewardCollections = batch
                    };

                    var response = await _taskClient.Post<BaseResponseDto>(Constant.ImportTaskRewardCollectionApiUrl, request);

                    if (response?.ErrorCode != null)
                    {
                        var error = $"Batch failed. ErrorCode: {response.ErrorCode}, Message: {response.ErrorMessage}";
                        _logger.LogError("{ClassName}.{MethodName}: {Error}", className, methodName, error);
                        errorMessages.Add(error);
                    }
                }

                if (errorMessages.Any())
                {
                    finalResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }


        private string GetAwsTmpS3BucketName()
        {
            return _configuration.GetSection("AWS:AWS_TMP_BUCKET_NAME").Value?.ToString() ?? "";
        }
        private static string CleanTaskName(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
            {
                return string.Empty;
            }
            // Convert to lowercase, remove whitespace, and remove all non-alphanumeric symbols
            string cleanedTaskName = taskName.ToLower();  // Convert to lowercase
            cleanedTaskName = Regex.Replace(cleanedTaskName, @"\s+", "");  // Remove whitespace
            cleanedTaskName = Regex.Replace(cleanedTaskName, @"\W", "");  // Remove non-alphanumeric characters

            return cleanedTaskName;
        }
        private async Task<ImportTaskResponseDto> ProcessTaskImport(TaskImportJson taskImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessTaskImport);

            try
            {
                var taskRewardsList = new List<ImportTaskRewardDto>();
                if (taskImport?.Data?.Task == null || taskImport?.Data?.Task.Count == 0 || taskImport?.Data?.TaskDetail == null || taskImport?.Data?.TaskDetail.Count == 0
                    || taskImport?.Data?.TaskReward == null && taskImport?.Data?.TaskReward.Count == 0 || taskImport == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No task or taskReward or TaskDetails data found in the file. Data: {taskImport}",
                        className, methodName, taskImport?.ToJson());

                    return new ImportTaskResponseDto
                    {
                        ErrorMessage = "File does not contain task or taskReward or TaskDetails data"
                    };
                }
                var uniqueTasks = taskImport.Data.Task.Where(task => task?.Task != null && task?.Task.TaskId != null && task?.Task.TaskName != null)
                 .GroupBy(task => CleanTaskName(task.Task.TaskName))
                 .Select(group => group.OrderByDescending(t => t.Task?.TaskId).FirstOrDefault())
                 .ToList();
                var taskRewardDetailDtos = uniqueTasks

                    .Select(task => new ImportTaskRewardDetailDto
                    {
                        Task = new ExportTaskDto
                        {
                            Task = task.Task,
                            TaskTypeCode = task.TaskTypeCode,
                            TaskCategoryCode = task.TaskCategoryCode
                        },
                        TaskDetail = taskImport.Data.TaskDetail?.Where(detail => detail.TaskId == task.Task?.TaskId
                        && !string.IsNullOrEmpty(detail.LanguageCode)).ToList(),
                        TaskReward = taskImport.Data.TaskReward?
                            .Where(reward => reward.TaskReward?.TaskId == task.Task?.TaskId)
                            .Select(reward => new TaskRewardDto.ExportTaskRewardDto
                            {
                                TaskReward = reward.TaskReward,
                                TaskRewardTypeCode = reward.TaskRewardTypeCode
                            })
                            .FirstOrDefault()
                    }).OrderBy(x => x.Task?.Task?.TaskId)
                    .ToList();

                if (!taskRewardDetailDtos.Any())
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: No valid task rewards found for import.", className, methodName);
                    return new ImportTaskResponseDto
                    {
                        ErrorCode = StatusCodes.Status204NoContent,
                        ErrorMessage = "No valid task rewards found"
                    };
                }
                var taskRewardDetailBatches = taskRewardDetailDtos
                    .Select((task, index) => new { task, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(group => group.Select(x => x.task).ToList())
                    .ToList();


                var tenantTaskCategoryBatches = taskImport.Data.TenantTaskCategory
                    .Select((task, index) => new { task, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(group => group.Select(x => x.task).ToList())
                    .ToList();
                var termsOfServiceLookup = taskImport.Data.TermsOfService
                    .Where(x => x != null)
                    .ToDictionary(x => x.TermsOfServiceId, x => x);

                var errorMessages = new List<string>();
                for (int i = 0; i < taskRewardDetailBatches.Count; i++)
                {
                    var batch = taskRewardDetailBatches[i];
                    batch = ImportTaskPayloadForSubtask(taskRewardDetailDtos, batch, taskImport).Item1;
                    var SubTasks = ImportTaskPayloadForSubtask(taskRewardDetailDtos, batch, taskImport).Item2;
                    var taskrewardExternalCodeInBatch = batch.Select(x => x.TaskReward)?.Where(x => x != null && x.TaskReward != null && x.TaskReward.TaskExternalCode != null).Select(x => x?.TaskReward?.TaskExternalCode).ToList();
                    var taskExternalMappings = taskImport.Data.TaskExternalMapping
                   .Where(s => s != null && taskrewardExternalCodeInBatch != null && taskrewardExternalCodeInBatch.Contains(s.TaskExternalCode)).ToList();

                    // Extract distinct TermsOfServiceIds from the batch
                    var tosIdsInBatch = batch
                     .Where(x => x.TaskDetail != null)
                     .SelectMany(x => x.TaskDetail)
                     .Select(td => td.TermsOfServiceId)
                     .Distinct()
                     .ToList();



                    // Fetch corresponding TermsOfServiceDto from the lookup
                    var termsOfServices = tosIdsInBatch
                        .Where(termsOfServiceLookup.ContainsKey)
                        .Select(id => termsOfServiceLookup[id])
                        .ToList();
                    var importRequest = new ImportTaskRewardDetailsRequestDto
                    {
                        TenantCode = tenantImportRequestDto.tenantCode,
                        TaskRewardDetails = batch,
                        SubTasks = SubTasks ?? new List<SubTaskDto>(),
                        TaskExternalMappings = taskExternalMappings ?? new List<TaskExternalMappingDto>(),
                        TenantTaskCategory = tenantTaskCategoryBatches.ElementAtOrDefault(i) ?? new List<ExportTenantTaskCategoryDto>(),
                        TermsOfServices = termsOfServices ?? new List<TermsOfServiceDto>()
                    };
                    // Send API request
                    var taskResponse = await _taskClient.Post<ImportTaskResponseDto>(Constant.ImportTaskApiUrl, importRequest);
                    taskRewardsList.AddRange(taskResponse.TaskRewardList);

                    if (taskResponse?.ErrorCode != null)
                    {
                        string errorMessage = $"Task Error: Some records encountered errors for tenant: {tenantImportRequestDto.tenantCode}, " +
                                              $"Message: {taskResponse.ErrorMessage}";

                        _logger.LogWarning("{ClassName}.{MethodName}: Error while importing details, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}",
                            className, methodName, tenantImportRequestDto.tenantCode, taskResponse.ErrorCode);

                        errorMessages.Add(errorMessage);
                    }
                }

                return errorMessages.Any()
                    ? new ImportTaskResponseDto
                    {
                        ErrorCode = StatusCodes.Status206PartialContent,
                        ErrorMessage = string.Join(" | ", errorMessages),
                        TaskRewardList = taskRewardsList
                    }
                    : new ImportTaskResponseDto() { TaskRewardList = taskRewardsList };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: API Error - Message: {Message}, Error Code: {ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImportTaskResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An internal server error occurred."
                };
            }
        }
        private (List<ImportTaskRewardDetailDto>, List<SubTaskDto>?) ImportTaskPayloadForSubtask(List<ImportTaskRewardDetailDto> allImportTaskDto, List<ImportTaskRewardDetailDto> batch, TaskImportJson taskImport)
        {
            var taskrewardIdsInBatch = batch.Select(x => x.TaskReward)?.Where(x => x != null && x.TaskReward != null && x.TaskReward.TaskRewardId > 0).Select(x => x?.TaskReward?.TaskRewardId).ToList();
            var SubTasks = taskImport.Data.SubTask
           .Where(s => taskrewardIdsInBatch != null && taskrewardIdsInBatch.Contains(s.ParentTaskRewardId) && taskrewardIdsInBatch.Contains(s.ChildTaskRewardId)).ToList();

            var remainingSubTasksinBatch = taskImport.Data.SubTask
               .Where(s => taskrewardIdsInBatch != null && (taskrewardIdsInBatch.Contains(s.ParentTaskRewardId)
                 || taskrewardIdsInBatch.Contains(s.ChildTaskRewardId)) // At least one match
                    && !SubTasks.Contains(s)) // Exclude those already in SubTasks
                .ToList();
            if (remainingSubTasksinBatch?.Count > 0)
            {
                var taskRewardIds = remainingSubTasksinBatch
                    .SelectMany(x => new[] { x.ParentTaskRewardId, x.ChildTaskRewardId }) // Flatten both IDs
                    .Distinct() // Ensure unique values
                    .ToList();
                var missingTaskReward = allImportTaskDto
                        .Where(task => task.TaskReward?.TaskReward != null && taskRewardIds.Contains(task.TaskReward.TaskReward.TaskRewardId))
                        .Select(task => new ImportTaskRewardDetailDto
                        {
                            Task = new ExportTaskDto
                            {
                                Task = task.Task.Task,
                                TaskTypeCode = task.Task.TaskTypeCode,
                                TaskCategoryCode = task.Task.TaskCategoryCode
                            },
                            TaskDetail = task.TaskDetail,
                            TaskReward = new TaskRewardDto.ExportTaskRewardDto
                            {
                                TaskReward = task.TaskReward?.TaskReward,
                                TaskRewardTypeCode = task.TaskReward?.TaskRewardTypeCode
                            }

                        })
                        .ToList();
                batch.AddRange(missingTaskReward);
                SubTasks.AddRange(remainingSubTasksinBatch);
            }
            return (batch, SubTasks);
        }

        private async Task<BaseResponseDto> ProcessTriviaImport(TaskImportJson triviaImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessTriviaImport);
            try
            {
                var finalTriviaResponse = new BaseResponseDto();
                var errorMessages = new List<string>();

                if (triviaImport?.Data == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No data to process for import. Data: {taskImport}",
                                      className, methodName, triviaImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "No data to process" };
                }

                if (triviaImport.Data.Trivia == null ||
                    triviaImport.Data.TriviaQuestionGroup == null ||
                    triviaImport.Data.TriviaQuestion == null || triviaImport.Data.Trivia.Count == 0 ||
                    triviaImport.Data.TriviaQuestionGroup.Count == 0 ||
                    triviaImport.Data.TriviaQuestion.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: File does not contain trivia data. Data: {taskImport}",
                                      className, methodName, triviaImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "File does not contain trivia data" };
                }


                var taskRewardDetailBatches = triviaImport.Data.TriviaQuestionGroup
                    .GroupBy(x => x.TriviaId) // Group by TriviaId
                    .SelectMany(group => group
                        .Select((item, index) => new { item, index })
                        .GroupBy(x => x.index / batchSize) // Batch within each TriviaId group
                        .Select(g => g.Select(x => x.item).ToList())
                    )
                    .ToList();

                foreach (var batch in taskRewardDetailBatches)
                {
                    if (batch.Count == 0) continue; // Prevent errors if batch is empty

                    var triviaId = batch.First().TriviaId;
                    var questionIds = batch.Select(x => x.TriviaQuestionId).Distinct().ToList();

                    var triviaTriviaQuestionGroupDto = new ImportTriviaDetailDto
                    {
                        Trivia = triviaImport.Data.Trivia
                            .Where(x => x.Trivia?.TriviaId == triviaId).Select(x => new ExportTriviaDto { Trivia = x.Trivia, TaskExternalCode = x.TaskExternalCode }) //
                            .ToList(),
                        TriviaQuestion = triviaImport.Data.TriviaQuestion
                            .Where(x => questionIds.Contains(x.TriviaQuestionId)) // 
                            .ToList(),
                        TriviaQuestionGroup = batch //
                    };

                    var triviaRequestDto = new ImportTriviaRequestDto
                    {
                        TenantCode = tenantImportRequestDto.tenantCode,
                        TriviaDetailDto = triviaTriviaQuestionGroupDto
                    };

                    var triviaResponse = await _taskClient.Post<BaseResponseDto>(Constant.ImportTriviaApiUrl, triviaRequestDto);

                    if (triviaResponse?.ErrorCode != null)
                    {
                        string errorMessage = $"Trivia Error: Some records encountered error for TriviaId: {triviaId}, Message: {triviaResponse.ErrorMessage}";
                        _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage}", className, methodName, errorMessage);
                        errorMessages.Add(errorMessage);
                    }
                }
                if (errorMessages.Any())
                {
                    finalTriviaResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalTriviaResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalTriviaResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }
        }
        private async Task<BaseResponseDto> ProcessQuestionnaireImport(TaskImportJson questionnaireImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessQuestionnaireImport);
            try
            {
                var finalQuestionnaireResponse = new BaseResponseDto();
                var errorMessages = new List<string>();

                if (questionnaireImport?.Data == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No data to process for import. Data: {taskImport}",
                                      className, methodName, questionnaireImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "No data to process" };
                }

                if (questionnaireImport.Data.Questionnaire == null ||
                    questionnaireImport.Data.QuestionnaireQuestionGroup == null ||
                    questionnaireImport.Data.QuestionnaireQuestion == null || questionnaireImport.Data.Questionnaire.Count == 0 ||
                    questionnaireImport.Data.QuestionnaireQuestionGroup.Count == 0 ||
                    questionnaireImport.Data.QuestionnaireQuestion.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: File does not contain Questionnaire data. Data: {taskImport}",
                                      className, methodName, questionnaireImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "File does not contain questionnaire data" };
                }


                var taskRewardDetailBatches = questionnaireImport.Data.QuestionnaireQuestionGroup
                    .GroupBy(x => x.QuestionnaireId) // Group by QuestionnaireId
                    .SelectMany(group => group
                        .Select((item, index) => new { item, index })
                        .GroupBy(x => x.index / batchSize) // Batch within each QuestionnaireId group
                        .Select(g => g.Select(x => x.item).ToList())
                    )
                    .ToList();

                foreach (var batch in taskRewardDetailBatches)
                {
                    if (batch.Count == 0) continue; // Prevent errors if batch is empty

                    var questionnaireId = batch.First().QuestionnaireId;
                    var questionIds = batch.Select(x => x.QuestionnaireQuestionId).Distinct().ToList();

                    var QuestionnaireQuestionGroupDto = new ImportQuestionnaireDetailDto
                    {
                        Questionnaire = questionnaireImport.Data.Questionnaire
                            .Where(x => x.Questionnaire?.QuestionnaireId == questionnaireId).Select(x => new ExportQuestionnaireDto { Questionnaire = x.Questionnaire, TaskExternalCode = x.TaskExternalCode }) //
                            .ToList(),
                        QuestionnaireQuestion = questionnaireImport.Data.QuestionnaireQuestion
                            .Where(x => questionIds.Contains(x.QuestionnaireQuestionId)) // 
                            .ToList(),
                        QuestionnaireQuestionGroup = batch //
                    };

                    var questionnaireRequestDto = new ImportQuestionnaireRequestDto
                    {
                        TenantCode = tenantImportRequestDto.tenantCode,
                        QuestionnaireDetailDto = QuestionnaireQuestionGroupDto
                    };

                    var questionnaireResponse = await _taskClient.Post<BaseResponseDto>(Constant.ImportQuestionnaireApiUrl, questionnaireRequestDto);

                    if (questionnaireResponse?.ErrorCode != null)
                    {
                        string errorMessage = $"Trivia Error: Some records encountered error for QuestionnaireId: {questionnaireId}, Message: {questionnaireResponse.ErrorMessage}";
                        _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage}", className, methodName, errorMessage);
                        errorMessages.Add(errorMessage);
                    }
                }
                if (errorMessages.Any())
                {
                    finalQuestionnaireResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalQuestionnaireResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalQuestionnaireResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }
        }
        private async Task<ImportCmsResponseDto> ProcessCMSImport(CmsImportJson cmsImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessCMSImport);
            try
            {
                var finalcmsResponse = new ImportCmsResponseDto();
                var errorMessages = new List<string>();

                if (cmsImport?.Data == null || cmsImport == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No data to process for cms import. Data: {taskImport}",
                                      className, methodName, cmsImport?.ToJson());
                    return new ImportCmsResponseDto { ErrorMessage = "No data to process for cms import" };
                }
                if (cmsImport.Data.Component == null ||
                 cmsImport.Data.Component.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: File does not contain cms data. Data: {taskImport}",
                                      className, methodName, cmsImport?.ToJson());
                    return new ImportCmsResponseDto { ErrorMessage = "File does not contain cms data" };
                }
                // Separate components using LINQ
                var collectionComponents = cmsImport.Data.Component
                    .Where(x => x.ComponentTypeCode == Constant.CollectionComponentTypeCode)
                    .ToList();

                var nonCollectionComponents = cmsImport.Data.Component
                    .Where(x => x.ComponentTypeCode != Constant.CollectionComponentTypeCode)
                    .ToList();

                // Helper method to batch using LINQ
                List<List<ImportComponentDto>> Batch(List<ImportComponentDto> components, int batchSize) =>
                    components
                        .Select((component, index) => new { component, index })
                        .GroupBy(x => x.index / batchSize)
                        .Select(g => g.Select(x => x.component).ToList())
                        .ToList();

                // Create batches
                var nonCollectionBatches = Batch(nonCollectionComponents, batchSize);
                var collectionBatches = Batch(collectionComponents, batchSize);

                // Merge batches.We need collection batch at the end
                // so that the mapping can be established by SetChildComponentCode method
                var cmsBatches = nonCollectionBatches.Concat(collectionBatches).ToList();



                foreach (var batch in cmsBatches)
                {
                    if (batch.Count == 0) continue; // Prevent errors if batch is empty


                    var cmsRequestDto = new CmsImportRequestDto
                    {
                        TenantCode = tenantImportRequestDto.tenantCode,
                        Components = batch.Select(x => new ExportComponentDto { Component = x.Component, ComponentTypeCode = x.ComponentTypeCode }).ToList()
                    };
                    var parentChildComponent = batch.Any(x => x.ComponentTypeCode == Constant.CollectionComponentTypeCode);
                    if (parentChildComponent)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName}:started updating parent child code processing for cms tenant {TenantCode}", className, methodName, tenantImportRequestDto.tenantCode);
                        SetChildComponentCode(batch.Where(x => x.ComponentTypeCode == Constant.CollectionComponentTypeCode).ToList(), finalcmsResponse);

                    }
                    var cmsResponse = await _cmsClient.Post<ImportCmsResponseDto>(Constant.ImportCMSApiUrl, cmsRequestDto);
                   
                    if (cmsResponse?.ErrorCode != null)
                    {
                        string errorMessage = $"Batch failed for cms batch: {batch.ToJson()}, Message: {cmsResponse.ErrorMessage}";
                        _logger.LogError($"{className}.{methodName}: {errorMessage}");
                        errorMessages.Add(cmsResponse?.ErrorMessage ?? string.Empty);
                    }
                    if (cmsResponse?.ComponentList != null && cmsResponse.ComponentList.Any())
                    {
                        finalcmsResponse.ComponentList.AddRange(cmsResponse.ComponentList);
                    }
                }
                if (errorMessages.Any())
                {
                    finalcmsResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalcmsResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalcmsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImportCmsResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }
        }
        public void SetChildComponentCode(List<ImportComponentDto> parentComponentRequestDto,
   ImportCmsResponseDto childcomponentsCode)
        {
            const string methodName = nameof(SetChildComponentCode);

            foreach (var parentComponent in parentComponentRequestDto)
            {
                try
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Get Child CMS component started for parentComponentCode: {parentComponent}",
                        className, methodName, parentComponent.Component?.ComponentCode);

                    if (!string.IsNullOrEmpty(parentComponent.Component?.DataJson) && parentComponent.Component.DataJson != "{}")
                    {
                        // Parse DataJson as JsonNode for partial updates
                        var rootNode = JsonNode.Parse(parentComponent.Component.DataJson);

                        // Navigate to data node
                        var dataNode = rootNode?["data"] as JsonObject;

                        if (dataNode != null)
                        {
                            // Check if childrenComponentCodes exists
                            if (dataNode.ContainsKey("childrenComponentCodes"))
                            {
                                var oldChildCodes = dataNode["childrenComponentCodes"]?.AsArray();
                                var newChildCodesList = new List<string>();

                                if (oldChildCodes != null)
                                {
                                    foreach (var childCode in oldChildCodes)
                                    {
                                        var oldCode = childCode?.ToString();

                                        var childComponent = childcomponentsCode.ComponentList
                                            .FirstOrDefault(x => x.OldComponentCode == oldCode);

                                        if (childComponent == null)
                                        {
                                            _logger.LogError("{ClassName}.{MethodName}: No child component found for old componentcode: {code}", className, methodName, oldCode);
                                            continue;
                                        }

                                        newChildCodesList.Add(childComponent.ComponentCode);
                                    }
                                }

                                // Replace childrenComponentCodes with new values
                                dataNode["childrenComponentCodes"] = JsonSerializer.SerializeToNode(newChildCodesList);

                                // Assign updated DataJson
                                parentComponent.Component.DataJson = rootNode.ToJsonString();
                            }
                            else
                            {
                                _logger.LogWarning("{ClassName}.{MethodName}: 'childrenComponentCodes' not found in data node for component: {componentCode}", className, methodName, parentComponent.Component.ComponentCode);
                            }
                        }
                        else
                        {
                            _logger.LogError("{ClassName}.{MethodName}: 'data' node not found in JSON for component: {componentCode}", className, methodName, parentComponent.Component.ComponentCode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                    continue;
                }
            }
        }



        private async Task<BaseResponseDto> ProcessFisImport(FisImportJson FisImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessFisImport);
            try
            {
                IDictionary<string, long> parameters = new Dictionary<string, long>();
                var tenantSponsorCustomerDto = await _tenantClient.Get<TenantSponsorCustomerResponseDto>($"{Constant.GetTenantSponsorCustomer}/{tenantImportRequestDto.tenantCode}", parameters);
                if (tenantSponsorCustomerDto == null || tenantSponsorCustomerDto.Customer == null || !tenantSponsorCustomerDto.Customer.CustomerCode.Equals(tenantImportRequestDto.CustomerCode) ||
                   tenantSponsorCustomerDto.Sponsor == null || !tenantSponsorCustomerDto.Sponsor.SponsorCode.Equals(tenantImportRequestDto.SponsorCode))
                {
                    _logger.LogError("{ClassName}.{MethodName}: Incorrect Sponsor or Customer Code: {Request}", className, methodName, tenantImportRequestDto?.ToJson());

                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Incorrect Sponsor or Customer Code" };

                }
                else
                {

                    if (FisImport != null && FisImport.Data != null && FisImport.Data.TenantAccount != null)
                    {
                        FisImport.Data.TenantAccount.TenantCode = tenantImportRequestDto.tenantCode;
                        var TenantAccount = await _tenantAccountService.GetTenantAccount(tenantImportRequestDto.tenantCode);
                        if (TenantAccount.TenantAccount != null && TenantAccount.TenantAccount.TenantCode.Equals(tenantImportRequestDto.tenantCode))
                        {
                            TenantAccountRequestDto UpdateTenantAccountRequest = _mapper.Map<TenantAccountRequestDto>(FisImport.Data.TenantAccount);
                            UpdateTenantAccountRequest.LastMonetaryTransactionId = TenantAccount.TenantAccount.LastMonetaryTransactionId;
                            UpdateTenantAccountRequest.UpdateUser = _userContextService.GetUpdateUser();

                            var fisResponse = await _tenantAccountService.UpdateTenantAccount(tenantImportRequestDto.tenantCode, UpdateTenantAccountRequest);
                            ;
                            if (fisResponse.ErrorCode != null)
                            {
                                _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while Importing Tenant Account Details, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, tenantImportRequestDto.tenantCode, fisResponse.ErrorCode);
                                return fisResponse;
                            }
                            var walletResponse = await _tenantAccountService.CreateMasterWallets(UpdateTenantAccountRequest, tenantImportRequestDto.CustomerCode, tenantImportRequestDto.SponsorCode, Constant.CreateUserAsETL);
                            if (walletResponse.ErrorCode != null)
                            {
                                _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while Importing Tenant Account wallet, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, tenantImportRequestDto.tenantCode, walletResponse.ErrorCode);
                                return walletResponse;
                            }
                            return new BaseResponseDto();
                        }

                        else
                        {
                            CreateTenantAccountRequestDto createTenantAccountRequest = new CreateTenantAccountRequestDto
                            {
                                CustomerCode = tenantImportRequestDto.CustomerCode,
                                SponsorCode = tenantImportRequestDto.SponsorCode,
                                TenantAccount = _mapper.Map<PostTenantAccountDto>(FisImport.Data.TenantAccount)

                            };
                            createTenantAccountRequest.TenantAccount.CreateTs = DateTime.UtcNow;
                            createTenantAccountRequest.TenantAccount.CreateUser = Constant.ImportUser;
                            var fisResponse = await _tenantAccountService.CreateTenantAccount(createTenantAccountRequest);
                            if (fisResponse.ErrorCode != null)
                            {
                                _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while Importing Tenant Account Details, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, tenantImportRequestDto.tenantCode, fisResponse.ErrorCode);
                                return fisResponse;
                            }
                            return new BaseResponseDto();
                        }
                    }
                }
                _logger.LogError("{ClassName}.{MethodName}: an error occured for FIS import.No tenant Account record found for Fisdata: {FisImport}", className, methodName, FisImport?.ToJson());

                return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "No tenant Account record found" };


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }

        }
        private async Task<BaseResponseDto> ProcessFisTenantProgramImport(FisImportJson FisImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessFisTenantProgramImport);
            BaseResponseDto configResponse= new BaseResponseDto();
            try
            {
               

                    if (FisImport != null && FisImport.Data?.TenantProgramConfig != null && FisImport.Data?.TenantProgramConfig.Count>0)
                    {
                    var programconfig = FisImport.Data?.TenantProgramConfig
               .GroupBy(x => x.TenantCode) // Group by QuestionnaireId
               .SelectMany(group => group
                   .Select((item, index) => new { item, index })
                   .GroupBy(x => x.index / batchSize) // Batch within each QuestionnaireId group
                   .Select(g => g.Select(x => x.item).ToList())
               )
               .ToList();

                    foreach (var batch in programconfig)
                    {
                        if (batch.Count == 0) continue; // Prevent errors if batch is empty

                      
                        var tenantprogramConfigRequestDto = new ImportTenantprogramConfigRequestDto
                        {
                            TenantCode = tenantImportRequestDto.tenantCode,
                            TenantProgramConfigDto = batch //
                        };


                         configResponse = await _fisClient.Post<BaseResponseDto>(Constant.ImportTenantProgramApiUrl, tenantprogramConfigRequestDto);

                        if (configResponse?.ErrorCode != null)
                        {
                            string errorMessage = $"Tenant program Config Error: Some records encountered error for Tenant program config Message: {configResponse.ErrorMessage}";
                            _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage}", className, methodName, errorMessage);
                        }
                    }
                  

                    return configResponse;
                }


            
                
                _logger.LogError("{ClassName}.{MethodName}: an error occured for FIS import.No tenant program config record found for Fisdata: {FisImport}", className, methodName, FisImport?.ToJson());

                return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "No tenant program config record found" };


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }

        }


        private async Task<BaseResponseDto> ProcessTenantImportAsync(TenantImportRequestDto tenantImportRequestDto, ImportDto importDto)
        {
            // 1) Check tenant exist or not
            // 2) If tenant exist update
            // 3) If tenant not exist create
            const string methodName = nameof(ProcessTenantImportAsync);
            try
            {
                if (importDto.TenantCodeData == null || importDto.TenantCodeData.Data.Tenant == null)
                {
                    return new BaseResponseDto() { ErrorMessage = "Tenant Json not found" };
                }
                var tenantCode = tenantImportRequestDto.tenantCode;
                var tenantFromJson = importDto.TenantCodeData.Data;
                tenantFromJson.Tenant.TenantCode = tenantCode; //Update the tenant Code

                var createTenantRequest = new CreateTenantRequestDto()
                {
                    CustomerCode = tenantImportRequestDto.CustomerCode,
                    SponsorCode = tenantImportRequestDto.SponsorCode,
                    Tenant = _mapper.Map<PostTenantDto>(tenantFromJson.Tenant)
                };
                createTenantRequest.Tenant.CreateUser = Constant.ImportUser;
                var tenantData = await _tenantService.GetTenantDetails(tenantCode);

                if (tenantData?.ErrorCode != null && tenantData?.ErrorCode != StatusCodes.Status404NotFound)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while fetching tenant data for TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, tenantCode, tenantData.ErrorCode);
                    return new BaseResponseDto()
                    {
                        ErrorCode = tenantData?.ErrorCode,
                        ErrorMessage = tenantData?.ErrorMessage,
                    };
                }
                if (tenantData?.Tenant == null)
                {
                    createTenantRequest.Tenant.CreateTs = DateTime.UtcNow;

                    var createTenantResponse = await _tenantService.CreateTenant(createTenantRequest);
                    if (createTenantResponse?.ErrorCode != null)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenant data for TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, tenantCode, createTenantResponse.ErrorCode);
                        return new BaseResponseDto()
                        {
                            ErrorCode = createTenantResponse?.ErrorCode,
                            ErrorMessage = createTenantResponse?.ErrorMessage,
                        };
                    }

                }
                if (tenantData?.Tenant != null)
                {
                    var updateTenantDto = _mapper.Map<UpdateTenantDto>(importDto.TenantCodeData.Data.Tenant);
                    updateTenantDto.UpdateUser = _userContextService.GetUpdateUser();
                    var updateTenantResponse = await _tenantService.UpdateTenant(tenantCode, updateTenantDto);
                    if (updateTenantResponse?.ErrorCode != null)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenant data for TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, tenantCode, updateTenantResponse.ErrorCode);
                        return new BaseResponseDto()
                        {
                            ErrorCode = updateTenantResponse?.ErrorCode,
                            ErrorMessage = updateTenantResponse?.ErrorMessage,
                        };
                    }
                }

                return new BaseResponseDto();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }


        }
        private async Task<BaseResponseDto> ProcessSweepstakesImport(SweepstakesImportJson SweepstakesImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessSweepstakesImport);
            try
            {
                var finalSweepstakesResponse = new BaseResponseDto();
                var errorMessages = new List<string>();

                if (SweepstakesImport?.Data == null || SweepstakesImport?.Data.Sweepstakes == null || SweepstakesImport?.Data.Sweepstakes.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No data to process for Sweepstakes import. Data: {SweepstakesImport}",
                                      className, methodName, SweepstakesImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "No data to process for Sweepstakes import" };
                }
                var sweepstakes = SweepstakesImport?.Data.Sweepstakes ?? new List<SweepstakesDto>();
                var tenantSweepstakes = SweepstakesImport?.Data.TenantSweepstakes ?? new List<TenantSweepstakesDto>();

                var tenantLookup = tenantSweepstakes.GroupBy(ts => ts.SweepstakesId)
                                                    .ToDictionary(g => g.Key, g => g.ToList());

                // Split Sweepstakes into batches while keeping TenantSweepstakes linked
                var batches = sweepstakes
                    .Select((s, index) => new { s, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(group => new SweepstakesData
                    {
                        Sweepstakes = group.Select(x => x.s).ToList(),
                        TenantSweepstakes = group.SelectMany(x => tenantLookup.ContainsKey(x.s.SweepstakesId) ? tenantLookup[x.s.SweepstakesId] : new List<TenantSweepstakesDto>()).ToList()
                    })
                    .ToList();


                foreach (var batch in batches)
                {


                    var SweepstakesRequestDto = new ImportSweepstakesRequestDto
                    {
                        TenantCode = tenantImportRequestDto.tenantCode,
                        Sweepstakes = batch.Sweepstakes,
                        TenantSweepstakes = batch.TenantSweepstakes,
                    };

                    var sweepstakesResponse = await _sweepstakesClient.Post<BaseResponseDto>(Constant.ImportSweepstakesApiUrl, SweepstakesRequestDto);

                    if (sweepstakesResponse?.ErrorCode != null)
                    {
                        string errorMessage = $"Sweepstakes Error: Some records encountered error for: {batch.ToJson()}, Message: {sweepstakesResponse.ErrorMessage}";
                        _logger.LogError($"{className}.{methodName}: {errorMessage}");
                        errorMessages.Add(sweepstakesResponse?.ErrorMessage ?? string.Empty);
                    }
                }
                if (errorMessages.Any())
                {
                    finalSweepstakesResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalSweepstakesResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalSweepstakesResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }
        }
        private async Task<BaseResponseDto> ProcessCohortImport(CohortImportJson cohortImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessCohortImport);
            try
            {
                var finalcohortResponse = new BaseResponseDto();
                var errorMessages = new List<string>();

                if (cohortImport?.Data == null || cohortImport?.Data.Cohort == null || cohortImport?.Data.Cohort.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No data to process for cohort import. Data: {cohortImport}",
                                      className, methodName, cohortImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "No data to process for cohort import" };
                }
                var cohorts = cohortImport?.Data.Cohort ?? new List<CohortDto>();
                var tenantcohorts = cohortImport?.Data.CohortTenantTaskReward ?? new List<ImportCohortDto>();


                List<CohortTenantTaskRewardDto> cohortTenantTaskRewardDtoList = new();
                if (tenantcohorts != null && tenantcohorts.Count > 0)
                {
                    foreach (var cohortTenantTaskReward in tenantcohorts)
                    {
                        CohortTenantTaskRewardDto cohortTenantTask = new CohortTenantTaskRewardDto();


                        var url = $"{Constant.TaskRewardDetailsApiUrl}?tenantCode={tenantImportRequestDto.tenantCode}&taskExternalCode={cohortTenantTaskReward.TaskExternalCode}";

                        var response = await _taskClient.Get<TaskRewardDetailsResponseDto>(url, new Dictionary<string, long>());
                        if (response.ErrorCode != null)
                        {
                            _logger.LogError("{ClassName}.{MethodName}: No task reward found for cohort tenant task import. Data: {cohortTenantTaskReward}",
                                    className, methodName, cohortTenantTaskReward?.ToJson());
                            continue;
                        }
                        var taskReward = response.TaskRewardDetails?.Select(x => x.TaskReward)?.Where(x => x.TaskExternalCode == cohortTenantTaskReward.TaskExternalCode).FirstOrDefault();
                        if (taskReward != null)
                        {
                            cohortTenantTask = cohortTenantTaskReward.CohortTenantTaskReward;
                            cohortTenantTask.TaskRewardCode = taskReward.TaskRewardCode;
                        }


                        cohortTenantTaskRewardDtoList.Add(cohortTenantTask);
                    }

                }



                var tenantLookup = cohortTenantTaskRewardDtoList.GroupBy(ts => ts.CohortId)
                                                    .ToDictionary(g => g.Key, g => g.ToList());

                // Split cohort into batches while keeping Tenantcohort linked
                var batches = cohorts
                    .Select((s, index) => new { s, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(group => new ImportCohortRequestDto
                    {
                        TenantCode = tenantImportRequestDto.tenantCode,

                        Cohorts = group.Select(x => x.s).ToList(),
                        TenantCohortTaskRewards = group.SelectMany(x => tenantLookup.ContainsKey(x.s.CohortId) ? tenantLookup[x.s.CohortId] : new List<CohortTenantTaskRewardDto>()).ToList()
                    })
                    .ToList();


                foreach (var batch in batches)
                {
                    var cohortResponse = await _cohortClient.Post<BaseResponseDto>(Constant.ImportCohortApiUrl, batch);

                    if (cohortResponse?.ErrorCode != null)
                    {
                        string errorMessage = $"cohort Error: Some records encountered error for: {batch.ToJson()}, Message: {cohortResponse.ErrorMessage}";
                        _logger.LogError($"{className}.{methodName}: {errorMessage}");
                        errorMessages.Add(cohortResponse?.ErrorMessage ?? string.Empty);
                    }
                }
                if (errorMessages.Any())
                {
                    finalcohortResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalcohortResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalcohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }
        }

        /// <summary>
        /// Processes the admin import JSON file and persists data in batches, including Scripts, TenantTaskRewardScripts,
        /// and EventHandlerScripts based on the tenant import request.
        /// </summary>
        /// <param name="adminImportJson">The JSON object containing admin data to be imported.</param>
        /// <param name="tenantImportRequestDto">The import configuration and metadata related to the tenant.</param>
        /// <returns>
        /// A <see cref="BaseResponseDto"/> indicating the success or partial success/failure of the import,
        /// including error messages if applicable.
        /// </returns>
        private async Task<BaseResponseDto> ProcessAdminImport(AdminImportJson adminImportJson, TenantImportRequestDto tenantImportRequestDto,
            ImportTaskResponseDto importTaskResponse)
        {
            const string methodName = nameof(ProcessAdminImport);
            _logger.LogInformation("{ClassName}.{MethodName}: Starting admin import processing for tenant {TenantCode}", className, methodName, tenantImportRequestDto.tenantCode);

            var finalResponse = new BaseResponseDto();
            var errorMessages = new List<string>();

            try
            {
                var rewardCodeMapping = importTaskResponse?.TaskRewardList?.DistinctBy(x => x.TaskRewardCode)
                    .ToDictionary(x => x.TaskRewardCode!, x => x.NewRewardCode!) ?? new Dictionary<string, string>();

                if (adminImportJson?.Data == null)
                {
                    const string msg = "No data to process for admin import.";
                    _logger.LogError("{ClassName}.{MethodName}: {Message}. Input: {AdminImport}", className, methodName, msg, adminImportJson?.ToJson());
                    return new BaseResponseDto { ErrorMessage = msg };
                }

                if ((adminImportJson.Data.Scripts?.Count ?? 0) == 0 &&
                    (adminImportJson.Data.TenantTaskRewardScripts?.Count ?? 0) == 0 &&
                    (adminImportJson.Data.EventHandlerScripts?.Count ?? 0) == 0)
                {
                    const string msg = "File does not contain admin data.";
                    _logger.LogError("{ClassName}.{MethodName}: {Message}. Input: {AdminImport}", className, methodName, msg, adminImportJson?.ToJson());
                    return new BaseResponseDto { ErrorMessage = msg };
                }

                // Process in batches
                int skipCount = 0;
                bool hasMore;
                do
                {
                    var scriptBatch = adminImportJson.Data.Scripts?.Skip(skipCount).Take(batchSize).ToList() ?? new();
                    var taskRewardScriptBatch = adminImportJson.Data.TenantTaskRewardScripts?.Skip(skipCount).Take(batchSize).ToList() ?? new();
                    var eventHandlerBatch = adminImportJson.Data.EventHandlerScripts?.Skip(skipCount).Take(batchSize).ToList() ?? new();

                    hasMore = scriptBatch.Any() || taskRewardScriptBatch.Any() || eventHandlerBatch.Any();

                    if (hasMore)
                    {
                        var adminRequestDto = new ImportAdminRequestDto
                        {
                            TenantCode = tenantImportRequestDto.tenantCode,
                            Scripts = scriptBatch,
                            TenantTaskRewardScripts = taskRewardScriptBatch,
                            EventHandlerScripts = eventHandlerBatch
                        };

                        _logger.LogInformation("{ClassName}.{MethodName}: Processing admin batch with {ScriptCount} Scripts, {RewardScriptCount} TaskRewardScripts, {EventHandlerCount} EventHandlers for tenant {TenantCode}",
                            className, methodName, scriptBatch.Count, taskRewardScriptBatch.Count, eventHandlerBatch.Count, adminRequestDto.TenantCode);

                        var adminResponse = await _adminService.CreateAdminScripts(adminRequestDto, rewardCodeMapping);

                        if (adminResponse?.ErrorCode != null)
                        {
                            var errorMessage = $"Admin batch failed. TenantCode: {adminRequestDto.TenantCode}, Error: {adminResponse.ErrorMessage}";
                            _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage}, BatchRequest: {BatchRequest}", className, methodName, errorMessage, adminRequestDto.ToJson());
                            errorMessages.Add(adminResponse.ErrorMessage ?? "Unknown error");
                        }

                        skipCount += batchSize;
                    }

                } while (hasMore);

                // Finalize response
                if (errorMessages.Any())
                {
                    finalResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalResponse.ErrorMessage = string.Join(" | ", errorMessages);
                    _logger.LogWarning("{ClassName}.{MethodName}: Completed with partial errors. Errors: {Errors}", className, methodName, finalResponse.ErrorMessage);
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Admin import processing completed successfully for tenant {TenantCode}", className, methodName, tenantImportRequestDto.tenantCode);
                }

                return finalResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred - {Message}", className, methodName, ex.Message);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public BaseResponseDto ProcessErrors(List<BaseResponseDto> errorList)
        {
            if (errorList == null || !errorList.Any())
            {
                return new BaseResponseDto(); // Return an empty object if no errors
            }

            var errorMessages = errorList
                .Where(e => e.ErrorMessage != null)
                .Select(e => $"{e.ErrorMessage}")
                .ToList();
            var errorCode = errorList
                .Where(e => e.ErrorCode != null).OrderByDescending(x => x.ErrorCode).FirstOrDefault()?.ErrorCode ?? null;
            ;

            return new BaseResponseDto
            {
                ErrorCode = errorCode,
                ErrorMessage = string.Join(", ", errorMessages)
            };
        }

        /// <summary>
        /// ProcessWalletImport
        /// </summary>
        /// <param name="walletImport"></param>
        /// <returns></returns>
        private async Task<BaseResponseDto> ProcessWalletImport(WalletImportJson walletImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessWalletImport);
            try
            {
                var finalWalletResponse = new BaseResponseDto();
                var errorMessages = new List<string>();

                if (walletImport?.Data == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No data to process for wallet import. Data: {walletImport}",
                                      className, methodName, walletImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "No data to process" };
                }
                if (walletImport.Data.WalletTypeTransferRule == null ||
                 walletImport.Data.WalletTypeTransferRule.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: File does not contain walletType transfer rule data. Data: {walletImport}",
                                      className, methodName, walletImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "File does not contain walletType transfer rule data" };
                }
                var walletBatches = walletImport?.Data.WalletTypeTransferRule
                    .Select((wallet, index) => new { wallet, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(group => group.Select(x => x.wallet).ToList())
                    .ToList();


                foreach (var batch in walletBatches)
                {
                    if (batch.Count == 0) continue; // Prevent errors if batch is empty

                    var request = new ImportWalletTypeTransferRuleRequestDto
                    {
                        TenantCode = tenantImportRequestDto.tenantCode,
                        WalletTypeTransferRules = batch
                    };

                    var response = await _walletTypeTransferRuleService.ImportWalletTypeTranferRule(request);
                    if (response?.ErrorCode != null)
                    {
                        string errorMessage = $"Batch failed for walletType transfer rule batch: {batch.ToJson()}, Message: {response.ErrorMessage}";
                        _logger.LogError($"{className}.{methodName}: {errorMessage}");
                        errorMessages.Add(response?.ErrorMessage ?? string.Empty);
                    }
                }
                if (errorMessages.Any())
                {
                    finalWalletResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalWalletResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalWalletResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };
            }
        }

        /// <summary>
        /// Processes the metadata import JSON file and persists data in batches, including TaskTypes, TaskCategories, and RewardTypes based on the tenant import request.
        /// </summary>
        /// <param name="metadataImport"></param>
        /// <param name="tenantImportRequestDto"></param>
        /// <returns></returns>
        private async Task<BaseResponseDto> ProcessMetadataImport(MetadataImportJson metadataImport, TenantImportRequestDto tenantImportRequestDto)
        {
            const string methodName = nameof(ProcessWalletImport);
            try
            {
                var finalMetadataResponse = new BaseResponseDto();
                var errorMessages = new List<string>();

                if (metadataImport?.Data == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No metadata to process for tenant import for tenantCode: {tenantCode}. Data: {metadataImportJson}",
                                      className, methodName, tenantImportRequestDto.tenantCode, metadataImport?.ToJson());
                    return new BaseResponseDto { ErrorMessage = "No metadata to process" };
                }

                await ProcessMetadataBatches(metadataImport.Data.TaskTypes, batch => _taskService.ImportTaskTypes(batch), errorMessages, methodName);
                await ProcessMetadataBatches(metadataImport.Data.TaskCategories, batch => _taskService.ImportTaskCategories(batch), errorMessages, methodName);
                await ProcessMetadataBatches(metadataImport.Data.RewardTypes, batch => _taskService.ImportTaskRewardTypes(batch), errorMessages, methodName);
                await ProcessMetadataBatches(metadataImport.Data.WalletTypes, batch => _walletTypeService.ImportWalletTypesAsync(batch), errorMessages, methodName);
                await ProcessMetadataBatches(metadataImport.Data.ComponentTypes, batch => _componentService.ImportComponentTypesAsync(batch), errorMessages, methodName);

                if (errorMessages.Any())
                {
                    finalMetadataResponse.ErrorCode = StatusCodes.Status206PartialContent;
                    finalMetadataResponse.ErrorMessage = string.Join(" | ", errorMessages);
                }

                return finalMetadataResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error processing metadata for tenantCode:{tenantCode} - ERROR Msg:{msg}, Error Code:{errorCode}",
                    className, methodName, tenantImportRequestDto.tenantCode, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        /// <summary>
        /// Processes metadata batches for TaskTypes, TaskCategories, and RewardTypes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="items"></param>
        /// <param name="importFunc"></param>
        /// <param name="errorMessages"></param>
        /// <param name="batchLabel"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task ProcessMetadataBatches<T, TResponse>(
            List<T> items,
            Func<List<T>, Task<TResponse>> importFunc,
            List<string> errorMessages,
            string methodName) where TResponse : BaseResponseDto
        {
            var batchLabel = typeof(T)?.Name?.Replace("Dto", "");
            if (items == null || !items.Any())
            {
                _logger.LogWarning("{ClassName}.{MethodName}: No data found for {batchLabel} in metadata.json file",
                                  className, methodName, batchLabel);
                return;
            }

            var batches = items
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / batchSize)
                .Select(group => group.Select(x => x.item).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                if (!batch.Any()) continue;

                var response = await importFunc(batch);

                if (response?.ErrorCode != null)
                {
                    string errorMessage = $"Batch failed for {batchLabel} batch: {batch.ToJson()}, Message: {response.ErrorMessage}";
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage}", className, methodName, errorMessage);
                    errorMessages.Add(response.ErrorMessage ?? string.Empty);
                }
            }
        }

    }
}
