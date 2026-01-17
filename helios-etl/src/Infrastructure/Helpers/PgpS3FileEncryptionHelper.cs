using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.IO.Compression;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    public class PgpS3FileEncryptionHelper :AwsConfiguration, IPgpS3FileEncryptionHelper
    {
        private readonly ILogger<S3FileEncryptionHelper> _logger;
        private readonly IVault _vault;
        private readonly IS3Helper _s3Helper;
        private readonly IBatchOperationService _batchOperationService;
        const string className = nameof(PgpS3FileEncryptionHelper);
        public PgpS3FileEncryptionHelper(ILogger<S3FileEncryptionHelper> logger, IVault vault, IConfiguration configuration,
            IS3Helper s3Helper, IBatchOperationService batchOperationService) :
            base(vault, configuration)
        {
            _logger = logger;
            _vault = vault;
            _batchOperationService = batchOperationService;
            _s3Helper = s3Helper;
        }
        public async Task DecryptAndSaveToLocalPath(string bucketName, string folderName, string fileName, string privateKeyBase64, string passPhrase, string localFolderPath)
        {
            try
            {
                using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {

                    var fileFullPath = folderName != null ? $"{folderName.TrimEnd('/')}/{fileName}" : fileName;
                    string fisPrivateKey = Encoding.UTF8.GetString(Convert.FromBase64String(privateKeyBase64));


                    byte[] pgpKeyBytes = Encoding.UTF8.GetBytes(fisPrivateKey);
                    using (Stream privateKeyStream = new MemoryStream(pgpKeyBytes))
                    // Download the encrypted file from the S3 bucket
                    using (var s3Stream = await DownloadFile(s3Client, bucketName, fileFullPath))
                    using (MemoryStream inputStream = new MemoryStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        s3Stream.CopyTo(inputStream);
                        inputStream.Seek(0, SeekOrigin.Begin);
                        // Decrypt the file content
                        DecryptFile(inputStream, memoryStream, privateKeyStream, passPhrase.ToCharArray());

                        var combinedBytes = memoryStream.ToArray();

                        string decryptedFileName = $"Decrypted_{fileName}";
                        string localFilePath = Path.Combine(localFolderPath, decryptedFileName);
                        File.WriteAllBytes(localFilePath, combinedBytes);

                    }
                }

                _logger.LogInformation("{className}.DecryptAndSaveToLocalPath: Decryption and saving to local path completed successfully.", className);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.DecryptAndSaveToLocalPath: ERROR - decrypting and saving to local path: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }
        public async Task<byte[]> DownloadAndDecryptFile(SecureFileTransferRequestDto requestDto)
        {
            const string methodName = nameof(DownloadAndDecryptFile);
            try
            {
                byte[] data = null;
                string? decryptedFilename = null;
                using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var fileFullPath = requestDto.SourceFolderName != null
                        ? $"{requestDto.SourceFolderName.TrimEnd('/')}/{requestDto.SourceFileName}"
                        : requestDto.SourceFileName;
                    string sunnyPrivateKeyBase64 = string.Empty;
                    if (string.IsNullOrEmpty(requestDto.TenantCode))
                    {
                        sunnyPrivateKeyBase64 = GetSecret(requestDto.SunnyAplPrivateKeyName).Result;
                    }
                    else
                    {
                        sunnyPrivateKeyBase64 = GetTenantSecret(requestDto.TenantCode, requestDto.SunnyAplPrivateKeyName).Result;
                    }

                    if (string.IsNullOrEmpty(sunnyPrivateKeyBase64) || sunnyPrivateKeyBase64 == _vault.InvalidSecret)
                    {
                        _logger.LogError($"{className}.{methodName}: Sunny Private key is not configured for Tenant: {requestDto.TenantCode}");
                        throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, $"Sunny Private key is not configured in AWS Secret Manager for Tenant: {requestDto.TenantCode}");
                    }
                    string passPhraseKey = string.Empty;
                    if (string.IsNullOrEmpty(requestDto.TenantCode))
                    {
                        passPhraseKey = GetSecret(requestDto.PassPhraseKeyName).Result;
                    }
                    else
                    {
                        passPhraseKey = GetTenantSecret(requestDto.TenantCode, requestDto.PassPhraseKeyName).Result;
                    }
                    
                    if (string.IsNullOrEmpty(passPhraseKey) || passPhraseKey == _vault.InvalidSecret)
                    {
                        _logger.LogError($"{className}.{methodName}: PassPhrase key is not configured for Tenant: {requestDto.TenantCode}");
                        throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, $"PassPhrase key is not configured in AWS Secret Manager for Tenant: {requestDto.TenantCode}");
                    }
                    string sunnyPrivateKey = Encoding.UTF8.GetString(Convert.FromBase64String(sunnyPrivateKeyBase64));
                    byte[] pgpKeyBytes = Encoding.UTF8.GetBytes(sunnyPrivateKey);
                    using (Stream privateKeyStream = new MemoryStream(pgpKeyBytes))
                    // Download the decrypt file from the S3 bucket
                    using (var s3Stream = await DownloadFile(s3Client, requestDto.SourceBucketName, fileFullPath))
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        s3Stream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        if (fileFullPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                            {
                                var entry = archive.Entries.FirstOrDefault();
                                if (entry != null)
                                {
                                    decryptedFilename = entry.FullName;
                                    if (string.Equals(Path.GetExtension(entry.FullName), FISBatchConstants.ENCRYPTED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                                    {
                                        decryptedFilename = Path.GetFileNameWithoutExtension(entry.FullName);
                                    }
                                    using (var entryStream = entry.Open())
                                    using (MemoryStream entryFileMemoryStream = new MemoryStream())
                                    using (var decryptedStream = new MemoryStream())
                                    {
                                        entryStream.CopyTo(entryFileMemoryStream);
                                        entryFileMemoryStream.Seek(0, SeekOrigin.Begin);
                                        DecryptFile(entryFileMemoryStream, decryptedStream, privateKeyStream, passPhraseKey.ToCharArray());
                                        data = decryptedStream.ToArray();

                                    }
                                }
                            }

                        }
                        else if (!string.Equals(Path.GetExtension(requestDto.SourceFileName), FISBatchConstants.ENCRYPTED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                        {
                            data = memoryStream.ToArray();
                            decryptedFilename = requestDto.SourceFileName;
                        }
                        else
                        {
                            using (var decryptedStream = new MemoryStream())
                            {
                                DecryptFile(memoryStream, decryptedStream, privateKeyStream, passPhraseKey.ToCharArray());
                                data = decryptedStream.ToArray();
                                decryptedFilename = requestDto.SourceFileName;
                                if (string.Equals(Path.GetExtension(requestDto.SourceFileName), FISBatchConstants.ENCRYPTED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                                {
                                    decryptedFilename = Path.GetFileNameWithoutExtension(requestDto.SourceFileName);
                                }
                            }
                        }

                        //Save decrypted data to S3 tmp
                        if (decryptedFilename != null && data != null)
                        {
                            await _s3Helper.UploadByteDataToS3(data, requestDto.ArchiveBucketName, $"{requestDto.InboundArchiveFolderName}/{FISBatchConstants.DECRYPTED_FOLDER_NAME}", decryptedFilename);
                        }


                        if (requestDto.DeleteFileAfterCopy)
                        {
                            var fullFileName = requestDto.SourceFolderName != null ? $"{requestDto.SourceFolderName.TrimEnd('/')}/{requestDto.SourceFileName}" : requestDto.SourceFileName;
                            await DeleteFileFromBucket(s3Client, requestDto.SourceBucketName, fullFileName);
                        }
                    }
                }

                _logger.LogInformation($"{className}.{methodName}: Download and Decryption completed successfully.");
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.{methodName}: ERROR in downloading and decrypting: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }
        public async Task<LatestFileResult> DownloadLatestFileByName(S3FileDownloadRequestDto requestDto)
        {
            const string methodName = nameof(DownloadLatestFileByName);
            try
            {
                var result = new LatestFileResult();
                using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var fileFullPath = requestDto.SourceFolderName != null
                        ? $"{requestDto.SourceFolderName.TrimEnd('/')}/{requestDto.SourceFileName}"
                        : requestDto.SourceFileName;

                    // Download the file from the S3 bucket
                    using (var downloadResult = await DownloadLatestFile(s3Client, requestDto.SourceBucketName, fileFullPath))
                    {
                        if (downloadResult?.FileStream == null)
                        {
                            _logger.LogError("{ClassName}.{MethodName} - No file stream returned from DownloadLatestFile. Bucket: {BucketName}, FilePath: {FilePath}", className, methodName, requestDto.SourceBucketName, fileFullPath);
                            return result;
                        }
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            downloadResult.FileStream.CopyTo(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            if (fileFullPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                                {
                                    var entry = archive.Entries.FirstOrDefault();
                                    if (entry != null)
                                    {
                                        using (var entryStream = entry.Open())
                                        using (MemoryStream entryFileMemoryStream = new MemoryStream())
                                        {
                                            entryStream.CopyTo(entryFileMemoryStream);
                                            entryFileMemoryStream.Seek(0, SeekOrigin.Begin);
                                            result.FileContent = entryFileMemoryStream.ToArray();
                                            result.FileName = downloadResult.FileName;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                result.FileContent = memoryStream.ToArray();
                                result.FileName = downloadResult.FileName;
                            }

                            if (requestDto.DeleteFileAfterCopy)
                            {
                                var fullFileName = requestDto.SourceFolderName != null ? $"{requestDto.SourceFolderName.TrimEnd('/')}/{requestDto.SourceFileName}" : requestDto.SourceFileName;
                                await DeleteFileFromBucket(s3Client, requestDto.SourceBucketName, fullFileName);
                            }
                        }
                    }

                }

                _logger.LogInformation($"{className}.{methodName}: Download completed successfully.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.{methodName}: ERROR  in downloading : {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }

        /// <summary>
        /// Encrypts the generated card creation file and uploads it to a secure location.
        /// </summary>
        /// <param name="tenantCode ,targetFolder, batchOperationGroupCode ">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task EncryptGeneratedFile(string batchOperationGroupCode, string tenantCode, string targetBucket = "", string targetFolder = "", string targetFileName = "")
        {
            const string methodName = nameof(EncryptGeneratedFile);
            try
            {
                var generatedRecords = await _batchOperationService.GetBatchOperationsRecords(batchOperationGroupCode, new List<BatchActions> { BatchActions.GENERATE });

                if (generatedRecords == null || generatedRecords.Count == 0)
                {
                    _logger.LogWarning("{className}.{methodName}: No generated file to encrypt. Error Code:{error}", className, methodName, StatusCodes.Status400BadRequest);
                    return;
                }

                foreach (var record in generatedRecords)
                {
                    var actionJson = JsonConvert.DeserializeObject<GenerateActionDto>(record.action_description_json);
                    if (actionJson == null)
                    {
                        _logger.LogWarning($"{className}.{methodName}: Generated Record entry with out File Details, reocrdID:-> {record.BatchOperationCode}. Error Code:{StatusCodes.Status400BadRequest}");
                        return;
                    }
                    var sourceS3BucketName = actionJson.Location.StorageName;
                    var sourcefileName = actionJson.Location.FileName;
                    var sourceFolder = String.IsNullOrWhiteSpace(actionJson.Location.FolderName) ? tenantCode : actionJson.Location.FolderName;

                    if (!string.IsNullOrWhiteSpace(sourceS3BucketName) && !string.IsNullOrWhiteSpace(sourcefileName))
                    {
                        var secureFileTransferRequestDto = new SecureFileTransferRequestDto
                        {
                            TenantCode = tenantCode,
                            SourceBucketName = sourceS3BucketName,
                            SourceFileName = sourcefileName,
                            TargetBucketName = String.IsNullOrWhiteSpace(targetBucket) ? sourceS3BucketName : targetBucket,
                            TargetFolderName = String.IsNullOrWhiteSpace(targetFolder) ? sourceFolder : targetFolder,
                            TargetFileName = String.IsNullOrWhiteSpace(targetFileName) ? sourcefileName : targetFileName,
                            FisAplPublicKeyName = GetFisAplPublicKeyName(),
                            SunnyAplPrivateKeyName = GetSunnyAplPrivateKeyName(),
                            SunnyAplPublicKeyName = GetSunnyAplPublicKeyName(),
                            BatchOperationGroupCode = batchOperationGroupCode
                        };

                        await EncryptFile(secureFileTransferRequestDto);
                    }
                    else
                    {
                        _logger.LogError($"SourceBucket/SourceFile not defined for record {record.BatchOperationCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while encrypting the file for TenantCode: {TenantCode}", className, methodName, tenantCode);
                throw;
            }
        }

        /// <summary>
        /// Copies the generated card file ( Encrypted/Unencrypted/both) to the specified destination.
        /// </summary>
        /// <param name="batchOperationGroupCode">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CopyFileToS3Destination(string batchOperationGroupCode, string destinationBucket, string destinationFolder)
        {
             await CopyOrArchiveFile(batchOperationGroupCode, destinationBucket, destinationFolder, BatchActions.COPY);
        }

        /// <summary>
        /// Archives the decrypted and encrypted card file to a respective archive folder.- Delete Files from tmp bucket
        /// </summary>
        /// <param name="batchOperationGroupCode">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ArchiveFile(string batchOperationGroupCode, string archiveBucketName, string archiveDestination)
        {
            await CopyOrArchiveFile(batchOperationGroupCode,archiveBucketName, archiveDestination, BatchActions.ARCHIVE);
        }

        /// <summary>
        /// Deletes the generated or encrypted card file from the S3 bucket.
        /// </summary>
        /// <param name="batchOperationGroupCode ">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteFile(string batchOperationGroupCode)
        {
            var actions = HandleFileTypes(BatchActions.DELETE);
   
            var batchOperationRecords = await _batchOperationService.GetBatchOperationsRecords(batchOperationGroupCode, actions);
            var uniqueBatchOperationsRecord = UniqueRecords(batchOperationRecords);

            foreach (var record in uniqueBatchOperationsRecord)
            {
                var actionDetails = GetFileLocationDetails(record);
                if (!string.IsNullOrWhiteSpace(actionDetails.S3BucketName) && !string.IsNullOrWhiteSpace(actionDetails.ConcatFileName))
                {
                    var secureFileTransferRequestDto = new DeleteFileS3RequestDto
                    {
                        SourceBucketName = actionDetails.S3BucketName,
                        SourceFileName = actionDetails.FileName,
                        SourceFolderName = actionDetails.FolderName,
                        BatchOperationGroupCode = batchOperationGroupCode,
                    };

                    await DeleteS3File(secureFileTransferRequestDto);
                }
                else
                {
                    _logger.LogError($"SourceBucket/SourceFile not defined for record {record.BatchOperationCode}");
                }
            }

        }


        #region private members
        private async Task UploadDecryptedFile(SecureFileTransferRequestDto requestDto, AmazonS3Client s3Client)
        {
            using (var downloadedFileStream = await DownloadFile(s3Client, requestDto.SourceBucketName, requestDto.SourceFileName))
            using (MemoryStream decryptedStream = new MemoryStream())
            {
                downloadedFileStream.CopyTo(decryptedStream);
                decryptedStream.Seek(0, SeekOrigin.Begin);
                var archiveFolder = $"{requestDto.OutboundArchiveFolderName}/{FISBatchConstants.DECRYPTED_FOLDER_NAME}";
                await UploadFile(s3Client, requestDto.ArchiveBucketName, archiveFolder, requestDto.TargetFileName, decryptedStream);
            }
        }

        private async Task DeleteFileFromBucket(AmazonS3Client s3Client, string bucketName, string fileName)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                };
                await s3Client.DeleteObjectAsync(deleteObjectRequest);
                _logger.LogInformation($"{className}.DeleteAllFilesFromTempBucket: Deleted file: {fileName} from bucket: {bucketName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.DeleteFileFromBucket: ERROR deleting file from S3 bucket: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }

        private async Task UploadFile(AmazonS3Client s3Client, string bucketName, string? folderName, string fileName, Stream inputStream,
            bool autoCloseStream = true)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = folderName != null ? $"{folderName.TrimEnd('/')}/{fileName}" : fileName,
                    InputStream = inputStream,
                    AutoCloseStream = autoCloseStream
                };

                await s3Client.PutObjectAsync(putRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.UploadFile: ERROR uploading file to S3 bucket: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }

        }

        private async Task<Stream> DownloadFile(AmazonS3Client s3Client, string bucketName, string fileKey)
        {
            try
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileKey
                };

                var response = await s3Client.GetObjectAsync(getRequest);
                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.DownloadFile: ERROR downloading file from S3 bucket: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }

        }

        private async Task<FileDownloadResult> DownloadLatestFile(AmazonS3Client s3Client, string bucketName, string filePrefix)
        {
            try
            {
                var listObjectsRequest = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    MaxKeys = 100,
                    Prefix = filePrefix
                };

                ListObjectsV2Response response;
                List<S3Object> allFiles = new List<S3Object>();

                do
                {
                    response = await s3Client.ListObjectsV2Async(listObjectsRequest);
                    allFiles.AddRange(response.S3Objects);
                    listObjectsRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                var latestFile = GetLatestFile(allFiles);
                if (latestFile != null)
                {
                    Console.WriteLine($"Latest file: {latestFile.Key}");
                    var stream = await DownloadFile(s3Client, bucketName, latestFile.Key);
                    return new FileDownloadResult
                    {
                        FileStream = stream,
                        FileName = latestFile.Key
                    };
                }
                else
                {
                    Console.WriteLine("No files found.");
                    _logger.LogError($"{className}.DownloadLatestFile: No files found in bucket '{bucketName}' with prefix '{filePrefix}'.");
                    return new FileDownloadResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.DownloadLatestFile: ERROR downloading file from S3 bucket: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }

        private S3Object GetLatestFile(List<S3Object> files)
        {
            return files
         .OrderByDescending(file =>
             DateTime.TryParseExact(
                 Path.GetFileNameWithoutExtension(file.Key).Split('_').Last(),
                 "yyyyMMdd-HHmmss",
                 null,
                 System.Globalization.DateTimeStyles.None,
                 out var parsedDate) ? parsedDate : DateTime.MinValue)
         .FirstOrDefault();
        }
        public void EncryptFile(Stream inputFileStream, Stream outputStream, Stream publicKeyStream)
        {
            PgpPublicKey publicKey = ReadPublicKey(publicKeyStream);
            using (Stream encryptedOut = ChainEncryptedOut(outputStream, publicKey))
            using (Stream compressedOut = ChainCompressedOut(encryptedOut))
            using (Stream literalOut = ChainLiteralOut(compressedOut, "filename"))
            {
                byte[] buf = new byte[Constants.RsaKeySize];
                int len;
                while ((len = inputFileStream.Read(buf, 0, buf.Length)) > 0)
                {
                    literalOut.Write(buf, 0, len);
                }
            }
        }

        public void DecryptFile(Stream inputStream, Stream outputStream, Stream privateKeyStream, char[] passPhrase)
        {
            using (Stream keyIn = PgpUtilities.GetDecoderStream(privateKeyStream))
            {
                PgpObjectFactory pgpF = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
                PgpEncryptedDataList enc;
                PgpObject o = pgpF.NextPgpObject();
                if (o is PgpEncryptedDataList)
                {
                    enc = (PgpEncryptedDataList)o;
                }
                else
                {
                    enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
                }

                PgpPrivateKey sKey = null;
                PgpPublicKeyEncryptedData pbe = null;
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(keyIn);

                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    sKey = FindSecretKey(pgpSec, pked.KeyId, passPhrase);
                    if (sKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }

                if (sKey == null)
                {
                    throw new ArgumentException("Secret key for message not found.");
                }

                using (Stream clear = pbe.GetDataStream(sKey))
                {
                    PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                    PgpObject message = plainFact.NextPgpObject();
                    PgpObjectFactory of = null;
                    if (message is PgpCompressedData)
                    {
                        PgpCompressedData cData = (PgpCompressedData)message;
                        of = new PgpObjectFactory(cData.GetDataStream());
                        message = of.NextPgpObject();
                    }

                    if (message is PgpOnePassSignatureList)
                    {
                        message = of.NextPgpObject();
                        PgpLiteralData ld = null;
                        ld = (PgpLiteralData)message;
                        Stream unc = ld.GetInputStream();
                        byte[] buf = new byte[Constants.RsaKeySize];
                        int len;
                        while ((len = unc.Read(buf, 0, buf.Length)) > 0)
                        {
                            outputStream.Write(buf, 0, len);
                        }
                    }
                    else
                    {
                        PgpLiteralData ld = null;
                        ld = (PgpLiteralData)message;
                        Stream unc = ld.GetInputStream();
                        byte[] buf = new byte[Constants.RsaKeySize];
                        int len;
                        while ((len = unc.Read(buf, 0, buf.Length)) > 0)
                        {
                            outputStream.Write(buf, 0, len);
                        }
                    }
                }
            }
        }

        private PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);
            PgpPublicKey key = null;

            foreach (PgpPublicKeyRing kRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey k in kRing.GetPublicKeys())
                {
                    if (k.IsEncryptionKey)
                    {
                        key = k;
                        break;
                    }
                }
                if (key != null)
                {
                    break;
                }
            }

            if (key == null)
            {
                throw new ArgumentException("No encryption key found in the public key ring.");
            }

            return key;
        }

        private Stream ChainEncryptedOut(Stream outputStream, PgpPublicKey key)
        {
            PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, true, new SecureRandom());
            encGen.AddMethod(key);
            return encGen.Open(outputStream, new byte[Constants.RsaKeySize]);
        }

        private Stream ChainCompressedOut(Stream encryptedOut)
        {
            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
            return comData.Open(encryptedOut);
        }

        private Stream ChainLiteralOut(Stream compressedOut, string fileName)
        {
            PgpLiteralDataGenerator litData = new PgpLiteralDataGenerator();
            return litData.Open(compressedOut, PgpLiteralData.Binary, fileName, DateTime.UtcNow, new byte[Constants.RsaKeySize]);
        }

        private PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);

            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        private string GetFisPublicKyeBase64(SecureFileTransferRequestDto requestDto)
        {
            string fisPublicKeyBase64 = null;
            if (requestDto.FisAplPublicKeyName == GetFisEtgAplPublicKeyName())
            {
                fisPublicKeyBase64 = GetSecret(requestDto.FisAplPublicKeyName).Result;
            }
            else
            {
                fisPublicKeyBase64 = GetTenantSecret(requestDto.TenantCode, requestDto.FisAplPublicKeyName).Result;
            }

            return fisPublicKeyBase64;
        }
        private async Task EncryptFile(SecureFileTransferRequestDto requestDto)
        {
            const string methodName = nameof(EncryptFile);
            try
            {
                using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    string fisPublicKeyBase64 = GetFisPublicKyeBase64(requestDto);
                    if (string.IsNullOrEmpty(fisPublicKeyBase64) || fisPublicKeyBase64 == _vault.InvalidSecret)
                    {
                        _logger.LogError($"{className}.{methodName}: FIS Public key is not configured for Tenant: {requestDto.TenantCode}");
                        throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, $"FIS Public key is not configured for Tenant: {requestDto.TenantCode}");
                    }
                    string fisPublicKey = Encoding.UTF8.GetString(Convert.FromBase64String(fisPublicKeyBase64));
                    byte[] pgpKeyBytes = Encoding.UTF8.GetBytes(fisPublicKey);
                    using (Stream publicKeyStream = new MemoryStream(pgpKeyBytes))
                    // Download the file from the input S3 bucket
                    using (var s3Stream = await DownloadFile(s3Client, requestDto.SourceBucketName, requestDto.SourceFileName))
                    using (MemoryStream inputStream = new MemoryStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        s3Stream.CopyTo(inputStream);
                        inputStream.Seek(0, SeekOrigin.Begin);

                        // Encrypt the combined data using the FIS public key
                        EncryptFile(inputStream, memoryStream, publicKeyStream);

                        // Upload the encrypted file to the output S3 bucket
                        memoryStream.Position = 0;
                        var encryptedFileName = string.Equals(Path.GetExtension(requestDto.TargetFileName),
                            FISBatchConstants.ENCRYPTED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)
                            ? requestDto.TargetFileName
                            : $"{requestDto.TargetFileName}{FISBatchConstants.ENCRYPTED_FILE_EXTENSION}";

                        await UploadFile(s3Client, requestDto.TargetBucketName, requestDto.TargetFolderName, encryptedFileName,
                            memoryStream, false);
                        requestDto.TargetFileName = encryptedFileName;
                    }

                    await SaveBatchOperation(BatchActions.ENCRYPT.ToString(), requestDto);

                }

                _logger.LogInformation("{className}.{methodName}: Encryption and upload completed successfully.", className, methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.{methodName}: ERROR encrypting and uploading files: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }

        }
        private async Task CopyFileToDestination(CopyFileS3RequestDto requestDto)
        {
            const string methodName = nameof(CopyFileToDestination);

            // Construct source and destination keys
            string sourceKey = $"{requestDto.SourceFolderName}/{requestDto.SourceFileName}";
            string destinationKey = $"{requestDto.TargetFolderName}/{requestDto.TargetFileName}";

            try
            {
                var copyResponse = await _s3Helper.CopyFileToFolder(requestDto.SourceBucketName, sourceKey, requestDto.TargetBucketName, destinationKey);

                // Check if the copy was successful
                if (copyResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"{methodName}: Copy file successfully from {sourceKey} to {destinationKey}");
                }
                await SaveBatchOperation(requestDto.BatchActionName, requestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.{methodName}: ERROR -Copying file: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }
        private async Task DeleteS3File(DeleteFileS3RequestDto requestDto)
        {
            const string methodName = nameof(CopyFileToDestination);
            // Construct source and destination keys
            string sourceKey = string.IsNullOrEmpty(requestDto.SourceFolderName) ? requestDto.SourceFileName : $"{requestDto.SourceFolderName}/{requestDto.SourceFileName}";
            try
            {
                await _s3Helper.DeleteFileFromBucket(requestDto.SourceBucketName, sourceKey);
                await SaveBatchOperation(BatchActions.DELETE.ToString(), requestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.{methodName}: ERROR -Copying file: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }

        }

        private async Task SaveBatchOperation(string batchAction, dynamic requestDto)
        {
            BatchActionDtoBase? action = CreateBatchActionDto(batchAction, requestDto);

            if (action != null)
            {
                await _batchOperationService.SaveBatchOperation(requestDto.BatchOperationGroupCode, action);
            }
        }
        private BatchActionDtoBase? CreateBatchActionDto(string batchAction, dynamic requestDto)
        {
            switch (batchAction.ToUpper().Trim())
            {
                case nameof(BatchActions.DELETE):
                    return new DeleteActionDto
                    {
                        BatchAction = batchAction,
                        Location = new ETLlocationDto(requestDto.SourceBucketName, requestDto.SourceFolderName ?? "", requestDto.SourceFileName)
                    };

                case nameof(BatchActions.ENCRYPT):
                    return new EncryptActionDto
                    {
                        BatchAction = batchAction,
                        SrcLocation = new ETLlocationDto(requestDto.SourceBucketName, requestDto.SourceFolderName ?? "", requestDto.SourceFileName),
                        DstLocation = new ETLlocationDto(requestDto.TargetBucketName, requestDto.TargetFolderName ?? "", requestDto.TargetFileName),
                    };

                case nameof(BatchActions.COPY):
                case nameof(BatchActions.ARCHIVE):
                    return new CopyActionDto
                    {
                        BatchAction = batchAction,
                        SrcLocation = new ETLlocationDto(requestDto.SourceBucketName, requestDto.SourceFolderName ?? "", requestDto.SourceFileName),
                        DstLocation = new ETLlocationDto(requestDto.TargetBucketName, requestDto.TargetFolderName ?? "", requestDto.TargetFileName),
                    };

                default:
                    return null;
            }
        }
        private async Task CopyOrArchiveFile(string batchOperationGroupCode, string targetBucketName, string targetFolderName, BatchActions action)
        {
            var actions = HandleFileTypes(action);
            var methodName = nameof(CopyOrArchiveFile);

            var batchOpsRecords = await _batchOperationService.GetBatchOperationsRecords(batchOperationGroupCode, actions);

            if (batchOpsRecords == null || batchOpsRecords.Count == 0)
            {
                _logger.LogWarning("{className}.{methodName}: No generated file to Copy/Archive. Error Code:{error}", className, methodName, StatusCodes.Status400BadRequest);
                return;
            }

            var uniqueBatchOperationsRecord = UniqueRecords(batchOpsRecords);

            foreach (var record in uniqueBatchOperationsRecord)
            {
                var actionDetails = GetFileLocationDetails(record);
                if (!String.IsNullOrWhiteSpace(actionDetails.S3BucketName) && !String.IsNullOrWhiteSpace(actionDetails.ConcatFileName))
                {
                    var secureFileTransferRequestDto = new CopyFileS3RequestDto
                    {
                        SourceBucketName = actionDetails.S3BucketName,
                        SourceFileName = actionDetails.FileName,
                        SourceFolderName = actionDetails.FolderName,
                        TargetBucketName = targetBucketName,
                        TargetFolderName = targetFolderName,
                        TargetFileName = actionDetails.FileName,
                        BatchActionName = action.ToString(),
                        BatchOperationGroupCode = batchOperationGroupCode
                    };
                    //Archiving a non encrypted file
                    if(!actionDetails.FileName.EndsWith(FISBatchConstants.ENCRYPTED_FILE_EXTENSION))
                    {
                        secureFileTransferRequestDto.TargetFolderName = $"{targetFolderName}/{FISBatchConstants.DECRYPTED_FOLDER_NAME}"; 
                    }
                    await CopyFileToDestination(secureFileTransferRequestDto);
                }
                else
                {
                    _logger.LogError($"SourceBucket/SourceFile not defined for record {record.BatchOperationCode}");
                }
            }
        }

        private List<EtlBatchOperationModel> UniqueRecords(IList<EtlBatchOperationModel> batchOperationRecords)
        {
            var generatedRecords = batchOperationRecords.Where(x => x.BatchAction == BatchActions.GENERATE.ToString()).ToList();

            var encryptAndCopyRecords = batchOperationRecords.Where(x =>
                x.BatchAction == BatchActions.ENCRYPT.ToString() || x.BatchAction == BatchActions.COPY.ToString()).ToList();

            var uniqueOtherRecords = encryptAndCopyRecords
                .GroupBy(record =>
                {
                    var actionData = JsonConvert.DeserializeObject<EncryptActionDto>(record.action_description_json);
                    return new
                    {
                        actionData.DstLocation.StorageName,
                        actionData.DstLocation.FolderName,
                        actionData.DstLocation.FileName
                    };
                })
                .Select(g => g.OrderByDescending(r => r.CreateTs).First()) // Keep the most recent record based on timestamp
                .ToList();
            generatedRecords.AddRange(uniqueOtherRecords);

            return generatedRecords;
        }

        private List<BatchActions> HandleFileTypes(BatchActions action)
        {
            var recordsActions = new List<BatchActions>();

            if (action == BatchActions.DELETE|| action == BatchActions.ARCHIVE)
            {
                recordsActions.Add(BatchActions.ENCRYPT);
                recordsActions.Add(BatchActions.GENERATE);
            }
            else if (action == BatchActions.COPY)
            {
                recordsActions.Add(BatchActions.ENCRYPT);
            }

            return recordsActions;
        }
        private FileLocationDetailsDto GetFileLocationDetails(EtlBatchOperationModel record)
        {
            string s3BucketName = string.Empty;
            string fileName = string.Empty;
            string folderName = string.Empty;

            // Deserialize based on BatchAction
            if (record.BatchAction == BatchActions.GENERATE.ToString())
            {
                var actionJson = JsonConvert.DeserializeObject<GenerateActionDto>(record.action_description_json);
                fileName = actionJson.Location.FileName;
                folderName = actionJson.Location.FolderName;
                s3BucketName = actionJson.Location.StorageName;
            }
            else if (record.BatchAction == BatchActions.ENCRYPT.ToString() || record.BatchAction == BatchActions.COPY.ToString())
            {
                var actionJson = JsonConvert.DeserializeObject<EncryptActionDto>(record.action_description_json);
                fileName = actionJson.DstLocation.FileName;
                folderName = actionJson.DstLocation.FolderName;
                s3BucketName = actionJson.DstLocation.StorageName;
            }

            // Return details as an object
            return new FileLocationDetailsDto(s3BucketName, fileName, folderName);
        }
        #endregion
    }
}
