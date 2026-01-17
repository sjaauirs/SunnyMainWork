using Amazon.S3;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class VaultMock : Mock<IVault>
    {
        public VaultMock()
        {
            Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync(It.IsAny<string>());


        }
    }
}
