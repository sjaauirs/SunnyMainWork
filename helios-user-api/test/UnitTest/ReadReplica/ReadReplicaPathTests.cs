using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class ReadReplicaPathTests
    {
        [Fact]
        public async Task AddressTypeService_GetAllAddressTypes_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var addressTypeRepo = new Mock<IAddressTypeRepo>();
            var logger = new Mock<ILogger<AddressTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var addressTypes = new List<AddressTypeModel>
            {
                new AddressTypeModel { AddressTypeId = 1, AddressTypeName = "Home", DeleteNbr = 0 }
            };
            
            var mockQueryOver = new Mock<IQueryOver<AddressTypeModel, AddressTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<AddressTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(addressTypes);
            
            mockSession.Setup(s => s.QueryOver<AddressTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            
            mapper.Setup(m => m.Map<IList<AddressTypeDto>>(addressTypes))
                .Returns(new List<AddressTypeDto> { new AddressTypeDto { AddressTypeId = 1 } });

            var service = new AddressTypeService(addressTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAllAddressTypes();

            // Assert
            mockSession.Verify(s => s.QueryOver<AddressTypeModel>(), Times.Once);
            addressTypeRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false), Times.Never);
        }

        [Fact]
        public async Task AddressTypeService_GetAddressTypeById_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var addressTypeRepo = new Mock<IAddressTypeRepo>();
            var logger = new Mock<ILogger<AddressTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var addressType = new AddressTypeModel { AddressTypeId = 1, AddressTypeName = "Home", DeleteNbr = 0 };
            
            var mockQueryOver = new Mock<IQueryOver<AddressTypeModel, AddressTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<AddressTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync(addressType);
            
            mockSession.Setup(s => s.QueryOver<AddressTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            
            mapper.Setup(m => m.Map<AddressTypeDto>(addressType))
                .Returns(new AddressTypeDto { AddressTypeId = 1 });

            var service = new AddressTypeService(addressTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAddressTypeById(1);

            // Assert
            mockSession.Verify(s => s.QueryOver<AddressTypeModel>(), Times.Once);
            addressTypeRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<AddressTypeModel, bool>>>(), false), Times.Never);
        }

        [Fact]
        public async Task PhoneTypeService_GetAllPhoneTypes_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var phoneTypeRepo = new Mock<IPhoneTypeRepo>();
            var logger = new Mock<ILogger<PhoneTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var phoneTypes = new List<PhoneTypeModel>
            {
                new PhoneTypeModel { PhoneTypeId = 1, PhoneTypeName = "Mobile", DeleteNbr = 0 }
            };
            
            var mockQueryOver = new Mock<IQueryOver<PhoneTypeModel, PhoneTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(phoneTypes);
            
            mockSession.Setup(s => s.QueryOver<PhoneTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            
            mapper.Setup(m => m.Map<IList<PhoneTypeDto>>(phoneTypes))
                .Returns(new List<PhoneTypeDto> { new PhoneTypeDto { PhoneTypeId = 1 } });

            var service = new PhoneTypeService(phoneTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAllPhoneTypes();

            // Assert
            mockSession.Verify(s => s.QueryOver<PhoneTypeModel>(), Times.Once);
            phoneTypeRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false), Times.Never);
        }

        [Fact]
        public async Task PhoneTypeService_GetPhoneTypeById_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var phoneTypeRepo = new Mock<IPhoneTypeRepo>();
            var logger = new Mock<ILogger<PhoneTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var phoneType = new PhoneTypeModel { PhoneTypeId = 1, PhoneTypeName = "Mobile", DeleteNbr = 0 };
            
            var mockQueryOver = new Mock<IQueryOver<PhoneTypeModel, PhoneTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync(phoneType);
            
            mockSession.Setup(s => s.QueryOver<PhoneTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            
            mapper.Setup(m => m.Map<PhoneTypeDto>(phoneType))
                .Returns(new PhoneTypeDto { PhoneTypeId = 1 });

            var service = new PhoneTypeService(phoneTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetPhoneTypeById(1);

            // Assert
            mockSession.Verify(s => s.QueryOver<PhoneTypeModel>(), Times.Once);
            phoneTypeRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>(), false), Times.Never);
        }

        [Fact]
        public async Task AddressTypeService_GetAllAddressTypes_WithReadReplica_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            var addressTypeRepo = new Mock<IAddressTypeRepo>();
            var logger = new Mock<ILogger<AddressTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var mockQueryOver = new Mock<IQueryOver<AddressTypeModel, AddressTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<AddressTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(new List<AddressTypeModel>());
            
            mockSession.Setup(s => s.QueryOver<AddressTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = new AddressTypeService(addressTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAllAddressTypes();

            // Assert
            Assert.Equal(404, result.ErrorCode);
        }

        [Fact]
        public async Task PhoneTypeService_GetAllPhoneTypes_WithReadReplica_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            var phoneTypeRepo = new Mock<IPhoneTypeRepo>();
            var logger = new Mock<ILogger<PhoneTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var mockQueryOver = new Mock<IQueryOver<PhoneTypeModel, PhoneTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(new List<PhoneTypeModel>());
            
            mockSession.Setup(s => s.QueryOver<PhoneTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = new PhoneTypeService(phoneTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAllPhoneTypes();

            // Assert
            Assert.Equal(404, result.ErrorCode);
        }

        [Fact]
        public async Task AddressTypeService_GetAddressTypeById_WithReadReplica_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var addressTypeRepo = new Mock<IAddressTypeRepo>();
            var logger = new Mock<ILogger<AddressTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var mockQueryOver = new Mock<IQueryOver<AddressTypeModel, AddressTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<AddressTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync((AddressTypeModel?)null);
            
            mockSession.Setup(s => s.QueryOver<AddressTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = new AddressTypeService(addressTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAddressTypeById(999);

            // Assert
            Assert.Null(result.AddressType);
        }

        [Fact]
        public async Task PhoneTypeService_GetPhoneTypeById_WithReadReplica_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var phoneTypeRepo = new Mock<IPhoneTypeRepo>();
            var logger = new Mock<ILogger<PhoneTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var mockQueryOver = new Mock<IQueryOver<PhoneTypeModel, PhoneTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync((PhoneTypeModel?)null);
            
            mockSession.Setup(s => s.QueryOver<PhoneTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = new PhoneTypeService(phoneTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetPhoneTypeById(999);

            // Assert
            Assert.Null(result.PhoneType);
        }

        [Fact]
        public async Task AddressTypeService_GetAllAddressTypes_WithReadReplica_MapsResultsCorrectly()
        {
            // Arrange
            var addressTypeRepo = new Mock<IAddressTypeRepo>();
            var logger = new Mock<ILogger<AddressTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var addressTypes = new List<AddressTypeModel>
            {
                new AddressTypeModel { AddressTypeId = 1, AddressTypeName = "Home", DeleteNbr = 0 },
                new AddressTypeModel { AddressTypeId = 2, AddressTypeName = "Work", DeleteNbr = 0 }
            };
            var addressTypeDtos = new List<AddressTypeDto>
            {
                new AddressTypeDto { AddressTypeId = 1, AddressTypeName = "Home" },
                new AddressTypeDto { AddressTypeId = 2, AddressTypeName = "Work" }
            };
            
            var mockQueryOver = new Mock<IQueryOver<AddressTypeModel, AddressTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<AddressTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(addressTypes);
            
            mockSession.Setup(s => s.QueryOver<AddressTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            mapper.Setup(m => m.Map<IList<AddressTypeDto>>(addressTypes)).Returns(addressTypeDtos);

            var service = new AddressTypeService(addressTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAllAddressTypes();

            // Assert
            Assert.NotNull(result.AddressTypesList);
            Assert.Equal(2, result.AddressTypesList.Count);
            mapper.Verify(m => m.Map<IList<AddressTypeDto>>(addressTypes), Times.Once);
        }

        [Fact]
        public async Task PhoneTypeService_GetAllPhoneTypes_WithReadReplica_MapsResultsCorrectly()
        {
            // Arrange
            var phoneTypeRepo = new Mock<IPhoneTypeRepo>();
            var logger = new Mock<ILogger<PhoneTypeService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var phoneTypes = new List<PhoneTypeModel>
            {
                new PhoneTypeModel { PhoneTypeId = 1, PhoneTypeName = "Mobile", DeleteNbr = 0 },
                new PhoneTypeModel { PhoneTypeId = 2, PhoneTypeName = "Home", DeleteNbr = 0 }
            };
            var phoneTypeDtos = new List<PhoneTypeDto>
            {
                new PhoneTypeDto { PhoneTypeId = 1, PhoneTypeName = "Mobile" },
                new PhoneTypeDto { PhoneTypeId = 2, PhoneTypeName = "Home" }
            };
            
            var mockQueryOver = new Mock<IQueryOver<PhoneTypeModel, PhoneTypeModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<PhoneTypeModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(phoneTypes);
            
            mockSession.Setup(s => s.QueryOver<PhoneTypeModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            mapper.Setup(m => m.Map<IList<PhoneTypeDto>>(phoneTypes)).Returns(phoneTypeDtos);

            var service = new PhoneTypeService(phoneTypeRepo.Object, logger.Object, mapper.Object, readOnlySession.Object);

            // Act
            var result = await service.GetAllPhoneTypes();

            // Assert
            Assert.NotNull(result.PhoneTypesList);
            Assert.Equal(2, result.PhoneTypesList.Count);
            mapper.Verify(m => m.Map<IList<PhoneTypeDto>>(phoneTypes), Times.Once);
        }

        // PersonService Read Replica Tests
        [Fact]
        public async Task PersonService_GetPersonData_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var personRepo = new Mock<IPersonRepo>();
            var consumerRepo = new Mock<IConsumerRepo>();
            var consumerService = new Mock<IConsumerService>();
            var logger = new Mock<ILogger<PersonService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var person = new PersonModel { PersonId = 1, FirstName = "John", LastName = "Doe", DeleteNbr = 0 };
            var personDto = new PersonDto { PersonId = 1, FirstName = "John", LastName = "Doe" };
            
            var mockQueryOver = new Mock<IQueryOver<PersonModel, PersonModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<PersonModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync(person);
            
            mockSession.Setup(s => s.QueryOver<PersonModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            mapper.Setup(m => m.Map<PersonDto>(person)).Returns(personDto);

            var service = new PersonService(logger.Object, mapper.Object, personRepo.Object, consumerRepo.Object, consumerService.Object, readOnlySession.Object);

            // Act
            var result = await service.GetPersonData(1);

            // Assert
            mockSession.Verify(s => s.QueryOver<PersonModel>(), Times.Once);
            personRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false), Times.Never);
            Assert.Equal(1, result.PersonId);
        }

        [Fact]
        public async Task PersonService_GetPersonData_WithReadReplica_ReturnsEmpty_WhenNotFound()
        {
            // Arrange
            var personRepo = new Mock<IPersonRepo>();
            var consumerRepo = new Mock<IConsumerRepo>();
            var consumerService = new Mock<IConsumerService>();
            var logger = new Mock<ILogger<PersonService>>();
            var mapper = new Mock<IMapper>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var mockQueryOver = new Mock<IQueryOver<PersonModel, PersonModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<PersonModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.SingleOrDefaultAsync(default))
                .ReturnsAsync((PersonModel?)null);
            
            mockSession.Setup(s => s.QueryOver<PersonModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = new PersonService(logger.Object, mapper.Object, personRepo.Object, consumerRepo.Object, consumerService.Object, readOnlySession.Object);

            // Act
            var result = await service.GetPersonData(999);

            // Assert
            Assert.Equal(0, result.PersonId);
        }

        [Fact]
        public async Task PersonService_GetPersonData_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var personRepo = new Mock<IPersonRepo>();
            var consumerRepo = new Mock<IConsumerRepo>();
            var consumerService = new Mock<IConsumerService>();
            var logger = new Mock<ILogger<PersonService>>();
            var mapper = new Mock<IMapper>();
            
            var person = new PersonModel { PersonId = 1, FirstName = "John", LastName = "Doe", DeleteNbr = 0 };
            var personDto = new PersonDto { PersonId = 1, FirstName = "John", LastName = "Doe" };
            
            personRepo
                .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(person);
            mapper.Setup(m => m.Map<PersonDto>(person)).Returns(personDto);

            var service = new PersonService(logger.Object, mapper.Object, personRepo.Object, consumerRepo.Object, consumerService.Object, null);

            // Act
            var result = await service.GetPersonData(1);

            // Assert
            personRepo.Verify(r => r.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false), Times.Once);
            Assert.Equal(1, result.PersonId);
        }

        // ConsumerDeviceService Read Replica Tests
        [Fact]
        public async Task ConsumerDeviceService_GetConsumerDevices_WithReadReplica_ExecutesReadReplicaPath()
        {
            // Arrange
            var consumerDeviceRepo = new Mock<IConsumerDeviceRepo>();
            var logger = new Mock<ILogger<ConsumerDeviceService>>();
            var encryptionHelper = new Mock<IEncryptionHelper>();
            var vault = new Mock<IVault>();
            var mapper = new Mock<IMapper>();
            var hashingService = new Mock<IHashingService>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerDeviceRequestDto { TenantCode = "TEST", ConsumerCode = "CONS001" };
            var devices = new List<ConsumerDeviceModel>
            {
                new ConsumerDeviceModel { ConsumerDeviceId = 1, TenantCode = "TEST", ConsumerCode = "CONS001", DeviceType = "iOS", DeleteNbr = 0 }
            };
            var deviceDtos = new List<ConsumerDeviceDto>
            {
                new ConsumerDeviceDto { ConsumerDeviceId = 1, DeviceType = "iOS" }
            };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerDeviceModel, ConsumerDeviceModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(devices);
            
            mockSession.Setup(s => s.QueryOver<ConsumerDeviceModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            mapper.Setup(m => m.Map<IList<ConsumerDeviceDto>>(devices)).Returns(deviceDtos);

            var service = new ConsumerDeviceService(logger.Object, consumerDeviceRepo.Object, encryptionHelper.Object, vault.Object, mapper.Object, hashingService.Object, readOnlySession.Object);

            // Act
            var result = await service.GetConsumerDevices(request);

            // Assert
            mockSession.Verify(s => s.QueryOver<ConsumerDeviceModel>(), Times.Once);
            consumerDeviceRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false), Times.Never);
            Assert.NotNull(result.ConsumerDevices);
        }

        [Fact]
        public async Task ConsumerDeviceService_GetConsumerDevices_WithReadReplica_ReturnsNotFound_WhenEmpty()
        {
            // Arrange
            var consumerDeviceRepo = new Mock<IConsumerDeviceRepo>();
            var logger = new Mock<ILogger<ConsumerDeviceService>>();
            var encryptionHelper = new Mock<IEncryptionHelper>();
            var vault = new Mock<IVault>();
            var mapper = new Mock<IMapper>();
            var hashingService = new Mock<IHashingService>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerDeviceRequestDto { TenantCode = "TEST", ConsumerCode = "CONS001" };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerDeviceModel, ConsumerDeviceModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(new List<ConsumerDeviceModel>());
            
            mockSession.Setup(s => s.QueryOver<ConsumerDeviceModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);

            var service = new ConsumerDeviceService(logger.Object, consumerDeviceRepo.Object, encryptionHelper.Object, vault.Object, mapper.Object, hashingService.Object, readOnlySession.Object);

            // Act
            var result = await service.GetConsumerDevices(request);

            // Assert
            Assert.Equal(404, result.ErrorCode);
        }

        [Fact]
        public async Task ConsumerDeviceService_GetConsumerDevices_WithoutReadReplica_UsesRepository()
        {
            // Arrange
            var consumerDeviceRepo = new Mock<IConsumerDeviceRepo>();
            var logger = new Mock<ILogger<ConsumerDeviceService>>();
            var encryptionHelper = new Mock<IEncryptionHelper>();
            var vault = new Mock<IVault>();
            var mapper = new Mock<IMapper>();
            var hashingService = new Mock<IHashingService>();
            
            var request = new GetConsumerDeviceRequestDto { TenantCode = "TEST", ConsumerCode = "CONS001" };
            var devices = new List<ConsumerDeviceModel>
            {
                new ConsumerDeviceModel { ConsumerDeviceId = 1, TenantCode = "TEST", ConsumerCode = "CONS001", DeviceType = "iOS", DeleteNbr = 0 }
            };
            var deviceDtos = new List<ConsumerDeviceDto>
            {
                new ConsumerDeviceDto { ConsumerDeviceId = 1, DeviceType = "iOS" }
            };
            
            consumerDeviceRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false))
                .ReturnsAsync(devices);
            mapper.Setup(m => m.Map<IList<ConsumerDeviceDto>>(devices)).Returns(deviceDtos);

            var service = new ConsumerDeviceService(logger.Object, consumerDeviceRepo.Object, encryptionHelper.Object, vault.Object, mapper.Object, hashingService.Object, null);

            // Act
            var result = await service.GetConsumerDevices(request);

            // Assert
            consumerDeviceRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false), Times.Once);
            Assert.NotNull(result.ConsumerDevices);
        }

        [Fact]
        public async Task ConsumerDeviceService_GetConsumerDevices_WithReadReplica_MapsResultsCorrectly()
        {
            // Arrange
            var consumerDeviceRepo = new Mock<IConsumerDeviceRepo>();
            var logger = new Mock<ILogger<ConsumerDeviceService>>();
            var encryptionHelper = new Mock<IEncryptionHelper>();
            var vault = new Mock<IVault>();
            var mapper = new Mock<IMapper>();
            var hashingService = new Mock<IHashingService>();
            var readOnlySession = new Mock<IReadOnlySession>();
            var mockSession = new Mock<NHibernate.ISession>();
            
            var request = new GetConsumerDeviceRequestDto { TenantCode = "TEST", ConsumerCode = "CONS001" };
            var devices = new List<ConsumerDeviceModel>
            {
                new ConsumerDeviceModel { ConsumerDeviceId = 1, TenantCode = "TEST", ConsumerCode = "CONS001", DeviceType = "iOS", DeleteNbr = 0 },
                new ConsumerDeviceModel { ConsumerDeviceId = 2, TenantCode = "TEST", ConsumerCode = "CONS001", DeviceType = "Android", DeleteNbr = 0 }
            };
            var deviceDtos = new List<ConsumerDeviceDto>
            {
                new ConsumerDeviceDto { ConsumerDeviceId = 1, DeviceType = "iOS" },
                new ConsumerDeviceDto { ConsumerDeviceId = 2, DeviceType = "Android" }
            };
            
            var mockQueryOver = new Mock<IQueryOver<ConsumerDeviceModel, ConsumerDeviceModel>>();
            mockQueryOver
                .Setup(q => q.Where(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>()))
                .Returns(mockQueryOver.Object);
            mockQueryOver
                .Setup(q => q.ListAsync(default))
                .ReturnsAsync(devices);
            
            mockSession.Setup(s => s.QueryOver<ConsumerDeviceModel>()).Returns(mockQueryOver.Object);
            readOnlySession.Setup(r => r.Session).Returns(mockSession.Object);
            mapper.Setup(m => m.Map<IList<ConsumerDeviceDto>>(devices)).Returns(deviceDtos);

            var service = new ConsumerDeviceService(logger.Object, consumerDeviceRepo.Object, encryptionHelper.Object, vault.Object, mapper.Object, hashingService.Object, readOnlySession.Object);

            // Act
            var result = await service.GetConsumerDevices(request);

            // Assert
            Assert.NotNull(result.ConsumerDevices);
            Assert.Equal(2, result.ConsumerDevices.Count);
            mapper.Verify(m => m.Map<IList<ConsumerDeviceDto>>(devices), Times.Once);
        }
    }
}

