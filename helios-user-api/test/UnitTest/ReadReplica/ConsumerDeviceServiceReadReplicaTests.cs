using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class ConsumerDeviceServiceReadReplicaTests
    {
        private readonly Mock<IConsumerDeviceRepo> _consumerDeviceRepo;
        private readonly Mock<ILogger<ConsumerDeviceService>> _logger;
        private readonly Mock<IEncryptionHelper> _encryptionHelper;
        private readonly Mock<IVault> _vault;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IHashingService> _hashingService;

        public ConsumerDeviceServiceReadReplicaTests()
        {
            _consumerDeviceRepo = new Mock<IConsumerDeviceRepo>();
            _logger = new Mock<ILogger<ConsumerDeviceService>>();
            _encryptionHelper = new Mock<IEncryptionHelper>();
            _vault = new Mock<IVault>();
            _mapper = new Mock<IMapper>();
            _hashingService = new Mock<IHashingService>();
        }

        [Fact]
        public async Task GetConsumerDevices_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var request = new GetConsumerDeviceRequestDto { TenantCode = "TEST", ConsumerCode = "CONS001" };
            var mockModels = new List<ConsumerDeviceModel>
            {
                new ConsumerDeviceModel 
                { 
                    ConsumerDeviceId = 1, 
                    TenantCode = "TEST", 
                    ConsumerCode = "CONS001",
                    DeviceType = "iOS",
                    DeleteNbr = 0 
                }
            };
            var mockDtos = new List<ConsumerDeviceDto>
            {
                new ConsumerDeviceDto { ConsumerDeviceId = 1, DeviceType = "iOS" }
            };

            _consumerDeviceRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false))
                .ReturnsAsync(mockModels);
            _mapper.Setup(m => m.Map<IList<ConsumerDeviceDto>>(mockModels)).Returns(mockDtos);

            var service = new ConsumerDeviceService(
                _logger.Object, 
                _consumerDeviceRepo.Object, 
                _encryptionHelper.Object, 
                _vault.Object, 
                _mapper.Object, 
                _hashingService.Object, 
                null);

            // Act
            var result = await service.GetConsumerDevices(request);

            // Assert
            Assert.NotNull(result.ConsumerDevices);
            Assert.Single(result.ConsumerDevices);
            _consumerDeviceRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false), Times.Once);
        }

        [Fact]
        public async Task GetConsumerDevices_WithoutReadReplica_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            var request = new GetConsumerDeviceRequestDto { TenantCode = "TEST", ConsumerCode = "CONS001" };
            
            _consumerDeviceRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerDeviceModel>());

            var service = new ConsumerDeviceService(
                _logger.Object, 
                _consumerDeviceRepo.Object, 
                _encryptionHelper.Object, 
                _vault.Object, 
                _mapper.Object, 
                _hashingService.Object, 
                null);

            // Act
            var result = await service.GetConsumerDevices(request);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerDevices_WithoutReadReplica_ThrowsException_OnError()
        {
            // Arrange
            var request = new GetConsumerDeviceRequestDto { TenantCode = "TEST", ConsumerCode = "CONS001" };
            
            _consumerDeviceRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            var service = new ConsumerDeviceService(
                _logger.Object, 
                _consumerDeviceRepo.Object, 
                _encryptionHelper.Object, 
                _vault.Object, 
                _mapper.Object, 
                _hashingService.Object, 
                null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.GetConsumerDevices(request));
        }
    }
}

