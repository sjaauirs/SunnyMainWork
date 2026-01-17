using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Infrastructure.AWSConfig.Interface
{
    public interface IAwsConfiguration
    {
        Task<string> GetAwsAccessKey();
        Task<string> GetAwsSecretKey();
        string GetAwsPublicS3BucketName();
        Task<string> AgreementPublicFolderPath();
        Task<string> UploadAgreementPublicFolderPath();


    }
}
