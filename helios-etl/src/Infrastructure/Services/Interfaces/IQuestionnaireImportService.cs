using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IQuestionnaireImportService
    {
        /// <summary>
        /// Imports Questionnaire files for given tenant code
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <returns></returns>
        Task Import(EtlExecutionContext etlExecutionContext);
    }
}
