using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IEventingWrapperService
    {
        Task<List<(long RowId, bool Published, string Error)>> PublishMessagesInParallelAsync(
             List<(string EventMessage, long RowId, string EventId, string EventType, string PersonUniqueIdentifier)> messages,
             string jobId,
             string topicName);
    }
}
