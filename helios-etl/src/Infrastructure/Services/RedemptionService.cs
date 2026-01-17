using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class RedemptionService : IRedemptionService
    {
        private readonly ILogger<RedemptionService> _logger;
        private readonly IRedemptionRepo _redemptionRepo;
        private const string className=nameof(RedemptionService);

        public RedemptionService(ILogger<RedemptionService> logger, IRedemptionRepo redemptionRepo)
        {
            _logger = logger;
            _redemptionRepo = redemptionRepo;
        }

        public ETLRedemptionModel? GetRedemptionWithRedemptionRef(string redemptionRef)
        {
            const string methodName = nameof(GetRedemptionWithRedemptionRef);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing GetRedemption with RedemptionRef:{Ref} ", className, methodName, redemptionRef);
                return _redemptionRepo.GetRedemptionWithRedemptionRef(redemptionRef);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while get redemption with RedemptionRef:{Ref}", className, methodName,redemptionRef);
                throw;
            }
        }

        public int UpdateRedemptionStatus(string redemptionRef, string redemptionStatus)
        {
            const string methodName=nameof(UpdateRedemptionStatus);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing GetRedemption with RedemptionRef:{Ref} ", className, methodName, redemptionRef);
                return _redemptionRepo.UpdateRedemptionStatus(DateTime.UtcNow, redemptionStatus, redemptionRef);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while update redemption with RedemptionRef:{Ref}", className, methodName, redemptionRef);
                throw;
            }
        }
    }
}
