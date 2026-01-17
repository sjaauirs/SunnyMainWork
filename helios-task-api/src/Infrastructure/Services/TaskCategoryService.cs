using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System.Text;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="taskCategoryRepo"></param>
    public class TaskCategoryService: BaseService, ITaskCategoryService
    {
        private readonly ILogger<TaskCategoryService> _logger;
        private readonly IMapper _mapper;
        private readonly ITaskCategoryRepo _taskCategoryRepo;
        const string className = nameof(TaskCategoryService);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="taskCategoryRepo"></param>
        public TaskCategoryService(ILogger<TaskCategoryService> logger, IMapper mapper, ITaskCategoryRepo taskCategoryRepo) 
        {
            _logger = logger;
            _mapper = mapper;
            _taskCategoryRepo = taskCategoryRepo;
        }

        /// <summary>
        /// Retrieves a list of task categories from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        public async Task<TaskCategoriesResponseDto> GetTaskCategoriesAsync()
        {
            const string methodName = nameof(GetTaskCategoriesAsync); // Use consistent method naming for logging
            try
            {
                var result = await _taskCategoryRepo.FindAsync(x => x.DeleteNbr == 0);

                if (result == null || result.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No task category was found. Error Code: {ErrorCode}", className, methodName, StatusCodes.Status404NotFound);
                    
                    return new TaskCategoriesResponseDto
                    {
                        ErrorMessage = "No task category was found."
                    };
                }

                return new TaskCategoriesResponseDto
                {
                    TaskCategories = _mapper.Map<IList<TaskCategoryDto>>(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TaskCategoriesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Imports a predefined list of task categories. 
        /// Skips existing categories and returns detailed response on import status.
        /// </summary>
        /// <returns>Response DTO indicating success or partial failure.</returns>
        public async Task<ImportTaskCategoryResponseDto> ImportTaskCategoriesAsync(ImportTaskCategoryRequestDto taskCategoryRequestDto)
        {
            const string methodName = nameof(ImportTaskCategoriesAsync);
            var responseDto = new ImportTaskCategoryResponseDto();
            var errorMessages = new StringBuilder();

            _logger.LogInformation("{ClassName}.{MethodName} - Starting import of {Count} task categories.",
                className, methodName, taskCategoryRequestDto.TaskCategories.Count);

            foreach (var category in taskCategoryRequestDto.TaskCategories)
            {
                try
                {
                    var existingCategory = await _taskCategoryRepo.FindOneAsync(x =>
                        x.TaskCategoryCode == category.TaskCategoryCode && x.DeleteNbr == 0);

                    if (existingCategory != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - TaskCategory '{Code}' already exists. Skipping.", 
                            className, methodName, category.TaskCategoryCode);
                        continue;
                    }

                    await CreateTaskCategoryAsync(category);
                    _logger.LogInformation("{ClassName}.{MethodName} - TaskCategory '{Code}' created successfully.", 
                        className, methodName, category.TaskCategoryCode);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Failed to import TaskCategory '{category.TaskCategoryCode}': {ex.Message}";
                    _logger.LogError(ex, "{ClassName}.{MethodName} - {Error}",className, methodName, errorMsg);
                    errorMessages.AppendLine(errorMsg);
                }
            }

            if (errorMessages.Length > 0)
            {
                responseDto.ErrorCode = StatusCodes.Status206PartialContent;
                responseDto.ErrorMessage = errorMessages.ToString();
                _logger.LogWarning("{ClassName}.{MethodName}- Import completed with some errors.",className, methodName);
            }
            
            _logger.LogInformation("{ClassName}.{MethodName}- All task categories imported successfully.",className, methodName);

            return responseDto;
        }

        /// <summary>
        /// Creates a new task category in the database.
        /// </summary>
        /// <param name="categoryDto">DTO containing task category details.</param>
        /// <returns>The created TaskCategoryModel.</returns>
        private async Task<TaskCategoryModel> CreateTaskCategoryAsync(TaskCategoryDto categoryDto)
        {
            const string methodName = nameof(CreateTaskCategoryAsync);

            var categoryModel = _mapper.Map<TaskCategoryModel>(categoryDto);
            categoryModel.CreateTs = DateTime.UtcNow;
            categoryModel.CreateUser = Constant.ImportUser;

            await _taskCategoryRepo.CreateAsync(categoryModel);

            _logger.LogInformation("{ClassName}.{MethodName} - TaskCategory '{Code}' created successfully.",className, methodName, categoryModel.TaskCategoryCode);
            return categoryModel;
        }
    }
}