using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.IO.Compression;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class FileCryptoProcessor : AwsConfiguration, IFileCryptoProcessor
    {
        private readonly ILogger<IFileCryptoProcessor> _logger;
        private readonly IAwsS3Service _awsS3Service;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private const string className = nameof(FileCryptoProcessor);
        private const int DefaultBufferSize = 65536;

        public FileCryptoProcessor(ILogger<FileCryptoProcessor> logger, IAwsS3Service awsS3Service, IVault vault,
            IConfiguration configuration, IPgpS3FileEncryptionHelper s3FileEncryptionHelper) : base(vault, configuration)
        {
            _logger = logger;
            _awsS3Service = awsS3Service;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
        }

        public async Task Process(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(Process);
            _logger.LogInformation("{Class}.{Method}: Starting file processing for CustomerCode: {CustomerCode}", className, methodName, etlExecutionContext.CustomerCode);

            try
            {
                ValidateInput(etlExecutionContext);
                _logger.LogInformation("{Class}.{Method}: Input validation passed", className, methodName);

                var fileNames = new List<string>() {etlExecutionContext.IncomingFilePath };

                var (privateKeyBytes, publicKeyBytes, passphrase) = await LoadPgpKeys(etlExecutionContext);

                foreach (var file in fileNames)
                {
                    await ProcessFile(etlExecutionContext, file, privateKeyBytes, publicKeyBytes, passphrase);
                }

                _logger.LogInformation("{Class}.{Method}: Finished processing all files for CustomerCode: {CustomerCode}", className, methodName, etlExecutionContext.CustomerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Unhandled exception during processing", className, methodName);
                throw;
            }
        }

        private async Task<(byte[] privateKeyBytes, byte[]? publicKeyBytes, string passphrase)> LoadPgpKeys(EtlExecutionContext context)
        {
            var passphrase = await GetSecret($"{context.CustomerCode}_PGP_PASSPHRASE");
            var privateKeyBase64 = await GetSecret($"{context.CustomerCode}_PGP_PRIVATE_KEY");
            var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

            byte[]? publicKeyBytes = null;

            if (context.ActionName.Equals(Constants.Encrypt, StringComparison.OrdinalIgnoreCase))
            {
                var publicKeyBase64 = await GetSecret($"{context.CustomerCode}_PGP_PUBLIC_KEY");
                publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            }

            return (privateKeyBytes, publicKeyBytes, passphrase);
        }
        private async Task ProcessFile(EtlExecutionContext context, string fileName, byte[] privateKeyBytes, byte[]? publicKeyBytes, string passphrase)
        {
            const string methodName = nameof(ProcessFile);
            var s3Key = fileName;
            string? tempInputFilePath = null;
            _logger.LogInformation("{Class}.{Method}: Processing file '{FileName}'", className, methodName, fileName);

            try
            {

                tempInputFilePath = await DownloadFileToTemp(context.IncomingBucketName, s3Key);
                await ArchiveOriginalFile(context, tempInputFilePath, fileName);

                if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessZipFile(context, tempInputFilePath, fileName, privateKeyBytes, publicKeyBytes, passphrase);
                }
                else
                {
                    await ProcessSingleFile(context, tempInputFilePath, fileName, privateKeyBytes, publicKeyBytes, passphrase);
                }

                await _awsS3Service.DeleteFile(context.IncomingBucketName, s3Key);
                _logger.LogInformation("{Class}.{Method}: Deleted file '{FileName}' from incoming bucket", className, methodName, fileName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Processing failed for file '{FileName}'", className, methodName, fileName);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempInputFilePath))
                {
                    TryDeleteFile(tempInputFilePath);
                }
            }
        }

        private async Task<string> DownloadFileToTemp(string bucketName, string key)
        {
            await using var s3Stream = await _awsS3Service.DownloadFile(bucketName, key);
            var tempFilePath = Path.GetTempFileName();

            await using var tempFile = File.Create(tempFilePath);
            await s3Stream.CopyToAsync(tempFile);

            return tempFilePath;
        }

        private async Task ArchiveOriginalFile(EtlExecutionContext context, string filePath, string fileName)
        {
            await using var archiveStream = File.OpenRead(filePath);
            var archiveKey = $"{context.ArchiveFilePath}";

            await _awsS3Service.UploadStreamToS3(
                archiveStream,
                context.ArchiveBucketName,
                archiveKey
            );

            _logger.LogInformation("{Class}.{Method}: Archived file '{FileName}' to '{ArchiveKey}'", className, nameof(ArchiveOriginalFile), fileName, archiveKey);
        }

        private async Task ProcessZipFile(
            EtlExecutionContext context,
            string zipFilePath,
            string originalFileName,
            byte[] privateKey,
            byte[]? publicKey,
            string passphrase)
        {
            const string methodName = nameof(ProcessZipFile);

            using var zipArchive = new ZipArchive(File.OpenRead(zipFilePath), ZipArchiveMode.Read);
            _logger.LogInformation("{Class}.{Method}: Unzipping '{File}' with {EntryCount} entries", className, methodName, originalFileName, zipArchive.Entries.Count);

            foreach (var entry in zipArchive.Entries)
            {
                if (entry.FullName.EndsWith("/")) continue;

                var tempEntryPath = Path.GetTempFileName();
                var tempOutputPath = Path.GetTempFileName();

                try
                {
                    // Extract entry to a temp file
                    await using (var entryStream = entry.Open())
                    await using (var tempEntryFile = File.Create(tempEntryPath))
                    {
                        await entryStream.CopyToAsync(tempEntryFile);
                    }

                    await using var zipEntryStream = File.OpenRead(tempEntryPath);

                    // Process the file and write to temp output path
                    await using (var outputStream = File.Create(tempOutputPath))
                    {
                        if (context.ActionName.Equals(Constants.Decrypt, StringComparison.OrdinalIgnoreCase))
                        {
                            using var decryptedStream = new MemoryStream();
                            await using (var privateKeyStream = new MemoryStream(privateKey))
                            {
                                _s3FileEncryptionHelper.DecryptFile(zipEntryStream, decryptedStream, privateKeyStream, passphrase.ToCharArray());
                            }

                            decryptedStream.Seek(0, SeekOrigin.Begin);

                            if (context.ShouldAddSourceFileName)
                            {
                                string delimiter = GetDelimiterCharacter(context.Delimiter);

                                using var reader = new StreamReader(decryptedStream);
                                await using var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true);

                                var header = await reader.ReadLineAsync();
                                if (header == null)
                                    throw new InvalidOperationException("Decrypted zip entry is empty or invalid.");

                                await writer.WriteLineAsync($"{header}{delimiter}source_file_name");

                                while (!reader.EndOfStream)
                                {
                                    var line = await reader.ReadLineAsync();
                                    if (!string.IsNullOrWhiteSpace(line))
                                        await writer.WriteLineAsync($"{line}{delimiter}{entry.Name}");
                                }

                                await writer.FlushAsync();
                            }
                            else
                            {
                                decryptedStream.CopyTo(outputStream);
                            }
                        }
                        else if (context.ActionName.Equals(Constants.Encrypt, StringComparison.OrdinalIgnoreCase))
                        {
                            if (context.ShouldAddSourceFileName)
                            {
                                using var reader = new StreamReader(zipEntryStream);
                                await using var tempStream = new MemoryStream();
                                await using var writer = new StreamWriter(tempStream, Encoding.UTF8, leaveOpen: true);

                                string delimiter = GetDelimiterCharacter(context.Delimiter);
                                var header = await reader.ReadLineAsync();

                                if (header == null)
                                    throw new InvalidOperationException("Plaintext zip entry is empty or invalid.");

                                await writer.WriteLineAsync($"{header}{delimiter}source_file_name");

                                while (!reader.EndOfStream)
                                {
                                    var line = await reader.ReadLineAsync();
                                    if (!string.IsNullOrWhiteSpace(line))
                                        await writer.WriteLineAsync($"{line}{delimiter}{entry.Name}");
                                }

                                await writer.FlushAsync();
                                tempStream.Seek(0, SeekOrigin.Begin);

                                await using var publicKeyStream = new MemoryStream(publicKey!);
                                _s3FileEncryptionHelper.EncryptFile(tempStream, outputStream, publicKeyStream);
                            }
                            else
                            {
                                await using var publicKeyStream = new MemoryStream(publicKey!);
                                _s3FileEncryptionHelper.EncryptFile(zipEntryStream, outputStream, publicKeyStream);
                            }
                        }
                    }

                    // Upload the output file to S3
                    var outboundName = BuildFileName(context.OutboundFileNamePattern, context.CustomerCode, entry.Name);
                    var outputKey = $"{context.OutboundFilePath}/{outboundName}";

                    await _awsS3Service.UploadFileToS3(tempOutputPath, context.OutboundBucketName, outputKey);

                    _logger.LogInformation("{Class}.{Method}: Processed zip entry '{EntryName}' as '{OutputKey}'", className, methodName, entry.FullName, outputKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Class}.{Method}: Error processing zip entry '{Entry}' in '{File}'", className, methodName, entry.FullName, originalFileName);
                    throw;
                }
                finally
                {
                    TryDeleteFile(tempEntryPath);
                    TryDeleteFile(tempOutputPath);
                }
            }
        }

        private async Task ProcessSingleFile(
            EtlExecutionContext context,
            string inputFilePath,
            string fileName,
            byte[] privateKey,
            byte[]? publicKey,
            string passphrase)
        {
            const string methodName = nameof(ProcessSingleFile);

            string? tempProcessedPath = null;

            try
            {
                tempProcessedPath = Path.GetTempFileName();

                await using var inputStream = File.OpenRead(inputFilePath);
                await using var outputFile = File.Create(tempProcessedPath);

                if (context.ActionName.Equals(Constants.Decrypt, StringComparison.OrdinalIgnoreCase))
                {
                    using var decryptedStream = new MemoryStream();
                    await using (var privateKeyStream = new MemoryStream(privateKey))
                    {
                        _s3FileEncryptionHelper.DecryptFile(inputStream, decryptedStream, privateKeyStream, passphrase.ToCharArray());
                    }

                    if (!context.RemoveFooter && !context.ShouldAddSourceFileName)
                    {
                        // no processing needed, just upload decrypted stream
                        decryptedStream.Seek(0, SeekOrigin.Begin);

                        var OutputFileName = BuildFileName(context.OutboundFileNamePattern, context.CustomerCode, fileName);
                        var outputfilePath = $"{context.OutboundFilePath}/{OutputFileName}";

                        await _awsS3Service.UploadStreamToS3(decryptedStream, context.OutboundBucketName, outputfilePath);

                        _logger.LogInformation("{Class}.{Method}: Successfully processed and uploaded file '{FileName}' to '{OutputKey}'",
                            className, methodName, fileName, outputfilePath);
                        return;
                    }

                    decryptedStream.Seek(0, SeekOrigin.Begin);

                    using var reader = new StreamReader(
                        decryptedStream,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: DefaultBufferSize,
                        leaveOpen: true);

                    await using var writer = new StreamWriter(
                        outputFile,
                        Encoding.UTF8,
                        bufferSize: DefaultBufferSize,
                        leaveOpen: true);

                    string delimiter = GetDelimiterCharacter(context.Delimiter);

                    string? line;
                    int dataLinesWritten = 0;

                    if (context.HasHeader)
                    {
                        line = await reader.ReadLineAsync(); // read Header

                        if (string.IsNullOrWhiteSpace(line))
                            throw new InvalidOperationException("Decrypted file is empty or invalid (missing header).");

                        if (context.ShouldAddSourceFileName)
                            await writer.WriteLineAsync($"{line}{delimiter}source_file_name");
                        else
                            await writer.WriteLineAsync(line);
                    }

                    string? prevLine = null; // we keep one line in hand; final one becomes the footer if removeFooter=true

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // If we already had a previous line, it's safe to write it now
                        if (prevLine != null)
                        {
                            if (context.ShouldAddSourceFileName)
                                await writer.WriteLineAsync($"{prevLine}{delimiter}{fileName}");
                            else
                                await writer.WriteLineAsync(prevLine);

                            dataLinesWritten++;
                        }

                        // Hold onto the current line (it might be the footer)
                        prevLine = line;
                    }

                    if (prevLine != null && !context.RemoveFooter)
                    {
                        if (context.ShouldAddSourceFileName)
                            await writer.WriteLineAsync($"{prevLine}{delimiter}{fileName}");
                        else
                            await writer.WriteLineAsync(prevLine);

                        dataLinesWritten++;
                    }

                    if (!context.HasHeader && dataLinesWritten == 0)
                        throw new InvalidOperationException("Decrypted file is empty or contains only footer/blank lines.");
                    
                }
                else if (context.ActionName.Equals(Constants.Encrypt, StringComparison.OrdinalIgnoreCase))
                {
                    string tempPlainPath = Path.GetTempFileName();

                    try
                    {
                        await using var plainOutput = File.Create(tempPlainPath);

                        if (context.ShouldAddSourceFileName)
                        {
                            string delimiter = GetDelimiterCharacter(context.Delimiter);

                            using var reader = new StreamReader(inputStream);
                            await using var writer = new StreamWriter(plainOutput, Encoding.UTF8, leaveOpen: true);

                            var header = await reader.ReadLineAsync();
                            if (header == null)
                                throw new InvalidOperationException("Input file is empty or invalid.");

                            await writer.WriteLineAsync($"{header}{delimiter}source_file_name");

                            while (!reader.EndOfStream)
                            {
                                var line = await reader.ReadLineAsync();
                                if (!string.IsNullOrWhiteSpace(line))
                                    await writer.WriteLineAsync($"{line}{delimiter}{fileName}");
                            }

                            await writer.FlushAsync();
                        }
                        else
                        {
                            await inputStream.CopyToAsync(plainOutput);
                        }

                        plainOutput.Close();

                        await using var plainInput = File.OpenRead(tempPlainPath);
                        await using var publicKeyStream = new MemoryStream(publicKey!);
                        _s3FileEncryptionHelper.EncryptFile(plainInput, outputFile, publicKeyStream);
                    }
                    finally
                    {
                        TryDeleteFile(tempPlainPath);
                    }
                }

                outputFile.Close(); // Ensure output is flushed before upload

                // Upload to S3
                var outboundName = BuildFileName(context.OutboundFileNamePattern, context.CustomerCode, fileName);
                var outputKey = $"{context.OutboundFilePath}/{outboundName}";

                await using var finalStream = File.OpenRead(tempProcessedPath);
                await _awsS3Service.UploadStreamToS3(finalStream, context.OutboundBucketName, outputKey);

                _logger.LogInformation("{Class}.{Method}: Successfully processed and uploaded file '{FileName}' to '{OutputKey}'",
                    className, methodName, fileName, outputKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Error processing file '{FileName}'",
                    className, methodName, fileName);
                throw;
            }
            finally
            {
                if (tempProcessedPath != null)
                    TryDeleteFile(tempProcessedPath);
            }
        }

        private string GetDelimiterCharacter(string delimiter)
        {
            if (!Enum.TryParse<DelimiterType>(delimiter, true, out var parsed))
                throw new ArgumentException("Invalid delimiter. Allowed values: Comma, Tab, Pipe, Semicolon.");

            return parsed switch
            {
                DelimiterType.Comma => ",",
                DelimiterType.Tab => "\t",
                DelimiterType.Pipe => "|",
                DelimiterType.Semicolon => ";",
                _ => ","
            };
        }

        private void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Failed to delete temp file '{Path}'", path);
            }
        }

        private static string BuildFileName(string pattern, string customerCode, string inputFileName)
        {
            string uuid = Guid.NewGuid().ToString("N");
            string dateStr = DateTime.UtcNow.ToString("MMddyyyy");
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputFileName);

            return pattern
                .Replace("{input_filename}", fileNameWithoutExt)
                .Replace("{customer_code}", customerCode)
                .Replace("{uuid}", uuid)
                .Replace("{mmddyyyy}", dateStr);
        }


        private static void ValidateInput(EtlExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(context.CustomerCode))
                throw new ArgumentException("CustomerCode must be provided");

            if (string.IsNullOrWhiteSpace(context.ActionName) ||
                !(context.ActionName.Equals(Constants.Encrypt, StringComparison.OrdinalIgnoreCase) ||
                  context.ActionName.Equals(Constants.Decrypt, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("ActionName must be 'Encrypt' or 'Decrypt'");

            if (string.IsNullOrWhiteSpace(context.IncomingFilePath))
                throw new ArgumentException("IncomingFilePath must be provided");

            if (string.IsNullOrWhiteSpace(context.IncomingBucketName))
                throw new ArgumentException("IncomingBucketName must be provided");

            if (string.IsNullOrWhiteSpace(context.OutboundFilePath))
                throw new ArgumentException("OutboundFilePath must be provided");

            if (string.IsNullOrWhiteSpace(context.OutboundBucketName))
                throw new ArgumentException("OutboundBucketName must be provided");

            if (string.IsNullOrWhiteSpace(context.ArchiveFilePath))
                throw new ArgumentException("ArchiveFilePath must be provided");

            if (string.IsNullOrWhiteSpace(context.ArchiveBucketName))
                throw new ArgumentException("ArchiveBucketName must be provided");

            if (string.IsNullOrWhiteSpace(context.OutboundFileNamePattern))
                throw new ArgumentException("OutboundFileNamePattern must be provided");
        }

    }

}
