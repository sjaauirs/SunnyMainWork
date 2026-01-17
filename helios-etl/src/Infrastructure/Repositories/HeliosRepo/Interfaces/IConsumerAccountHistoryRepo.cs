using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IConsumerAccountHistoryRepo : IBaseRepo<ETLConsumerAccountHistoryModel>
    {
    }
}
