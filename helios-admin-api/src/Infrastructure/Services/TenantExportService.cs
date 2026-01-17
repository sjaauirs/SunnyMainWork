using System.Runtime.Serialization;
using FluentNHibernate.Testing.Values;
using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Infrastructure.Exceptions;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using static SunnyRewards.Helios.Task.Core.Domain.Dtos.TaskRewardDto;
using Threading = System.Threading.Tasks;
using SunnyBenefits.Fis.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TenantExportService : ITenantExportService
    {
        private readonly ILogger<TenantExportService> _logger;
        private readonly ITenantClient _tenantClient;
        private readonly IS3Service _s3Service;
        private readonly ISecretHelper _secretHelper;
        private readonly ICohortClient _cohortClient;
        private readonly ITaskClient _taskClient;
        private readonly ICmsClient _cmsClient;
        private readonly IFisClient _fisClient;
        private readonly ISweepstakesClient _sweepstakesClient;
        private readonly IAdminService _adminService;
        private readonly IWalletClient _walletClient;
        private readonly ITaskRewardTypeService _taskRewardTypeService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly ITaskTypeService _taskTypeService;
        private readonly IComponentService _componentService;
        private readonly IWalletTypeService _walletTypeService;

        const string _className = nameof(TenantExportService);

        public TenantExportService(ILogger<TenantExportService> logger, ITenantClient tenantClient, IS3Service s3Service,
            ISecretHelper secretHelper, ICohortClient cohortClient, ITaskClient taskClient, ICmsClient cmsClient,
            IFisClient fisClient, ISweepstakesClient sweepstakesClient, IAdminService adminService, IWalletClient walletClient,
            ITaskRewardTypeService taskRewardTypeService, ITaskCategoryService taskCategoryService, ITaskTypeService taskTypeService,
            IComponentService componentService, IWalletTypeService walletTypeService)
        {
            _logger = logger;
            _tenantClient = tenantClient;
            _s3Service = s3Service;
            _secretHelper = secretHelper;
            _cohortClient = cohortClient;
            _taskClient = taskClient;
            _cmsClient = cmsClient;
            _fisClient = fisClient;
            _sweepstakesClient = sweepstakesClient;
            _adminService = adminService;
            _walletClient = walletClient;
            _taskRewardTypeService = taskRewardTypeService;
            _taskCategoryService = taskCategoryService;
            _taskTypeService = taskTypeService;
            _componentService = componentService;
            _walletTypeService = walletTypeService;
        }
        /// <summary>
        /// Exports tenant data based on the provided request.
        /// </summary>
        /// <param name="request">The export tenant request DTO containing the necessary parameters.</param>
        /// <returns>An ExportTenantResponseDto containing the export file data or an error response.</returns>
        public async Task<ExportTenantResponseDto> ExportTenantAsync(ExportTenantRequestDto request)
        {
            var methodName = nameof(ExportTenantAsync);
            var tenantCode = request.TenantCode;

            try
            {
                // Log the start of the export process
                _logger.LogInformation("{ClassName}.{MethodName}: Export process started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

                if (request.ExportOptions == null || request.ExportOptions?.Length <= 0)
                {
                    return new ExportTenantResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Please provide ExportOptions"
                    };
                }

                // Retrieve tenant information
                var tenant = await GetTenantByTenantCode(tenantCode);
                if (tenant == null || tenant.TenantId < 1)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Tenant not found, TenantCode: {TenantCode}", _className, methodName, tenantCode);
                    return new ExportTenantResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Tenant not found"
                    };
                }

                // Export tenant.json
                var tenantExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.TENANT.ToString(),
                    Data = new { Tenant = tenant }
                };
                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string tenantFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.TenantJson}";
                await SaveJsonToS3Async(tenantExportData, tenantFileName);

                //Export metadata tables based on export options
                await ExportMetadata(tenantCode, request!.ExportOptions!);

                // Export cohort.json if required
                if (request!.ExportOptions!.Contains(ExportOption.EXPORT_ALL.ToString()) || request!.ExportOptions!.Contains(ExportOption.COHORT.ToString()))
                {
                    await ExportCohortsAsync(tenantCode, request!.ExportOptions!);
                }

                // Export task.json if required
                if (request!.ExportOptions!.Contains(ExportOption.EXPORT_ALL.ToString()) || request!.ExportOptions!.Contains(ExportOption.TASK.ToString()))
                {
                    await ExportTasksAsync(tenantCode, request!.ExportOptions!);
                    await ExportAdventuresAsync(tenantCode, request!.ExportOptions!);
                    await ExportTaskRewardCollectionsAsync(tenantCode);
                }

                // Export cms.json if required
                if (request!.ExportOptions!.Contains(ExportOption.EXPORT_ALL.ToString()) || request!.ExportOptions!.Contains(ExportOption.CMS.ToString()))
                {
                    await ExportCmsAsync(tenantCode);
                }

                // Export fis.json if required
                if (request!.ExportOptions!.Contains(ExportOption.EXPORT_ALL.ToString()) || request!.ExportOptions!.Contains(ExportOption.FIS.ToString()))
                {
                    await ExportFISAsync(tenantCode);
                }

                // Export sweepstakes.json if required
                if (request!.ExportOptions!.Contains(ExportOption.EXPORT_ALL.ToString()) || request!.ExportOptions!.Contains(ExportOption.SWEEPSTAKES.ToString()))
                {
                    await ExportSweepstakesAsync(tenantCode);
                }
                // Export admin.json if required
                if (request!.ExportOptions!.Contains(ExportOption.EXPORT_ALL.ToString()) || request!.ExportOptions!.Contains(ExportOption.ADMIN.ToString()))
                {
                    await ExportAdminAsync(tenantCode, request!.ExportOptions!);

                }

                // Export wallet.json if required
                if (request!.ExportOptions!.Contains(ExportOption.EXPORT_ALL.ToString()) || request!.ExportOptions!.Contains(ExportOption.WALLET.ToString()))
                {
                    await ExportWallet(tenantCode);
                }

                // Generate checksum.txt
                await GenerateChecksumAsync(s3Folder);

                // Create and upload zip file
                var environment = await _secretHelper.GetEnvironment();
                var zipFileName = $"{environment}_{tenantCode}_{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
                var isZipFileUploaded = await ZipAndUploadToS3Async(tenantCode, s3Folder, zipFileName);

                if (!isZipFileUploaded)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Failed to upload zip file for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                    return new ExportTenantResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "Could not upload zip file"
                    };
                }

                // Delete all JSON files
                await DeleteJsonFiles(tenantCode, s3Folder);

                // Download and return the zip file
                var zipStream = await DownloadZipFile(tenantCode, s3Folder, zipFileName);
                return new ExportTenantResponseDto
                {
                    ExportFileData = zipStream,
                    FileName = zipFileName,
                    FileType = ContentTypes.ZipFile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during Tenant export. ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}", _className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        /// <summary>
        /// Get Tenant by tenant code
        /// </summary>
        /// <param name="tenantCode">The code of the tenant to retrieve.</param>
        /// <returns>A TenantDto object.</returns>
        private async Task<TenantDto> GetTenantByTenantCode(string tenantCode)
        {
            var getTenantCodeRequestDto = new GetTenantCodeRequestDto()
            {
                TenantCode = tenantCode,
            };
            var tenantResponse = await _tenantClient.Post<TenantDto>("tenant/get-by-tenant-code", getTenantCodeRequestDto);
            _logger.LogInformation("Retrieved Tenant Data Successfully for TenantCode : {TenantCode}", tenantCode);

            return tenantResponse;
        }

        /// <summary>
        /// Zips the specified folder and uploads it to S3.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="s3Folder">The S3 folder path.</param>
        /// <param name="zipFileName">The name of the zip file to be created.</param>
        /// <returns>A boolean indicating whether the zip and upload operation was successful.</returns>
        private async Task<bool> ZipAndUploadToS3Async(string tenantCode, string s3Folder, string zipFileName)
        {
            var s3BucketName = _secretHelper.GetAwsTmpS3BucketName();
            var exportJsonFilesFolderName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}";
            var zipFileNameWithFolder = $"{s3Folder}/{zipFileName}";
            return await _s3Service.ZipFolderAndUpload(s3BucketName, exportJsonFilesFolderName, zipFileNameWithFolder);
        }

        /// <summary>
        /// Downloads the specified zip file from S3.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="s3Folder">The S3 folder path.</param>
        /// <param name="zipFileName">The name of the zip file to be downloaded.</param>
        /// <returns>A MemoryStream containing the downloaded zip file.</returns>
        private async Task<MemoryStream> DownloadZipFile(string tenantCode, string s3Folder, string zipFileName)
        {
            var s3BucketName = _secretHelper.GetAwsTmpS3BucketName();
            var exportJsonFilesFolderName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}";
            var zipFileNameWithFolder = $"{s3Folder}/{zipFileName}";
            return await _s3Service.DownloadZipFile(s3BucketName, zipFileNameWithFolder);
        }

        /// <summary>
        /// Deletes all JSON files in the specified S3 folder.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="s3Folder">The S3 folder path.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        private async System.Threading.Tasks.Task DeleteJsonFiles(string tenantCode, string s3Folder)
        {
            var s3BucketName = _secretHelper.GetAwsTmpS3BucketName();
            var exportJsonFilesFolderName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}";
            await _s3Service.DeleteFolder(s3BucketName, exportJsonFilesFolderName);
        }


        /// <summary>
        /// Exports tasks for a given tenant code and export options.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which tasks are to be exported.</param>
        /// <param name="exportOptions">The list of export options selected by the user.</param>
        private async System.Threading.Tasks.Task ExportTasksAsync(string tenantCode, string[] exportOptions, List<string?> taskRewardCodes = null)
        {
            const string methodName = nameof(ExportTasksAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var tasks = new ExportTaskResponseDto();
                var exportTaskRequestDto = new ExportTaskRequestDto
                {
                    TenantCode = tenantCode,
                };
                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to TaskExportAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                tasks = await _taskClient.Post<ExportTaskResponseDto>(Constant.TaskExportAPIUrl, exportTaskRequestDto);

                if (taskRewardCodes != null && taskRewardCodes.Count > 0)
                {
                    tasks = GetTasks(taskRewardCodes, tasks);
                }
                if (tasks != null && tasks.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching tasks data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}", _className, methodName, tenantCode, tasks.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched tasks data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }

                var tasksExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.TASK.ToString(),
                    Data = CreateTaskDataObject(tasks, exportOptions)
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string taskFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.TaskJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving tasks data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, taskFileName);
                await SaveJsonToS3Async(tasksExportData, taskFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved tasks data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }

        /// <summary>
        /// Filters and returns a subset of tasks and related entities from the provided <see cref="ExportTaskResponseDto"/> 
        /// based on the specified list of task reward codes.
        /// </summary>
        /// <param name="taskRewardCodes">
        /// A list of task reward codes to filter the data by.
        /// </param>
        /// <param name="tasks">
        /// The complete set of task-related data from which filtered data should be returned. Can be null.
        /// </param>
        /// <returns>
        private static ExportTaskResponseDto GetTasks(List<string?> taskRewardCodes, ExportTaskResponseDto? tasks)
        {
            var taskRewards = tasks.TaskReward?
                .Where(r => taskRewardCodes.Contains(r.TaskReward?.TaskRewardCode ?? ""))
                .ToList() ?? new();

            var rewardIds = taskRewards
                .Select(r => r.TaskReward!.TaskRewardId)
                .ToHashSet();

            var taskIds = taskRewards
                .Select(r => r.TaskReward?.TaskId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToHashSet();

            var taskCatCodes = tasks.Task?
                .Where(t => taskIds.Contains(t.Task.TaskId))
                .Select(t => t.TaskCategoryCode)
                .ToHashSet() ?? new();

            var tosIds = tasks.TaskDetail?
                .Where(td => taskIds.Contains(td.TaskId) && td.TermsOfServiceId != null)
                .Select(td => td.TermsOfServiceId)
                .ToHashSet() ?? new();

            var externalCodes = taskRewards
                .Select(r => r.TaskReward?.TaskExternalCode)
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .ToHashSet();

            var trivia = tasks.Trivia?
                .Where(t => rewardIds.Contains(t.Trivia.TaskRewardId))
                .ToList() ?? new();

            var triviaIds = trivia
                .Select(t => t.Trivia.TriviaId)
                .ToHashSet();

            var triviaQGs = tasks.TriviaQuestionGroup?
                .Where(g => triviaIds.Contains(g.TriviaQuestionGroupId))
                .ToList() ?? new();

            var triviaQIds = triviaQGs
                .Select(g => g.TriviaQuestionId)
                .ToHashSet();

            return new ExportTaskResponseDto
            {
                TaskReward = taskRewards,
                Task = tasks.Task?.Where(t => taskIds.Contains(t.Task.TaskId)).ToList(),
                TaskDetail = tasks.TaskDetail?.Where(td => taskIds.Contains(td.TaskId)).ToList(),
                SubTask = tasks.SubTask?.Where(st => rewardIds.Contains(st.ParentTaskRewardId)).ToList(),
                TenantTaskCategory = tasks.TenantTaskCategory?.Where(tc => taskCatCodes.Contains(tc.TaskCategoryCode)).ToList(),
                TermsOfService = tasks.TermsOfService?.Where(tos => tosIds.Contains(tos.TermsOfServiceId)).ToList(),
                TaskExternalMapping = tasks.TaskExternalMapping?.Where(map => externalCodes.Contains(map.TaskExternalCode)).ToList(),
                Trivia = trivia,
                TriviaQuestionGroup = triviaQGs,
                TriviaQuestion = tasks.TriviaQuestion?.Where(q => triviaQIds.Contains(q.TriviaQuestionId)).ToList()
            };
        }

        /// <summary>
        /// Exports CMS data for a given tenant code and export options.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which cms data to be exported.</param>
        private async System.Threading.Tasks.Task ExportCmsAsync(string tenantCode, List<string> cmsComponentCodes = null)
        {
            const string methodName = nameof(ExportCmsAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var cmsData = new ExportCmsResponseDto();

                var exportCmsRequestDto = new ExportCmsRequestDto
                {
                    TenantCode = tenantCode,
                    componentCodesList = cmsComponentCodes
                };

                if (cmsComponentCodes != null && cmsComponentCodes.Count > 0)
                {
                    cmsData = await _cmsClient.Post<ExportCmsResponseDto>(Constant.CmsExportAPIUrl, exportCmsRequestDto);
                    // Filter the components based on provided component codes
                    cmsData.Components = cmsData.Components?
                        .Where(c => c.Component != null && cmsComponentCodes.Contains(c.Component.ComponentCode!))
                        .ToList();
                }
                else
                {

                    _logger.LogInformation("{ClassName}.{MethodName} - Sending request to CmsExportAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                    cmsData = await _cmsClient.Post<ExportCmsResponseDto>(Constant.CmsExportAPIUrl, exportCmsRequestDto);
                }
                if (cmsData != null && cmsData.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching cms data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}", _className, methodName, tenantCode, cmsData.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched cms data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }

                var cmsExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.CMS.ToString(),
                    Data = new
                    {
                        Component = cmsData?.Components
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string cmsFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.CmsJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving cms data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, cmsFileName);
                await SaveJsonToS3Async(cmsExportData, cmsFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved cms data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }

        /// <summary>
        /// Exports Tenant Account data for a given tenant code and export options.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which tenant account data to be exported.</param>
        private async System.Threading.Tasks.Task ExportFISAsync(string tenantCode)
        {
            const string methodName = nameof(ExportFISAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var exportTenantRequestDto = new ExportTenantRequestDto
                {
                    TenantCode = tenantCode,
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to GetTenantAccountAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var tenantAccountResponse = await _fisClient.Post<ExportTenantAccountResponseDto>(Constant.GetTenantAccountAPIUrl, exportTenantRequestDto);

                if (tenantAccountResponse != null && tenantAccountResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching tenant account data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}", _className, methodName, tenantCode, tenantAccountResponse.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched tenant account data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }
                var tenantConfig= await ProcessFisTenantProgramExport(tenantCode);
                var fisExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.FIS.ToString(),
                    Data = new
                    {
                        TenantAccount = tenantAccountResponse?.TenantAccount,
                        TenantProgramConfig= tenantConfig.TenantProgramConfigDto
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string fisFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.FisJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving tenant account data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, fisFileName);
                await SaveJsonToS3Async(fisExportData, fisFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved tenant account data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }
        private async System.Threading.Tasks.Task<TenantProgramConfigExportResponseDto> ProcessFisTenantProgramExport(string tenantCode)
        {
            const string methodName = nameof(ProcessFisTenantProgramExport);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var exportTenantRequestDto = new TenantProgramConfigExportRequestDto
                {
                    TenantCode = tenantCode,
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to GetTenantAccountAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var tenantProgramResponse = await _fisClient.Post<TenantProgramConfigExportResponseDto>(Constant.GetTenantProgramConfigAPIUrl, exportTenantRequestDto);

                if (tenantProgramResponse != null && tenantProgramResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching tenant program config data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}", _className, methodName, tenantCode, tenantProgramResponse.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched tenant program config for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }
                return tenantProgramResponse;
              }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }

        /// <summary>
        /// Exports Sweepstakes data for a given tenant code and export options.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which sweepstakes data to be exported.</param>
        private async System.Threading.Tasks.Task ExportSweepstakesAsync(string tenantCode)
        {
            const string methodName = nameof(ExportSweepstakesAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var exportSweepstakesRequestDto = new ExportSweepstakesRequestDto
                {
                    TenantCode = tenantCode,
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to SweepstakesExportAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var sweepstakesResponse = await _sweepstakesClient.Post<ExportSweepstakesResponseDto>(Constant.SweepstakesExportAPIUrl, exportSweepstakesRequestDto);

                if (sweepstakesResponse != null && sweepstakesResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching sweepstakes data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}", _className, methodName, tenantCode, sweepstakesResponse.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched sweepstakes data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }

                var sweepstakesExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.SWEEPSTAKES.ToString(),
                    Data = new
                    {
                        Sweepstakes = sweepstakesResponse?.Sweepstakes,
                        TenantSweepstakes = sweepstakesResponse?.TenantSweepstakes
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string sweepstakesFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.SweepstakesJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving sweepstakes data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, sweepstakesFileName);
                await SaveJsonToS3Async(sweepstakesExportData, sweepstakesFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved sweepstakes data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }

        /// <summary>
        /// Creates a task data object based on the provided tasks and export options.
        /// </summary>
        /// <param name="tasks">The tasks to be included in the export.</param>
        /// <param name="exportOptions">The export options specifying which fields to include.</param>
        /// <returns>An object containing the task data to be exported.</returns>
        private object CreateTaskDataObject(ExportTaskResponseDto? tasks, string[] exportOptions)
        {
            object data = new
            {
                Task = tasks?.Task,
                SubTask = tasks?.SubTask,
                TenantTaskCategory = tasks?.TenantTaskCategory,
                TaskDetail = tasks?.TaskDetail,
                TaskReward = tasks?.TaskReward,
                TaskExternalMapping = tasks?.TaskExternalMapping,
                TermsOfService = tasks?.TermsOfService,
                Questionnaire = tasks?.Questionnaire,
                QuestionnaireQuestionGroup = tasks?.QuestionnaireQuestionGroup,
                QuestionnaireQuestion = tasks?.QuestionnaireQuestion
            };
            // Check if it is Export_all -> Task, Trivia and Questionnaire fields need to be included
            if (exportOptions.Contains(ExportOption.EXPORT_ALL.ToString()))
            {
                data = new
                {
                    Task = tasks?.Task,
                    SubTask = tasks?.SubTask,
                    TenantTaskCategory = tasks?.TenantTaskCategory,
                    TaskDetail = tasks?.TaskDetail,
                    TaskReward = tasks?.TaskReward,
                    TaskExternalMapping = tasks?.TaskExternalMapping,
                    TermsOfService = tasks?.TermsOfService,
                    Trivia = tasks?.Trivia,
                    TriviaQuestionGroup = tasks?.TriviaQuestionGroup,
                    TriviaQuestion = tasks?.TriviaQuestion,
                    Questionnaire = tasks?.Questionnaire,
                    QuestionnaireQuestionGroup = tasks?.QuestionnaireQuestionGroup,
                    QuestionnaireQuestion = tasks?.QuestionnaireQuestion
                };
            }
            // Check if trivia fields need to be included
            else if (exportOptions.Contains(ExportOption.EXPORT_ALL.ToString()) || exportOptions.Contains(ExportOption.TRIVIA.ToString()))
            {
                data = new
                {
                    Task = tasks?.Task,
                    SubTask = tasks?.SubTask,
                    TenantTaskCategory = tasks?.TenantTaskCategory,
                    TaskDetail = tasks?.TaskDetail,
                    TaskReward = tasks?.TaskReward,
                    TaskExternalMapping = tasks?.TaskExternalMapping,
                    TermsOfService = tasks?.TermsOfService,
                    Trivia = tasks?.Trivia,
                    TriviaQuestionGroup = tasks?.TriviaQuestionGroup,
                    TriviaQuestion = tasks?.TriviaQuestion
                };
            }

            return data;
        }

        /// <summary>
        /// Exports cohort data for the specified tenant and saves it to S3.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async System.Threading.Tasks.Task ExportCohortsAsync(string tenantCode, string[] exportOptions)
        {
            const string methodName = nameof(ExportCohortsAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var exportCohortRequestDto = new ExportCohortRequestDto
                {
                    TenantCode = tenantCode,
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to CohortExportAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var cohorts = await _cohortClient.Post<ExportCohortResponseDto>(Constant.CohortExportAPIUrl, exportCohortRequestDto);

                if (cohorts != null && cohorts.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching cohorts data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}", _className, methodName, tenantCode, cohorts.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched cohorts data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }
                List<CohortTenantTaskRewardExportDto> exportCohortTenantTaskRewardDto = new List<CohortTenantTaskRewardExportDto>();

                if (cohorts?.CohortTenantTaskReward != null && cohorts?.CohortTenantTaskReward.Count > 0)
                {
                    foreach (var cohortTenantTaskReward in cohorts.CohortTenantTaskReward)
                    {
                        var getRequestDto = new GetTaskRewardByCodeRequestDto()
                        {
                            TaskRewardCode = cohortTenantTaskReward.TaskRewardCode,
                        };

                        var response = await _taskClient.Post<GetTaskRewardByCodeResponseDto>("get-task-reward-by-code", getRequestDto);
                        if (response != null && response.TaskRewardDetail != null && response.TaskRewardDetail?.TaskReward != null)
                        {
                            CohortTenantTaskRewardExportDto exportDto = new();
                            exportDto.TaskExternalCode = response.TaskRewardDetail?.TaskReward.TaskExternalCode;
                            exportDto.CohortTenantTaskReward = cohortTenantTaskReward;
                            exportCohortTenantTaskRewardDto.Add(exportDto);
                        }
                        else
                        {
                            _logger.LogError("{ClassName}.{MethodName}: No task reward found for cohort tenant task export. Data: {cohortTenantTaskReward}",
                                    _className, methodName, cohortTenantTaskReward?.ToJson());
                        }
                    }

                }
                var cohortExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.COHORT.ToString(),
                    Data = new
                    {
                        Cohort = cohorts?.Cohort,
                        CohortTenantTaskReward = exportCohortTenantTaskRewardDto,
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string cohortFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.CohortJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving cohort data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, cohortFileName);
                await SaveJsonToS3Async(cohortExportData, cohortFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved cohort data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);

                if (!exportOptions.Contains(ExportOption.EXPORT_ALL.ToString()) && !exportOptions.Contains(ExportOption.TASK.ToString()))
                {
                    await ExportTasksAsync(tenantCode, exportOptions, exportCohortTenantTaskRewardDto.Select(x => x!.CohortTenantTaskReward!.TaskRewardCode).Distinct().ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }



        /// <summary>
        /// Generates a checksum file for all JSON files in the specified S3 folder and uploads it to S3.
        /// </summary>
        /// <param name="s3Folder">The S3 folder path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async System.Threading.Tasks.Task GenerateChecksumAsync(string s3Folder)
        {
            var s3BucketName = _secretHelper.GetAwsTmpS3BucketName();
            s3Folder = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}";
            var files = await _s3Service.GetListOfFilesInFolder(s3BucketName, s3Folder);
            var checksums = new List<string>();
            foreach (var file in files)
            {
                var fileContent = await _s3Service.GetFileContent(s3BucketName, file);
                var hash = ComputeSha256Checksum(fileContent);
                checksums.Add($"{hash} {Path.GetFileName(file)}");
            }

            var checksumContent = string.Join("\n", checksums);
            string checkSumFileName = $"{s3Folder}/{ExportFileNames.ChecksumText}";

            var contentType = ContentTypes.PlainText;

            await _s3Service.UploadFile(s3BucketName, checkSumFileName, checksumContent, contentType);
        }

        /// <summary>
        /// Computes the SHA-256 checksum for the given file content.
        /// </summary>
        /// <param name="fileContent">The content of the file.</param>
        /// <returns>A string representing the SHA-256 checksum.</returns>
        private string ComputeSha256Checksum(byte[] fileContent)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(fileContent);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Gets the S3 folder name for exporting tenant data based on the tenant code.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <returns>The S3 folder name for the tenant export.</returns>
        private string GetTenantExportS3FolderName(string tenantCode)
        {
            return $"{Constant.TenantExportFolderName}/{tenantCode}";
        }

        /// <summary>
        /// Saves the given data as a JSON file to S3.
        /// </summary>
        /// <param name="data">The data to be saved.</param>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async System.Threading.Tasks.Task SaveJsonToS3Async(object data, string fileName)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonContent = JsonSerializer.Serialize(data, options);
            var s3BucketName = _secretHelper.GetAwsTmpS3BucketName();
            var contentType = ContentTypes.Json;

            await _s3Service.UploadFile(s3BucketName, fileName, jsonContent, contentType);
        }

        /// <summary>
        /// Exports admin data for a given tenant code and saves it s3
        /// </summary>
        /// <param name="tenantCode">The tenant code for which admin data to be exported</param>
        private async Threading.Task ExportAdminAsync(string tenantCode)
        {
            const string methodName = nameof(ExportAdminAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to admin service for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var adminResponse = _adminService.GetAdminScripts(tenantCode);

                if (adminResponse != null && adminResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching admin data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}",
                        _className, methodName, tenantCode, adminResponse.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched admin data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }

                var adminExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.ADMIN.ToString(),
                    Data = new
                    {
                        Scripts = adminResponse?.Scripts,
                        EventHandlerScripts = adminResponse?.EventHandlerScripts,
                        TenantTaskRewardScripts = adminResponse?.TenantTaskRewardScripts
                    }
                };
                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string adminFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.AdminJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving admin data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, adminFileName);
                await SaveJsonToS3Async(adminExportData, adminFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved admin data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }

        /// <summary>
        /// Exports adventures for a given tenant code and export options.
        /// </summary>
        /// <param name="tenantCode">The tenant code for what adventures are to be exported.</param>
        private async System.Threading.Tasks.Task ExportAdventuresAsync(string tenantCode, string[] exportOptions)
        {
            const string methodName = nameof(ExportAdventuresAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var exportAdventureRequestDto = new ExportAdventureRequestDto
                {
                    TenantCode = tenantCode,
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to AdventureExportAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var adventures = await _taskClient.Post<ExportAdventureResponseDto>(Constant.AdventureExportAPIUrl, exportAdventureRequestDto);

                if (adventures != null && adventures.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching tasks data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}",
                        _className, methodName, tenantCode, adventures.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched tasks data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }
                var allCmsComponentCodes = adventures!.Adventures
                  .Where(a => !string.IsNullOrWhiteSpace(a.CmsComponentCode))
                  .Select(a => a.CmsComponentCode!)
                  .Distinct()
                  .ToList();

                if (allCmsComponentCodes != null && !exportOptions.Contains(ExportOption.EXPORT_ALL.ToString()) && !exportOptions.Contains(ExportOption.CMS.ToString()))
                {
                    await ExportCmsAsync(tenantCode, allCmsComponentCodes);

                }

                if (!exportOptions.Contains(ExportOption.EXPORT_ALL.ToString()) && !exportOptions.Contains(ExportOption.CMS.ToString()))
                {
                    allCmsComponentCodes = adventures!.Adventures
                                                .Where(a => !string.IsNullOrWhiteSpace(a.CmsComponentCode))
                                                .Select(a => a.CmsComponentCode!)
                                                .Distinct()
                                                .ToList();
                    await ExportCmsAsync(tenantCode, allCmsComponentCodes);

                }

                var adventuresExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.TASK.ToString(),
                    Data = new
                    {
                        Adventure = adventures?.Adventures,
                        TenantAdventure = adventures?.TenantAdventures
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string adventureFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.AdventureJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving tasks data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, adventureFileName);
                await SaveJsonToS3Async(adventuresExportData, adventureFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved tasks data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }
        /// <summary>
        /// Exports taskReward Collection for a given tenant code and export options.
        /// </summary>
        /// <param name="tenantCode">The tenant code for what taskrewards that are associated to be exported.</param>

        private async System.Threading.Tasks.Task ExportTaskRewardCollectionsAsync(string tenantCode)
        {
            const string methodName = nameof(ExportTaskRewardCollectionsAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var exportTaskRewardCollection = new ExportTaskRewardCollectionRequestDto
                {
                    TenantCode = tenantCode,
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to AdventureExportAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var taskCollections = await _taskClient.Post<ExportTaskRewardCollectionResponseDto>(Constant.TaskRewardCollectionExportAPIUrl, exportTaskRewardCollection);

                if (taskCollections != null && taskCollections.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching tasks Collection data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}",
                        _className, methodName, tenantCode, taskCollections.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched tasks collections data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }

                var taskCollectionExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.TASK.ToString(),
                    Data = new
                    {
                        TaskRewardCollections = taskCollections?.TaskRewardCollections,
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string taskCollectionFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.TaskRewardCollectionJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving tasks data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, taskCollectionFileName);
                await SaveJsonToS3Async(taskCollectionExportData, taskCollectionFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved tasks data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }
        /// <summary>
        /// Exports admin data for a given tenant code and saves it s3
        /// </summary>
        /// <param name="tenantCode">The tenant code for which admin data to be exported</param>

        private async Threading.Task ExportAdminAsync(string tenantCode, string[] exportOptions)
        {
            const string methodName = nameof(ExportAdminAsync);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to admin service for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var adminResponse = _adminService.GetAdminScripts(tenantCode);

                if (adminResponse != null && adminResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching admin data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}",
                        _className, methodName, tenantCode, adminResponse.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched admin data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }

                var adminExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.ADMIN.ToString(),
                    Data = new
                    {
                        Scripts = adminResponse?.Scripts,
                        EventHandlerScripts = adminResponse?.EventHandlerScripts,
                        TenantTaskRewardScripts = adminResponse?.TenantTaskRewardScripts
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string adminFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.AdminJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving admin data to S3 for TenantCode: {TenantCode}, FileName: {FileName}", _className, methodName, tenantCode, adminFileName);
                await SaveJsonToS3Async(adminExportData, adminFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved admin data to S3 for TenantCode: {TenantCode}", _className, methodName, tenantCode);

                if (!exportOptions.Contains(ExportOption.EXPORT_ALL.ToString()) && !exportOptions.Contains(ExportOption.TASK.ToString()))
                {
                    await ExportTasksAsync(tenantCode, exportOptions, adminResponse!.TenantTaskRewardScripts.Select(x => x.TaskRewardCode).Distinct().ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }

        /// <summary>
        /// Exports wallet type transfer rule for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task ExportWallet(string tenantCode)
        {
            const string methodName = nameof(ExportWallet);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode: {TenantCode}", _className, methodName, tenantCode);

            try
            {
                var exportWalletTypeTransferRuleRequest = new ExportWalletTypeTransferRuleRequestDto
                {
                    TenantCode = tenantCode,
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Sending request to WalletTypeTransferRuleExportAPI for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                var response = await _walletClient.Post<ExportWalletTypeTransferRuleResponseDto>(Constant.WalletTypeTransferRuleExportAPIUrl, exportWalletTypeTransferRuleRequest);

                if (response != null && response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while fetching walletType transfer rule data for TenantCode: {TenantCode}, ErrorResponse: {ErrorResponse}",
                        _className, methodName, tenantCode, response.ToJson());
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully fetched walletType transfer rule data for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                }

                var walletExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.WALLET.ToString(),
                    Data = new
                    {
                        WalletTypeTransferRule = response?.WalletTypeTransferRules
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string walletFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.WalletJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving walletType transfer rule data to S3 for TenantCode: {TenantCode}, FileName: {FileName}",
                    _className, methodName, tenantCode, walletFileName);
                await SaveJsonToS3Async(walletExportData, walletFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved walletType transfer rule data to S3 for TenantCode: {TenantCode}",
                    _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} ended for TenantCode: {TenantCode}", _className, methodName, tenantCode);
            }
        }

        /// <summary>
        /// Export metadata tables
        /// </summary>
        /// <param name="exportOptions"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task ExportMetadata(string tenantCode, string[] exportOptions)
        {
            const string methodName = nameof(ExportMetadata);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started exporting metadata tables", _className, methodName);

                // Sort the list based on the enum order
                var sortedExportOptions = exportOptions.Distinct().ToList();

                if (sortedExportOptions.Contains(ExportOption.EXPORT_ALL.ToString()))
                {
                    sortedExportOptions = [ExportOption.EXPORT_ALL.ToString()];
                }
                List<TaskTypeDto> taskTypes = [];
                List<TaskCategoryDto> taskCategories = [];
                List<TaskRewardTypeDto> rewardTypes = [];
                List<WalletTypeDto> walletTypes = [];
                IList<ComponentTypeDto> componentTypes = [];

                foreach (var option in sortedExportOptions)
                {
                    switch (option.ToUpper())
                    {
                        case nameof(ExportOption.CMS):
                            componentTypes = await FetchComponentTypesAsync();
                            break;
                        case nameof(ExportOption.TASK):
                            (taskTypes, taskCategories, rewardTypes) = await FetchTaskMetadataAsync();
                            break;
                        case nameof(ExportOption.WALLET):
                            walletTypes = await FetchWalletTypesAsync();
                            break;
                        case nameof(ExportOption.EXPORT_ALL):
                            componentTypes = await FetchComponentTypesAsync();
                            (taskTypes, taskCategories, rewardTypes) = await FetchTaskMetadataAsync();
                            walletTypes = await FetchWalletTypesAsync();
                            break;
                        default:
                            _logger.LogError("{ClassName}.{MethodName}:invalid export option.", _className, methodName);
                            break;
                    }
                }

                var metadataExportData = new ExportDto
                {
                    ExportVersion = _secretHelper.GetExportTenantVersion(),
                    ExportTs = DateTime.UtcNow,
                    ExportType = ExportType.METADATA.ToString(),
                    Data = new
                    {
                        ComponentType = componentTypes,
                        TaskType = taskTypes,
                        TaskCategory = taskCategories,
                        RewardType = rewardTypes,
                        WalletType = walletTypes
                    }
                };

                var s3Folder = GetTenantExportS3FolderName(tenantCode);
                string metadataFileName = $"{s3Folder}/{Constant.ExportJsonFilesFolderName}/{ExportFileNames.MetadataJson}";

                _logger.LogInformation("{ClassName}.{MethodName} - Saving metadata to S3 for TenantCode: {TenantCode}, FileName: {FileName}",
                    _className, methodName, tenantCode, metadataFileName);
                await SaveJsonToS3Async(metadataExportData, metadataFileName);

                _logger.LogInformation("{ClassName}.{MethodName} - Successfully saved metadata to S3 for TenantCode: {TenantCode}",
                    _className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred for TenantCode: {TenantCode}", _className, methodName, tenantCode);
                throw;
            }
        }

        /// <summary>
        /// FetchTaskMetadataAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServiceValidationException"></exception>
        private async Task<(List<TaskTypeDto>, List<TaskCategoryDto>, List<TaskRewardTypeDto>)> FetchTaskMetadataAsync()
        {
            const string methodName = nameof(FetchTaskMetadataAsync);
            try
            {
                #region TaskType Metadata
                //Fetch Task type metadata
                var taskTypesResponse = await _taskTypeService.GetTaskTypesAsync();
                if (taskTypesResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while fetching task categories, ErrorCode: {ErrorCode},Error:{Msg}",
                            _className, methodName, taskTypesResponse.ErrorCode, taskTypesResponse.ErrorMessage);
                    throw new ServiceValidationException(taskTypesResponse.ErrorMessage ?? string.Empty, taskTypesResponse.ErrorCode);

                }
                var taskTypes = taskTypesResponse.TaskTypes?.ToList() ?? new List<TaskTypeDto>();
                #endregion

                #region TaskCategory Metadata
                //Fetch Task category metadata
                var taskCategoriesResponse = await _taskCategoryService.GetTaskCategoriesAsync();
                if (taskCategoriesResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while fetching task categories, ErrorCode: {ErrorCode},Error:{Msg}",
                            _className, methodName, taskCategoriesResponse.ErrorCode, taskCategoriesResponse.ErrorMessage);
                    throw new ServiceValidationException(taskCategoriesResponse.ErrorMessage ?? string.Empty, taskCategoriesResponse.ErrorCode);

                }
                var taskCategories = taskCategoriesResponse.TaskCategories?.ToList() ?? new List<TaskCategoryDto>();
                #endregion

                #region RewardType Metadata
                //Fetch reward type metadata
                var rewardTypesResponse = await _taskRewardTypeService.GetTaskRewardTypesAsync();
                if (rewardTypesResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while fetching reward types, ErrorCode: {ErrorCode},Error:{Msg}",
                            _className, methodName, rewardTypesResponse.ErrorCode, rewardTypesResponse.ErrorMessage);
                    throw new ServiceValidationException(rewardTypesResponse.ErrorMessage ?? string.Empty, rewardTypesResponse.ErrorCode);
                }
                var rewardTypes = rewardTypesResponse.TaskRewardTypes?.ToList() ?? new List<TaskRewardTypeDto>();
                #endregion

                return (taskTypes, taskCategories, rewardTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while fetching task metadata. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}",
                    _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
        /// <summary>
        /// Fetchs component types
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServiceValidationException"></exception>
        private async Task<IList<ComponentTypeDto>> FetchComponentTypesAsync()
        {
            const string methodName = nameof(FetchComponentTypesAsync);
            try
            {
                var componentTypeResponse = await _componentService.GetAllComponentTypes();
                if (componentTypeResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while fetching component types, ErrorCode: {ErrorCode},Error:{Msg}",
                            _className, methodName, componentTypeResponse.ErrorCode, componentTypeResponse.ErrorMessage);
                    throw new ServiceValidationException(componentTypeResponse.ErrorMessage ?? string.Empty, componentTypeResponse.ErrorCode);
                }
                return componentTypeResponse.ComponentTypes ?? new List<ComponentTypeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while fetching component type metadata. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}",
                    _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
        /// <summary>
        /// Fetches wallet types
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServiceValidationException"></exception>
        private async Task<List<WalletTypeDto>> FetchWalletTypesAsync()
        {
            const string methodName = nameof(FetchWalletTypesAsync);
            try
            {
                var walletTypeResponse = await _walletTypeService.GetAllWalletTypes();
                if (walletTypeResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while fetching component types, ErrorCode: {ErrorCode},Error:{Msg}",
                            _className, methodName, walletTypeResponse.ErrorCode, walletTypeResponse.ErrorMessage);
                    throw new ServiceValidationException(walletTypeResponse.ErrorMessage ?? string.Empty, walletTypeResponse.ErrorCode);
                }
                return walletTypeResponse.WalletTypes ?? new List<WalletTypeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while fetching component type metadata. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}",
                    _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
