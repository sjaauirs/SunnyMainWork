using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    public class TaskRewardCollectionRepo:BaseRepo<TaskRewardCollectionModel>,ITaskRewardCollectionRepo
    {
        private readonly NHibernate.ISession _session;
        public TaskRewardCollectionRepo(ILogger<BaseRepo<TaskRewardCollectionModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public async Task<ExportTaskRewardCollectionResponseDto> GetTaskRewardCollections(string tenantCode)
        {
            var query = await (from collection in _session.Query<TaskRewardCollectionModel>()
                               join parent in _session.Query<TaskRewardModel>()
                                   on collection.ParentTaskRewardId equals parent.TaskRewardId
                               join child in _session.Query<TaskRewardModel>()
                                   on collection.ChildTaskRewardId equals child.TaskRewardId
                               where parent.IsCollection == true
                                     && parent.TenantCode == tenantCode
                                     && child.TenantCode == tenantCode
                                     && collection.DeleteNbr == 0
                                     && parent.DeleteNbr == 0
                                     && child.DeleteNbr == 0
                               select new ExportTaskRewardCollectionDto
                               {
                                   TaskRewardCollectionId = collection.TaskRewardCollectionId,
                                   ParentTaskRewardId = collection.ParentTaskRewardId,
                                   ParentTaskRewardCode = parent.TaskRewardCode,
                                   ChildTaskRewardId = collection.ChildTaskRewardId,
                                   ChildTaskRewardCode = child.TaskRewardCode,
                                   UniqueChildCode = collection.UniqueChildCode,
                                   ConfigJson = collection.ConfigJson
                               }).ToListAsync();

            return new ExportTaskRewardCollectionResponseDto
            {
                TaskRewardCollections = query
            };
        }
    }
}
