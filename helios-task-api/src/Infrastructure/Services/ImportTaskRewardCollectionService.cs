using AutoMapper;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class ImportTaskRewardCollectionService : IImportTaskRewardCollectionService
    {
        private readonly ILogger<ImportTaskRewardCollectionService> _logger;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskRewardCollectionRepo _taskRewardCollectionRepo;
        private readonly IMapper _mapper;
        private const string className = nameof(ImportTaskRewardCollectionService);

        public ImportTaskRewardCollectionService(ILogger<ImportTaskRewardCollectionService> logger, ITaskRewardRepo taskRewardRepo,
            ITaskRewardCollectionRepo taskRewardCollectionRepo, IMapper mapper)
        {
            _logger = logger;
            _taskRewardRepo = taskRewardRepo;
            _taskRewardCollectionRepo = taskRewardCollectionRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Imports or updates a collection of task reward relationships based on the provided data.
        /// For each item in the request, it validates parent and child task rewards, then creates or updates the 
        /// corresponding task reward collection record. Logs any missing references or exceptions encountered during processing.
        /// </summary>
        /// <param name="taskRewardCollections">The collection of task reward relationships to import or update.</param>
        /// <returns>
        /// A <see cref="BaseResponseDto"/> indicating the overall result of the import operation. 
        /// The response does not include item-level success or failure details.
        /// </returns>
        public async Task<BaseResponseDto> ImportTaskRewardCollection(ImportTaskRewardCollectionRequestDto taskRewardCollections)
        {
            const string methodName = nameof(ImportTaskRewardCollection);

            foreach (var item in taskRewardCollections.TaskRewardCollections)
            {
                try
                {
                    var parentTaskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == item.ParentTaskRewardCode
                                        && x.DeleteNbr == 0 && x.IsCollection == true);
                    var childTaskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == item.ChildTaskRewardCode
                                            && x.DeleteNbr == 0);
                    if (parentTaskReward == null || childTaskReward == null)
                    {
                        _logger.LogError("{className}.{methodName}: Parent task reward or Child task Reward not Found with Parent TaskRewardCode:{parent}" +
                            "Child TaskRewardCode:{child}", className, methodName, item.ParentTaskRewardCode, item.ChildTaskRewardCode);
                        continue;
                    }
                    var taskRewardCollection = await _taskRewardCollectionRepo.FindOneAsync(x => x.ParentTaskRewardId == parentTaskReward.TaskRewardId 
                                                && x.ChildTaskRewardId == childTaskReward.TaskRewardId && x.DeleteNbr == 0);
                    if (taskRewardCollection != null)
                    {
                        // Update existing TaskRewardCollection
                        _mapper.Map(new TaskRewardCollectionDto
                        {
                            ParentTaskRewardId = parentTaskReward.TaskRewardId,
                            ChildTaskRewardId = childTaskReward.TaskRewardId,
                            UniqueChildCode = item.UniqueChildCode,
                            ConfigJson = item.ConfigJson
                        }, taskRewardCollection);
                        taskRewardCollection.TaskRewardCollectionId = item.TaskRewardCollectionId;
                        taskRewardCollection.UpdateTs = DateTime.UtcNow;
                        taskRewardCollection.UpdateUser = Constant.ImportUser;

                        await _taskRewardCollectionRepo.UpdateAsync(taskRewardCollection);
                    }
                    else
                    {
                        // Create new TaskRewardCollection
                        var guid = Guid.NewGuid();
                        var newTaskRewardCollection = _mapper.Map<TaskRewardCollectionModel>(new TaskRewardCollectionDto
                        {
                            ParentTaskRewardId = parentTaskReward.TaskRewardId,
                            ChildTaskRewardId = childTaskReward.TaskRewardId,
                            UniqueChildCode = $"ucc-{guid:N}",
                            ConfigJson = item.ConfigJson
                        });

                        newTaskRewardCollection.CreateUser = Constant.ImportUser;
                        newTaskRewardCollection.CreateTs = DateTime.UtcNow;
                        newTaskRewardCollection.DeleteNbr = 0;

                        await _taskRewardCollectionRepo.CreateAsync(newTaskRewardCollection);
                    }

                    _logger.LogInformation("{className}.{methodName}: TaskReward Collection Inserted with Parent TaskRewardCode:{parent}" +
                             "Child TaskRewardCode:{child}", className, methodName, item.ParentTaskRewardCode, item.ChildTaskRewardCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.{methodName}: Exception occurred during import Task Reward Collection. For ParentTaskRewardCode:{parent}, ChildTaskRewardCode:{child}", className, methodName, item.ParentTaskRewardCode, item.ChildTaskRewardCode);
                    continue;
                }

            }
            return new BaseResponseDto();

        }
    }
}
