using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    public class TaskRewardTypeRepo : BaseRepo<TaskRewardTypeModel>, ITaskRewardTypeRepo
    {
        public TaskRewardTypeRepo(ILogger<BaseRepo<TaskRewardTypeModel>> baseLogger, ISession session) : base(baseLogger, session)
        {
                
        }
    }
}
