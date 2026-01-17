using Amazon.S3;
using Moq;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class FileHelperMock : Mock<IFileHelper>
    {
        public FileHelperMock()
        {
            Setup(x => x.UploadFile(It.IsAny<AmazonS3Client>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()));        


        }
    }
}

