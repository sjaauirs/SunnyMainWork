namespace SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface
{
    public interface IS3FileLogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task CreateFileInS3(string fileName);
        Task AddErrorLogs(S3LogContext logContext);

        /// <summary>
        /// true if we create logs in s3
        /// </summary>
        /// <returns></returns>
        bool Executed();

        /// <summary>
        /// 
        /// </summary>
        string? S3LogFile { get; set; }
    }
}
