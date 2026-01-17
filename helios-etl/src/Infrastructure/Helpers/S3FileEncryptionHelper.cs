using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using System.Security.Cryptography;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    public class S3FileEncryptionHelper : AwsConfiguration, IS3FileEncryptionHelper
    {
        private readonly ILogger<S3FileEncryptionHelper> _logger;
        private readonly IVault _vault;
        const string className = nameof(S3FileEncryptionHelper);
        public S3FileEncryptionHelper(ILogger<S3FileEncryptionHelper> logger, IVault vault, IConfiguration configuration) :
            base(vault, configuration)
        {
            _logger = logger;
            _vault = vault;
        }

        public async Task DecryptAndSaveToLocalPath(string bucketName, string folderName, string fileName, byte[] fisPrivateKey, byte[] sunnyPublicKey, string localFolderPath)
        {
            try
            {
                using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {

                    var fileFullPath = folderName != null ? $"{folderName.TrimEnd('/')}/{fileName}" : fileName;
                    // Download the encrypted file from the S3 bucket
                    using (var s3Stream = await DownloadFile(s3Client, bucketName, fileFullPath))
                    using (MemoryStream inputStream = new MemoryStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        s3Stream.CopyTo(inputStream);
                        inputStream.Seek(0, SeekOrigin.Begin);
                        // Decrypt the file content
                        DecryptStream(inputStream, memoryStream, fisPrivateKey);

                        var combinedBytes = memoryStream.ToArray();
                        // (byte[] signature, byte[] data) = ExtractSignatureAndData(combinedBytes, Constants.RsaSignatureLength);

                        //  bool isSignatureValid = VerifySignature(data, signature, sunnyPublicKey);
                        //if (isSignatureValid)
                        //{
                        //    _logger.LogInformation("DecryptAndSaveToLocalPath: Signature is valid.");

                        //    // Construct the local file path
                        //    string decryptedFileName = $"Decrypted_{fileName}";
                        //    string localFilePath = Path.Combine(localFolderPath, decryptedFileName);
                        //    File.WriteAllBytes(localFilePath, data);
                        //}
                        //else
                        //{
                        //    _logger.LogError("DecryptAndSaveToLocalPath: Signature is not valid.");
                        //}
                        // Construct the local file path
                        string decryptedFileName = $"Decrypted_{fileName}";
                        string localFilePath = Path.Combine(localFolderPath, decryptedFileName);
                        File.WriteAllBytes(localFilePath, combinedBytes);

                    }
                }

                _logger.LogInformation("{className}.DecryptAndSaveToLocalPath: Decryption and saving to local path completed successfully.", className);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.DecryptAndSaveToLocalPath: Error decrypting and saving to local path: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }


        public async Task<byte[]> DownloadAndDecryptFile(SecureFileTransferRequestDto requestDto)
        {
            const string methodName = nameof(DownloadAndDecryptFile);
            try
            {
                byte[] data;
                using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var fileFullPath = requestDto.SourceFolderName != null
                        ? $"{requestDto.SourceFolderName.TrimEnd('/')}/{requestDto.SourceFileName}"
                        : requestDto.SourceFileName;
                    // Download the decrypt file from the S3 bucket
                    using (var s3Stream = await DownloadFile(s3Client, requestDto.SourceBucketName, fileFullPath))
                    using (MemoryStream inputStream = new MemoryStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        var sunnyPrivateKey = GetTenantSecret(requestDto.TenantCode, requestDto.SunnyAplPrivateKeyName).Result;
                        if (string.IsNullOrEmpty(sunnyPrivateKey) || sunnyPrivateKey == _vault.InvalidSecret)
                        {
                            _logger.LogError($"{className}.{methodName}: Sunny Private key is not configured for Tenant: {requestDto.TenantCode}");
                            return null;
                        }
                        s3Stream.CopyTo(inputStream);
                        inputStream.Seek(0, SeekOrigin.Begin);
                        // Decrypt the file content
                        DecryptStream(inputStream, memoryStream, Convert.FromBase64String(sunnyPrivateKey));
                        data = memoryStream.ToArray();
                        //var combinedBytes = memoryStream.ToArray();
                        //(byte[] signature, data) = ExtractSignatureAndData(combinedBytes, Constants.RsaSignatureLength);

                        //var fisPublicKey = GetTenantSecret(requestDto.TenantCode, requestDto.FisAplPublicKeyName).Result;
                        //if (string.IsNullOrEmpty(fisPublicKey) || fisPublicKey == _vault.InvalidSecret)
                        //{
                        //    _logger.LogError($"{methodName}: FIS Public key is not configured for Tenant: {requestDto.TenantCode}");
                        //    return data;
                        //}
                        //bool isSignatureValid = VerifySignature(data, signature, Convert.FromBase64String(fisPublicKey));
                        //if (isSignatureValid)
                        //{
                        //    _logger.LogInformation($"{methodName}: Signature is valid.");

                        //    string decryptedFileName = $"Decrypted_{requestDto.SourceFileName}";
                        //}
                        //else
                        //{
                        //    _logger.LogError($"{methodName}: Signature is not valid.");
                        //    return data;
                        //}

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
                _logger.LogError(ex, $"{className}.{methodName}: Error in downloading and decrypting: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }

        private (byte[] signature, byte[] data) ExtractSignatureAndData(byte[] combinedBytes, int signatureLength)
        {
            // Extract signature
            byte[] signature = combinedBytes.Take(signatureLength).ToArray();

            // Extract data
            byte[] data = combinedBytes.Skip(signatureLength).ToArray();

            return (signature, data);
        }

        private byte[] SignData(byte[] data, byte[] privateKey)
        {
            using (var rsa = RSA.Create(Constants.RsaKeySize))
            {
                rsa.ImportRSAPrivateKey(privateKey, out _);
                return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            }
        }

        private byte[] CombineDataAndSignature(byte[] dataBytes, byte[] signature)
        {
            using (var combinedStream = new MemoryStream())
            {
                combinedStream.Write(signature, 0, signature.Length);
                combinedStream.Write(dataBytes, 0, dataBytes.Length);

                return combinedStream.ToArray();
            }
        }

        private byte[] ConvertStreamToByteArray(Stream stream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
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

        private async Task UploadFile(AmazonS3Client s3Client, string bucketName, string? folderName, string fileName, Stream inputStream)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = folderName != null ? $"{folderName.TrimEnd('/')}/{fileName}" : fileName,
                    InputStream = inputStream
                };

                await s3Client.PutObjectAsync(putRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.UploadFile: ERROR uploading file to S3 bucket: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }

        }

        private void EncryptStream(byte[] inputData, Stream outputStream, byte[] fisPublicKey)
        {
            try
            {

                using (var rsa = RSA.Create(Constants.RsaKeySize))
                {
                    rsa.ImportRSAPublicKey(fisPublicKey, out _);
                    int keySizeBytes = rsa.KeySize / 8;
                    int maxDataSize = keySizeBytes - Constants.OaepSHA256PaddingOverhead;
                    int offset = 0;
                    long totalLength = inputData.Length;

                    while (offset < totalLength)
                    {
                        long remainingBytes = totalLength - offset;
                        int count = (int)Math.Min(remainingBytes, maxDataSize);
                        byte[] buffer = new byte[count];
                        Array.Copy(inputData, offset, buffer, 0, count);

                        // Encrypt the bytes in the buffer
                        byte[] encryptedData = rsa.Encrypt(buffer, RSAEncryptionPadding.OaepSHA256);
                        outputStream.Write(encryptedData, 0, encryptedData.Length);

                        offset += count;
                    }
                }
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, $"{className}.EncryptStream: ERROR encrypting file data: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }

        private bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey)
        {
            using (var rsa = RSA.Create(Constants.RsaKeySize))
            {
                rsa.ImportRSAPublicKey(publicKey, out _);
                return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            }
        }

        private void DecryptStream(Stream inputStream, Stream outputStream, byte[] privateKey)
        {
            try
            {
                using (var rsa = RSA.Create(Constants.RsaKeySize))
                {
                    rsa.ImportRSAPrivateKey(privateKey, out _);

                    int keySizeBytes = rsa.KeySize / 8;
                    // Encrypted block size is equal to key size in bytes
                    int encryptedBlockSize = keySizeBytes;

                    byte[] buffer = new byte[encryptedBlockSize];
                    int bytesRead;

                    while ((bytesRead = inputStream.Read(buffer, 0, encryptedBlockSize)) > 0)
                    {
                        byte[] decryptedData = rsa.Decrypt(buffer, RSAEncryptionPadding.OaepSHA256);
                        outputStream.Write(decryptedData, 0, decryptedData.Length);
                    }
                }
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, $"{className}.DecryptStream: ERROR decrypting stream: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }
        }
    }
}