using Microsoft.Extensions.Configuration;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Infrastructure.AWSConfig
{

    public class AwsConfiguration :IAwsConfiguration
    {
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;

        public AwsConfiguration(IVault vault, IConfiguration configuration)
        {
            _vault = vault;
            _configuration = configuration;
        }

        public async Task<string> GetAwsAccessKey() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_ACCESS_KEY_NAME").Value?.ToString() ?? "");
        public async Task<string> GetAwsSecretKey() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_SECRET_KEY_NAME").Value?.ToString() ?? "");
        public async Task<string> AgreementPublicFolderPath() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_AGREEMENT_FOLDER_PATH_NAME").Value?.ToString() ?? "");
        public async Task<string> UploadAgreementPublicFolderPath() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_UPLOAD_AGREEMENT_PUBLIC_FOLDER_NAME").Value?.ToString() ?? "");
        public string GetAwsPublicS3BucketName() => _configuration.GetSection("AWS:AWS_PUBLIC_BUCKET_NAME").Value?.ToString() ?? "";


    }
}
