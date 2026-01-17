using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.RuleEngine.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.RuleEngine
{
    public class ConsumerQuery : IConsumerQuery
    {
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly ICohortRepo _cohortRepo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskRepo"></param>
        /// <param name="cohortRepo"></param>
        public ConsumerQuery(IConsumerTaskRepo consumerTaskRepo, ICohortRepo cohortRepo)
        {
            _consumerTaskRepo = consumerTaskRepo;
            _cohortRepo = cohortRepo;
        }

        /// <summary>
        /// 
        /// </summary>
        public string? ConsumerCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskExternalCode"></param>
        /// <returns></returns>
        /// <message>Can't make them async rule-engine will not accept it</message>
        public bool HasCompletedTask(object taskExternalCode)
        {
            if (string.IsNullOrWhiteSpace(ConsumerCode)) return false;

            return _consumerTaskRepo.HasCompletedTask(ConsumerCode, taskExternalCode.ToString()).Result;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        /// <message>Can't make them async rule-engine will not accept it</message>
        public bool IsInCohort(object cohortName)
        {
            if (string.IsNullOrWhiteSpace(ConsumerCode)) return false;

            return _cohortRepo.IsInCohort(ConsumerCode, cohortName.ToString()).Result;
        }
    }
}
