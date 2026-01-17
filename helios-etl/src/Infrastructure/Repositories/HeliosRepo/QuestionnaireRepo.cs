using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class QuestionnaireRepo : BaseRepo<ETLQuestionnaireModel>, IQuestionnaireRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public QuestionnaireRepo(ILogger<BaseRepo<ETLQuestionnaireModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}