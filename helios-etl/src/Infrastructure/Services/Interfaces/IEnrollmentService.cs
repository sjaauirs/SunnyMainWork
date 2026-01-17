using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IEnrollmentService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> ProcessTenantEnrollments(EtlExecutionContext etlExecutionContext);

    }
}