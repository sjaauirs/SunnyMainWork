
namespace SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface
{
    public interface IS3FISFileLogger
    {
        Task AddErrorLogs(S3FISLogContext logContext);
    }
}
