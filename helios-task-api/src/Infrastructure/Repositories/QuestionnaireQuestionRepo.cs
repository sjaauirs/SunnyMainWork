using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireQuestionRepo : BaseRepo<QuestionnaireQuestionModel>, IQuestionnaireQuestionRepo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireQuestionRepo"/> class.
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public QuestionnaireQuestionRepo(ILogger<BaseRepo<QuestionnaireQuestionModel>> baseLogger, ISession session) : base(baseLogger, session)
        {
        }
    }
}
