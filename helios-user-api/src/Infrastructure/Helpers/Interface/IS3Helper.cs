using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Infrastructure.Helpers.Interface
{
    public interface IS3Helper
    {
        Task<bool> UploadFileToS3(Stream streamData, string s3BucketName, string fileName);
        Task<string> GetHtmlFromS3Async(string key, string bucketName);
    }
}
