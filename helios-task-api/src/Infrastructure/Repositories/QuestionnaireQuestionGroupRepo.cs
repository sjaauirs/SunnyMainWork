using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireQuestionGroupRepo : BaseRepo<QuestionnaireQuestionGroupModel>, IQuestionnaireQuestionGroupRepo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireQuestionGroupRepo"/> class.
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public QuestionnaireQuestionGroupRepo(ILogger<BaseRepo<QuestionnaireQuestionGroupModel>> baseLogger, ISession session) : base(baseLogger, session)
        {
        }
    }
}
