using Microsoft.Extensions.Configuration;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class ConfigurationMock : Mock<IConfiguration>
    {
        public ConfigurationMock()
        {
            Setup(c => c.GetSection(It.IsAny<string>()).Value).Returns(It.IsAny<string>());


        }
    }
}
