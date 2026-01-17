using Microsoft.Extensions.Configuration;
using NHibernate.Cfg;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ETLBatchJobRecordQueueRequestDto
    {
        private readonly IVault _vault;
        private readonly IConfiguration _cofiguration;


        // Constructor to initialize _vault
        public ETLBatchJobRecordQueueRequestDto(IVault vault, IConfiguration configuration)
        {
            _vault = vault;
            _cofiguration = configuration;

        }

        // Property to hold the environment
        public string Environment { get; private set; } = string.Empty;
        public string JobType { get; set; }=string.Empty;
        public string AdminPanelLink { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public async Task InitializeAsync()
        {
            
            Environment = await _vault.GetSecret("env");
            AdminPanelLink = _cofiguration.GetSection("AdminPortalBatchJobReportUrl").Value?? throw new NullReferenceException();
           
        }
    }
}
