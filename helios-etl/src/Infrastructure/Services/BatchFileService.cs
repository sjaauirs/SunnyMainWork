using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using Newtonsoft.Json.Serialization;
using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata;
using Amazon.S3.Model.Internal.MarshallTransformations;
using SunnyRewards.Helios.Common.Core.Domain;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class BatchFileService : IBatchFileService
    {
        private readonly IBatchFileRepo _batchFileRepo;
        private readonly ILogger<BatchFileService> _logger;
        const string className = nameof(BatchFileService);
        public BatchFileService(ILogger<BatchFileService> logger, IBatchFileRepo batchFileRepo)
        {
            _logger = logger;
            _batchFileRepo = batchFileRepo;
        }

        /// <summary>
        /// Save Batch file details when processing start
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="fileType"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<ETLBatchFileModel> SaveBatchFileRecord(BatchFileDirection direction, ScanS3FileType fileType, string filename)
        {

            var batchFile = new ETLBatchFileModel()
            {
                BatchFileCode = $"bfc-{Guid.NewGuid().ToString("N")}",
                Direction = direction.ToString(),
                FileType = fileType.ToString(),
                FileName = filename,
                ProcessStartTs = DateTime.UtcNow,
                CreateTs = DateTime.UtcNow,
                CreateUser = Constants.CreateUser
            };
            return await _batchFileRepo.CreateAsync(batchFile);

        }

        /// <summary>
        /// Update process end time for batch file processing
        /// </summary>
        /// <param name="batchFile"></param>
        /// <returns></returns>
        public async Task<ETLBatchFileModel> UpdateProcessEndTime(ETLBatchFileModel batchFile)
        {
            batchFile.ProcessEndTs = DateTime.UtcNow;
           return await _batchFileRepo.UpdateAsync(batchFile);
        }
    }
}
