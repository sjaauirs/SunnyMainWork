using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    public class TaskExternalMappingRepo : BaseRepo<TaskExternalMappingModel>, ITaskExternalMappingRepo
    {
        public TaskExternalMappingRepo(ILogger<BaseRepo<TaskExternalMappingModel>> baseLogger, NHibernate.ISession session) :
           base(baseLogger, session)
        {
        }
    }
}
