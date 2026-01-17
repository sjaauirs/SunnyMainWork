using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ITaskCompletionCheckerService
    {
        public string? CurrentConsumerCode { get; set; }
        bool CheckConsumerTaskCompleted(object tenantCode, object consumerCode, object taskExternalCode);
        bool CheckConsumerTaskEnrolled(object tenantCode, object consumerCode, object taskExternalCode);
    }
}
