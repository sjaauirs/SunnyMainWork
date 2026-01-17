using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig.Interface;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class UploadAgreementPDFService : IUploadAgreementPDFService
    {
        private readonly ILogger<UploadAgreementPDFService> _uploadAgreementPDFServiceLogger;
        private readonly IS3Helper _s3Helper;
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;
        private readonly IAwsConfiguration _awsConfig;



        private const string className = nameof(UploadAgreementPDFService);

        public UploadAgreementPDFService(ILogger<UploadAgreementPDFService> uploadAgreementPDFServiceLogger, IVault vault
           , IConfiguration configuration, IS3Helper s3Helper, IAwsConfiguration awsConfiguration)
        {
            _uploadAgreementPDFServiceLogger = uploadAgreementPDFServiceLogger;
            _vault = vault;
            _s3Helper = s3Helper;
            _configuration = configuration;
            _awsConfig = awsConfiguration;

        }

        public async Task<Dictionary<string, string>> UploadAgreementPDf(
      UpdateOnboardingStateDto verifyMemberDto,
      string tenantCode,
      string consumerCode)
        {
            const string methodName = nameof(UploadAgreementPDf);
            var uploadedFileNames = new Dictionary<string, string>();

            try
            {
                _uploadAgreementPDFServiceLogger.LogInformation(
                    "{ClassName}.{MethodName} - Process started for uploading multiple consumer consent agreements for consumer {ConsumerCode}",
                    className, methodName, consumerCode);

                var publicS3BucketName = _awsConfig.GetAwsPublicS3BucketName();
                string keyTemplate = await _awsConfig.AgreementPublicFolderPath();

                // Loop through all key/value pairs in HtmlFileName dictionary
                foreach (var kvp in verifyMemberDto.HtmlFileName)
                {
                    var agreementKey = kvp.Key;          // e.g. "MembershipAgreement"
                    var htmlFileName = kvp.Value;        // e.g. "membership_agreement_en.html"

                    try
                    {
                        // Step 1: Build file name and path
                        string fileName = $"{consumerCode}_{Path.GetFileNameWithoutExtension(htmlFileName)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                        string filePath = await GetFileName(tenantCode, consumerCode, fileName);

                        // Step 2: Build S3 key and get HTML content
                        string awsKey = keyTemplate
                            .Replace("{tenant_code}", tenantCode)
                            .Replace("{language_code}", verifyMemberDto.LanguageCode)
                            .Replace("{html_fileName}", htmlFileName);

                        _uploadAgreementPDFServiceLogger.LogInformation(
                            "{ClassName}.{MethodName} - Downloading agreement content from {AwsKey} for consumer {ConsumerCode}",
                            className, methodName, awsKey, consumerCode);

                        string htmlContent = await _s3Helper.GetHtmlFromS3Async(awsKey, publicS3BucketName);

                        // Step 3: Convert HTML to PDF and upload
                        bool result = await ConvertHtmlAndUploadAsync(htmlContent, publicS3BucketName, filePath);

                        if (result)
                        {
                            uploadedFileNames[agreementKey] = fileName;
                            _uploadAgreementPDFServiceLogger.LogInformation(
                                "{ClassName}.{MethodName} - Uploaded PDF {FileName} successfully for {ConsumerCode} (Key: {AgreementKey})",
                                className, methodName, fileName, consumerCode, agreementKey);
                        }
                        else
                        {
                            _uploadAgreementPDFServiceLogger.LogWarning(
                                "{ClassName}.{MethodName} - Failed to upload PDF for {HtmlFileName} (Consumer: {ConsumerCode}, Key: {AgreementKey})",
                                className, methodName, htmlFileName, consumerCode, agreementKey);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        _uploadAgreementPDFServiceLogger.LogError(innerEx,
                            "{ClassName}.{MethodName} - ERROR processing {HtmlFileName} for key {AgreementKey}: {Message}",
                            className, methodName, htmlFileName, kvp.Key, innerEx.Message);
                    }
                }

                return uploadedFileNames;
            }
            catch (Exception ex)
            {
                _uploadAgreementPDFServiceLogger.LogError(ex,
                    "{ClassName}.{MethodName}: ERROR - {Message}",
                    className, methodName, ex.Message);

                return uploadedFileNames; // return whatever succeeded before the error
            }
        }



        private async Task<string> GetFileName(string tenantCode, string consumerCode, string fileName)
        {
            var agreementtemplate = await _awsConfig.UploadAgreementPublicFolderPath();


            if (string.IsNullOrWhiteSpace(agreementtemplate))
            {
                _uploadAgreementPDFServiceLogger.LogError("{ClassName}.GetFileName - Agreement path for upload file not found", className);
                return string.Empty;
            }

            var filePath = agreementtemplate
                .Replace("{tenantCode}", tenantCode)
                .Replace("{consumerCode}", consumerCode)
                .Replace("{fileName}", fileName);

            return await Task.FromResult(filePath); // or just return filePath; if async isn't needed
        }

        private async Task<bool> ConvertHtmlAndUploadAsync(string htmlContent, string s3BucketName, string fileName)
        {

            var pdfstream = await ConvertHtmlToPdfStream(htmlContent);
            return await _s3Helper.UploadFileToS3(pdfstream, s3BucketName, fileName);
        }

        public async Task<MemoryStream> ConvertHtmlToPdfStream(string htmlContent)
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = _configuration.GetSection(Constant.PathSettings).GetValue<string>(Constant.ChromeFilePath),

                Args = new[]
                    {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage", // reduce memory use
                    "--disable-gpu",
                    "--single-process",
                    "--no-zygote"
                }
            };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Load }
            });

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            return new MemoryStream(pdfBytes);
        }
    }
}
