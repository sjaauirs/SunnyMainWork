using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using System.IO.Compression;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using Amazon;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers
{
    public class S3Helper : IS3Helper
    {
        private readonly ILogger<S3Helper> _logger;
        private readonly ISecretHelper _secretHelper;
        private readonly IAmazonS3ClientService _amazonClientService;

        const string className = nameof(S3Helper);
        public S3Helper(ILogger<S3Helper> logger, ISecretHelper secretHelper, IAmazonS3ClientService amazonClientService)
        {
            _logger = logger;
            _secretHelper = secretHelper;
            _amazonClientService = amazonClientService;
        }
        public async Task<bool> UploadFileToS3(string s3BucketName, IFormFile formFile, string key)
        {
            try
            {
                bool isuploaded = false;
                using var s3Client = _amazonClientService.GetAmazonS3Client(_secretHelper.GetAwsAccessKey().Result, _secretHelper.GetAwsSecretKey().Result, RegionEndpoint.USEast2);

                using (var stream = formFile.OpenReadStream())
                {
                    await s3Client.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = s3BucketName,
                        Key = key,
                        InputStream = stream,
                        ContentType = "application/zip",
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                    });

                }

                isuploaded = true;
                _logger.LogInformation($"{className}.UploadFileToS3: File uploaded to S3: {formFile.FileName}, Bucket Name:{s3BucketName}");

                return isuploaded;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"{className}.UploadFileToS3: Error uploading file to S3, File Name: {formFile.FileName}, Error Message: {ex.Message}");
                throw;
            }

        }
        public async Task<ImportDto?> UnzipAndProcessJsonFromS3(string s3Key, string tenantCode, string bucketName)
        {
            try
            {
                // Get the file from S3
                var getRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key
                };

                using var s3Client = _amazonClientService.GetAmazonS3Client(
                    await _secretHelper.GetAwsAccessKey(),
                    await _secretHelper.GetAwsSecretKey(),
                    RegionEndpoint.USEast2);

                using var getObjectResponse = await s3Client.GetObjectAsync(getRequest);
                using var zipStream = getObjectResponse.ResponseStream;
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                // Dictionary to store extracted JSON data
                var jsonFiles = new Dictionary<string, string>();

                // Extract and read JSON files directly from ZIP
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        using var entryStream = entry.Open();
                        using var reader = new StreamReader(entryStream);
                        string fileName = Path.GetFileName(entry.FullName).ToLower(); // Extract only the filename
                        jsonFiles[fileName] = await reader.ReadToEndAsync();

                    }
                }
                if (jsonFiles.Count > 0)
                {
                    // Deserialize JSON into ImportDto
                    var allData = new ImportDto
                    {
                        TaskData = DeserializeJson<TaskImportJson>(jsonFiles, ExportFileNames.TaskJson),
                        TenantCodeData = DeserializeJson<TenantImportJson>(jsonFiles, ExportFileNames.TenantJson),
                        TenantData = DeserializeJson<FisImportJson>(jsonFiles, ExportFileNames.FisJson),
                        CMSData = DeserializeJson<CmsImportJson>(jsonFiles, ExportFileNames.CmsJson),
                        CohortData = DeserializeJson<CohortImportJson>(jsonFiles, ExportFileNames.CohortJson),
                        SweepstakesData = DeserializeJson<SweepstakesImportJson>(jsonFiles, ExportFileNames.SweepstakesJson),
                        TaskRewardCollectionData = DeserializeJson<TaskRewardCollectionImportJson>(jsonFiles, ExportFileNames.TaskRewardCollectionJson),
                        AdventureAndTenantAdventureData = DeserializeJson<AdventureAndTenanImportJson>(jsonFiles, ExportFileNames.AdventureJson),
                        AdminData = DeserializeJson<AdminImportJson>(jsonFiles, ExportFileNames.AdminJson),
                        WalletData = DeserializeJson<WalletImportJson>(jsonFiles, ExportFileNames.WalletJson),
                        Metadata = DeserializeJson<MetadataImportJson>(jsonFiles, ExportFileNames.MetadataJson)

                    };
                    return allData;

                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to unzip and process JSON files from S3: {ex.Message}", ex);
            }
        }
        private  T? DeserializeJson<T>(Dictionary<string, string> jsonFiles, string key)
        {
            try
            {
                if (jsonFiles.TryGetValue(key.ToLower(), out var jsonContent))
                {
                    if (string.IsNullOrWhiteSpace(jsonContent))
                    {
                        return default;
                    }

                    return JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore, // Ignore extra properties
                        NullValueHandling = NullValueHandling.Ignore,         // Ignore nulls
                        Error = (sender, args) => {
                            _logger.LogError($"{className}.DeserializeJson: Deserialization , Error Message: {args.ErrorContext.Error.Message}");

                            args.ErrorContext.Handled = true;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{className}.DeserializeJson: Deserialization , Error Message: {ex.Message}");
            }

            return default;
        }
    }

}
