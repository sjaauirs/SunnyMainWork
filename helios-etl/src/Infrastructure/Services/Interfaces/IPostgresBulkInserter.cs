using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IPostgresBulkInserter
    {
        Task BulkInsertAsync(string postgresConnectionString, List<ETLMemberImportFileDataModel> dataModels);
    }
}
