using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Logs
{
    public class S3FileLogger : IS3FileLogger
    {
        private readonly IAwsS3Service _awsS3Service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awsS3Service"></param>
        public S3FileLogger(IAwsS3Service awsS3Service)
        {
            _awsS3Service = awsS3Service;
        }

        /// <summary>
        /// 
        /// </summary>
        public string? S3LogFile { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task CreateFileInS3(string fileName)
        {
            S3LogFile = $"log/etl/{fileName}";
            await _awsS3Service.CreateFile($"log/etl/{fileName}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logContext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task AddErrorLogs(S3LogContext logContext)
        {
            if (string.IsNullOrEmpty(S3LogFile))
            {
                string fileName = $"etl_err_{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss")}.log";
                await CreateFileInS3(fileName);
            }

            await _awsS3Service.AppendInFile(S3LogFile, logContext.ToLogContext() + "\n");
        }

        /// <summary>
        /// true if we create logs in s3
        /// </summary>
        /// <returns></returns>
        public bool Executed()
        {
            return !string.IsNullOrEmpty(S3LogFile);
        }
    }
}
