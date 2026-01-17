using SunnyRewards.Helios.ETL.Infrastructure.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Logs
{
    public class S3FISFileLogger : IS3FISFileLogger
    {
        private readonly IAwsS3Service _awsS3Service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="awsS3Service"></param>
        public S3FISFileLogger(IAwsS3Service awsS3Service)
        {
            _awsS3Service = awsS3Service;
        }

        /// <summary>
        /// 
        /// </summary>
        private string? S3LogFile { get; set; }

        /// <summary>
        /// Create FIS log File in S3
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<string> CreateFileInS3(string fileName)
        {
            string logFileName = $"{fileName}_{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss")}.log";
            S3LogFile = $"log/etl/FIS/{logFileName}";
            await _awsS3Service.CreateFile(S3LogFile);
            return S3LogFile;
        }

        /// <summary>
        /// AddErrorLogs
        /// </summary>
        /// <param name="logContext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task AddErrorLogs(S3FISLogContext logContext)
        {
            if (string.IsNullOrEmpty(S3LogFile))
            {
                S3LogFile = await CreateFileInS3(logContext.LogFileName);
            }
            await _awsS3Service.AppendInFile(S3LogFile, logContext.ToLogContext() + "\n");
            // throw Etl Exception 
            if (logContext.throwEtlError)
            {
                throw new EtlJobException(logContext.ToLogContext());
            }
        }
    }
}
