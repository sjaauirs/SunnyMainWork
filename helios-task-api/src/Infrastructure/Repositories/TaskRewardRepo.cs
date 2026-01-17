using AutoMapper;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class TaskRewardRepo : BaseRepo<TaskRewardModel>, ITaskRewardRepo
    {
        private readonly ISession _session;
        private readonly IMapper _mapper;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TaskRewardRepo(ILogger<BaseRepo<TaskRewardModel>> baseLogger, NHibernate.ISession session, IMapper mapper) : base(baseLogger, session)
        {
            _session = session;
            _mapper = mapper;
        }
        public async Task<List<TaskAndTaskRewardModel>> GetTasksAndTaskRewards(GetTasksAndTaskRewardsRequestDto requestDto)
        {
            var tasksAndTaskRewards = (from ts in _session.Query<TaskModel>()
                                       join tr in _session.Query<TaskRewardModel>()
                                       on ts.TaskId equals tr.TaskId
                                       where tr.TenantCode == requestDto.TenantCode
                                             && tr.DeleteNbr == 0
                                             && ts.DeleteNbr == 0
                                       orderby ts.TaskId
                                       select new TaskAndTaskRewardModel
                                       {
                                           Task = ts,
                                           TaskReward = tr
                                       }).ToListAsync();

            return await tasksAndTaskRewards;
        }

        public async Task<List<TaskRewardModel>> GetSelfReportTaskRewards(GetSelfReportTaskReward requestDto)
        {
            var tasksAndTaskRewards = (from ts in _session.Query<TaskModel>()
                                       join tr in _session.Query<TaskRewardModel>()
                                       on ts.TaskId equals tr.TaskId
                                       where tr.TenantCode == requestDto.TenantCode
                                             && tr.DeleteNbr == 0
                                             && ts.DeleteNbr == 0
                                             && ts.SelfReport == requestDto.selfReport
                                             && tr.TaskCompletionCriteriaJson != null
                                       orderby ts.TaskId
                                       select tr).ToListAsync();

            return await tasksAndTaskRewards;
        }

        /// <summary>
        /// Retrieves the list of task reward details for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>

        public async Task<List<TaskRewardDetailModel>> GetTaskRewardDetails(string tenantCode, string? taskExternalCode, string languageCode)
        {
            var fallbackLanguage = Constant.LanguageCode;

            // Step 1: Get TaskRewards
            var taskRewards = await _session.Query<TaskRewardModel>()
                .Where(tr => tr.TenantCode == tenantCode && tr.DeleteNbr == 0 &&
                             (string.IsNullOrEmpty(taskExternalCode) || tr.TaskExternalCode == taskExternalCode))
                .ToListAsync();

            var taskIds = taskRewards.Select(tr => tr.TaskId).Distinct().ToList();

            // Step 2: Get related TaskModels
            var tasks = await _session.Query<TaskModel>()
                .Where(ts => ts.DeleteNbr == 0 && taskIds.Contains(ts.TaskId))
                .ToListAsync();

            // Step 3: Get TaskDetails with both preferred and fallback languages
            var taskDetails = await _session.Query<TaskDetailModel>()
                .Where(td => td.DeleteNbr == 0 &&
                             td.TenantCode == tenantCode &&
                             taskIds.Contains(td.TaskId) &&
                             (td.LanguageCode == languageCode || td.LanguageCode == fallbackLanguage))
                .ToListAsync();

            // Step 4: Pick best-matching TaskDetail per TaskId
            var taskDetailDict = taskDetails
                .GroupBy(td => td.TaskId)
                .ToDictionary(
                    g => g.Key,
                    g => g.FirstOrDefault(td => td.LanguageCode == languageCode) ??
                         g.FirstOrDefault(td => td.LanguageCode == fallbackLanguage)
                );

            // Step 5: Merge everything together
            var result = (from tr in taskRewards
                          join ts in tasks on tr.TaskId equals ts.TaskId
                          where taskDetailDict.ContainsKey(ts.TaskId)
                          select new TaskRewardDetailModel
                          {
                              Task = ts,
                              TaskReward = tr,
                              TaskDetail = taskDetailDict[ts.TaskId]
                          }).ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public List<TaskRewardDetailDto> GetTaskRewardDetailsList(string tenantCode, string languageCode)
        {
            return GetTaskRewardDetails(tenantCode, languageCode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="languageCode"></param>
        /// <param name="taskRewardIds"></param>
        /// <returns></returns>
        public List<TaskRewardDetailDto> GetTaskRewardDetailsList(string tenantCode, string languageCode, List<long> taskRewardIds)
        {
            return GetTaskRewardDetails(tenantCode, languageCode,taskRewardIds);
        }


        private List<TaskRewardDetailDto> GetTaskRewardDetails(string tenantCode, string languageCode, List<long> taskRewardIds=null)
        {
            languageCode ??= Constant.LanguageCode;
            var nowUtc = DateTime.UtcNow;

            var query = from taskReward in _session.Query<TaskRewardModel>()
                        join taskDetail in _session.Query<TaskDetailModel>() on taskReward.TaskId equals taskDetail.TaskId
                        join task in _session.Query<TaskModel>() on taskReward.TaskId equals task.TaskId
                        join taskType in _session.Query<TaskTypeModel>() on task.TaskTypeId equals taskType.TaskTypeId
                        join rewardType in _session.Query<TaskRewardTypeModel>() on taskReward.RewardTypeId equals rewardType.RewardTypeId
                        join tc in _session.Query<TenantTaskCategoryModel>() on task.TaskCategoryId equals tc.TaskCategoryId into tcJoin
                        from taskCategory in tcJoin.DefaultIfEmpty()
                        join tos in _session.Query<TermsOfServiceModel>() on taskDetail.TermsOfServiceId equals tos.TermsOfServiceId into tsGroup
                        from termsOfService in tsGroup.DefaultIfEmpty()
                        where taskReward.DeleteNbr == 0
                              && taskReward.TenantCode == tenantCode
                              && taskReward.ValidStartTs <= nowUtc
                              && taskReward.Expiry >= nowUtc
                              && taskDetail.DeleteNbr == 0
                              && taskDetail.TenantCode == tenantCode
                              && taskDetail.LanguageCode == languageCode
                              && task.DeleteNbr == 0
                              && !task.IsSubtask
                        select new
                        {
                            taskReward,
                            taskDetail,
                            task,
                            taskType,
                            rewardType,
                            taskCategory,
                            termsOfService
                        };

            if (taskRewardIds?.Count>0)
            {
                query = query.Where(x => taskRewardIds.Contains(x.taskReward.TaskRewardId));
            }

            var taskRewardDetails = query.AsEnumerable().Select(x => new TaskRewardDetailDto
            {
                TaskReward = _mapper.Map<TaskRewardDto>(x.taskReward),
                Task = _mapper.Map<TaskDto>(x.task),
                TaskDetail = _mapper.Map<TaskDetailDto>(x.taskDetail),
                TaskType = _mapper.Map<TaskTypeDto>(x.taskType),
                RewardTypeName = x.rewardType?.RewardTypeName,
                TenantTaskCategory = x.taskCategory != null ? _mapper.Map<TenantTaskCategoryDto>(x.taskCategory) : null,
                TermsOfService = x.termsOfService != null ? _mapper.Map<TermsOfServiceDto>(x.termsOfService) : null
            }).DistinctBy(x => x?.TaskReward?.TaskRewardId).ToList();

            return taskRewardDetails;
        }

    }

}

