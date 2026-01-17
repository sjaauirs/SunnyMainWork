using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    /// <summary>
    /// Costom Exception to raise and catch for ETL errors
    /// </summary>
    public class EtlJobException : Exception
    {
        public EtlJobException(string message)
        : base(message)
        {
        }
    }
}
