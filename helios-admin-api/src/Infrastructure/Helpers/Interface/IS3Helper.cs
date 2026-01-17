using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface
{
    public interface IS3Helper
    {
        Task<bool> UploadFileToS3(string s3BucketName, IFormFile formFile, string key);
        Task<ImportDto?> UnzipAndProcessJsonFromS3(string s3Key, string tenantCode, string bucketName);
    }
}
