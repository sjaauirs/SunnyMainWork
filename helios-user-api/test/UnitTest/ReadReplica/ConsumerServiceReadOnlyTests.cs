using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class ConsumerServiceReadOnlyTests
    {
        private readonly Mock<ILogger<ConsumerService>> _logger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly Mock<IRoleRepo> _roleRepo;
        private readonly Mock<NHibernate.ISession> _primarySession;
        private readonly Mock<IPersonRoleRepo> _personRoleRepo;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IAddressTypeService> _addressTypeService;
        private readonly Mock<IUploadAgreementPDFService> _uploadPdfService;
        private readonly Mock<IPersonAddressRepo> _personAddressRepo;
        private readonly Mock<IPhoneNumberRepo> _phoneNumberRepo;
        private readonly Mock<IMemberImportFileDataRepo> _memberImportFileDataRepo;
        private readonly Mock<IEventService> _eventService;
        private readonly Mock<IConsumerETLRepo> _consumerETLRepo;
        private readonly Mock<IHeliosEventPublisher<AgreementsVerifiedEventDto>> _heliosEventPublisher;

        public ConsumerServiceReadOnlyTests()
        {
            _logger = new Mock<ILogger<ConsumerService>>();
            _mapper = new Mock<IMapper>();
            _consumerRepo = new Mock<IConsumerRepo>();
            _personRepo = new Mock<IPersonRepo>();
            _roleRepo = new Mock<IRoleRepo>();
            _primarySession = new Mock<NHibernate.ISession>();
            _personRoleRepo = new Mock<IPersonRoleRepo>();
            _tenantClient = new Mock<ITenantClient>();
            _addressTypeService = new Mock<IAddressTypeService>();
            _uploadPdfService = new Mock<IUploadAgreementPDFService>();
            _personAddressRepo = new Mock<IPersonAddressRepo>();
            _phoneNumberRepo = new Mock<IPhoneNumberRepo>();
            _memberImportFileDataRepo = new Mock<IMemberImportFileDataRepo>();
            _eventService = new Mock<IEventService>();
            _consumerETLRepo = new Mock<IConsumerETLRepo>();
            _heliosEventPublisher = new Mock<IHeliosEventPublisher<AgreementsVerifiedEventDto>>();
        }

        private ConsumerService CreateService(IReadOnlySession? readOnlySession = null)
        {
            return new ConsumerService(
                _logger.Object,
                _mapper.Object,
                _consumerRepo.Object,
                _personRepo.Object,
                _roleRepo.Object,
                _primarySession.Object,
                _personRoleRepo.Object,
                _tenantClient.Object,
                _addressTypeService.Object,
                _uploadPdfService.Object,
                _personAddressRepo.Object,
                _phoneNumberRepo.Object,
                _memberImportFileDataRepo.Object,
                _eventService.Object,
                _consumerETLRepo.Object,
                _heliosEventPublisher.Object,
                readOnlySession);
        }

        [Fact]
        public async Task GetConsumerData_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerRequestDto { ConsumerCode = "CONS001" };
            var consumer = new ConsumerModel { ConsumerId = 1, ConsumerCode = "CONS001", DeleteNbr = 0 };
            var consumerDto = new ConsumerDto { ConsumerId = 1, ConsumerCode = "CONS001" };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerModel, ConsumerModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync(consumer);
            
            mockSession.Setup(s => s.QueryOver<ConsumerModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            _mapper.Setup(m => m.Map<ConsumerDto>(consumer)).Returns(consumerDto);

            var service = CreateService(readOnlySession.Object);

            // Act
            var result = await service.GetConsumerData(request);

            // Assert
            mockSession.Verify(s => s.QueryOver<ConsumerModel>(), Times.Once);
            _consumerRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false), Times.Never);
            Assert.NotNull(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerData_WithReadReplica_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerRequestDto { ConsumerCode = "NOTFOUND" };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerModel, ConsumerModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync((ConsumerModel?)null);
            
            mockSession.Setup(s => s.QueryOver<ConsumerModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = CreateService(readOnlySession.Object);

            // Act
            var result = await service.GetConsumerData(request);

            // Assert
            Assert.Null(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerData_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var request = new GetConsumerRequestDto { ConsumerCode = "CONS001" };
            var consumer = new ConsumerModel { ConsumerId = 1, ConsumerCode = "CONS001", DeleteNbr = 0 };
            var consumerDto = new ConsumerDto { ConsumerId = 1, ConsumerCode = "CONS001" };
            
            _consumerRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);
            _mapper.Setup(m => m.Map<ConsumerDto>(consumer)).Returns(consumerDto);

            var service = CreateService(null);

            // Act
            var result = await service.GetConsumerData(request);

            // Assert
            _consumerRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false), Times.Once);
            Assert.NotNull(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerByMemId_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerByMemIdRequestDto { TenantCode = "TEST", MemberId = "MEM001" };
            var consumer = new ConsumerModel { ConsumerId = 1, TenantCode = "TEST", MemberId = "MEM001", DeleteNbr = 0 };
            var responseDto = new GetConsumerByMemIdResponseDto { Consumer = new ConsumerDto { ConsumerId = 1 } };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerModel, ConsumerModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync(consumer);
            
            mockSession.Setup(s => s.QueryOver<ConsumerModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            _mapper.Setup(m => m.Map<GetConsumerByMemIdResponseDto>(consumer)).Returns(responseDto);

            var service = CreateService(readOnlySession.Object);

            // Act
            var result = await service.GetConsumerByMemId(request);

            // Assert
            mockSession.Verify(s => s.QueryOver<ConsumerModel>(), Times.Once);
            _consumerRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false), Times.Never);
            Assert.NotNull(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerByMemId_WithReadReplica_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerByMemIdRequestDto { TenantCode = "TEST", MemberId = "NOTFOUND" };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerModel, ConsumerModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync((ConsumerModel?)null);
            
            mockSession.Setup(s => s.QueryOver<ConsumerModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = CreateService(readOnlySession.Object);

            // Act
            var result = await service.GetConsumerByMemId(request);

            // Assert
            Assert.Null(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerByMemId_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var request = new GetConsumerByMemIdRequestDto { TenantCode = "TEST", MemberId = "MEM001" };
            var consumer = new ConsumerModel { ConsumerId = 1, TenantCode = "TEST", MemberId = "MEM001", DeleteNbr = 0 };
            var responseDto = new GetConsumerByMemIdResponseDto { Consumer = new ConsumerDto { ConsumerId = 1 } };
            
            _consumerRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumer);
            _mapper.Setup(m => m.Map<GetConsumerByMemIdResponseDto>(consumer)).Returns(responseDto);

            var service = CreateService(null);

            // Act
            var result = await service.GetConsumerByMemId(request);

            // Assert
            _consumerRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false), Times.Once);
            Assert.NotNull(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerData_WithReadReplica_MapsResultCorrectly()
        {
            // Arrange
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerRequestDto { ConsumerCode = "CONS001" };
            var consumer = new ConsumerModel 
            { 
                ConsumerId = 1, 
                ConsumerCode = "CONS001", 
                TenantCode = "TEST",
                MemberId = "MEM001",
                DeleteNbr = 0 
            };
            var consumerDto = new ConsumerDto 
            { 
                ConsumerId = 1, 
                ConsumerCode = "CONS001",
                TenantCode = "TEST",
                MemberId = "MEM001"
            };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerModel, ConsumerModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync(consumer);
            
            mockSession.Setup(s => s.QueryOver<ConsumerModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            _mapper.Setup(m => m.Map<ConsumerDto>(consumer)).Returns(consumerDto);

            var service = CreateService(readOnlySession.Object);

            // Act
            var result = await service.GetConsumerData(request);

            // Assert
            Assert.NotNull(result.Consumer);
            Assert.Equal("CONS001", result.Consumer.ConsumerCode);
            Assert.Equal("TEST", result.Consumer.TenantCode);
            _mapper.Verify(m => m.Map<ConsumerDto>(consumer), Times.Once);
        }
    }
}

