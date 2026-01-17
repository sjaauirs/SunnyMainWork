using Microsoft.Extensions.Configuration;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.AwsConfig
{
    public class AwsConfiguration
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
        public string GetAwsTmpS3BucketName() => _configuration.GetSection("AWS:AWS_TMP_BUCKET_NAME").Value?.ToString() ?? "";



    }
}
