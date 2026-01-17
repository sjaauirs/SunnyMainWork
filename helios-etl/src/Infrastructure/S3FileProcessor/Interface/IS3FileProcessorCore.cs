using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.S3FileProcessor.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IS3FileProcessorCore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task StartScanAndProcessFiles(EtlExecutionContext etlExecutionContext);
    }
}
