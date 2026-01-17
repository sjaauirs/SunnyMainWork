using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    public class CsvWrapper: ICsvWrapper
    {
        private readonly ILogger<CsvWrapper> _logger;
        private const string className = nameof(CsvWrapper);

        public CsvWrapper(ILogger<CsvWrapper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create a csv file in given local path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="csvConfig"></param>
        /// <param name="records"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task CreateCsvFile<T>(CsvConfiguration csvConfig, List<T> records, string fileName) where T : class
        {
            const string methodName = nameof(CreateCsvFile);

            _logger.LogInformation($"{className}:{methodName}: Started creating {fileName}.");

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csvWriter = new CsvWriter(streamWriter, csvConfig);
            await csvWriter.WriteRecordsAsync(records);
            await streamWriter.FlushAsync();

            try
            {
                // Save the file locally
                using var localFileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                memoryStream.Position = 0; // Reset the position of the memory stream again
                await memoryStream.CopyToAsync(localFileStream);

                _logger.LogInformation($"{className}:{methodName}: Records written to {fileName} file.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{className}:{methodName}: Error writing records to {fileName} file. \n Error: {ex.Message}");
                throw;
            }
            _logger.LogInformation($"{className}:{methodName}: Completed creating {fileName}.");
        }
    }
}
