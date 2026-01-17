using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using NSubstitute;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Helpers;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockHttpClient;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Helpers
{
    public class ConsumerServiceMock
    {
        private readonly Mock<ILogger<ConsumerService>> _mockConsumerLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConsumerRepo> _mockConsumerRepo;
        private readonly Mock<IPersonRepo> _mockPersonRepo;
        private readonly Mock<IRoleRepo> _mockRoleRepo;
        private readonly Mock<IPersonRoleRepo> _mockPersonRoleRepo;
        private readonly Mock<NHibernate.ISession> _mockSession;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IAddressTypeService> _addressTypeService;
        private readonly Mock<IUploadAgreementPDFService> _uploadpdfService;
        private readonly Mock<IPersonAddressRepo> _personAddressRepo;
        private readonly Mock<IPhoneNumberRepo> _phoneNumberRepo;
        private readonly Mock<IMemberImportFileDataRepo> _memberImportFileDataRepo;
        private readonly Mock<IConsumerETLRepo> _consumerETLRepo;
        private readonly ConsumerService _consumerService;
        private readonly Mock<IEventService> _eventService;
        private readonly Mock<IHeliosEventPublisher<AgreementsVerifiedEventDto>> _heliosEventPublisher;
        public ConsumerServiceMock()
        {
            _mockConsumerLogger = new Mock<ILogger<ConsumerService>>();
            _mockMapper = new Mock<IMapper>();
            _mockConsumerRepo = new Mock<IConsumerRepo>();
            _mockPersonRepo = new Mock<IPersonRepo>();
            _mockRoleRepo = new Mock<IRoleRepo>();
            _mockSession = new Mock<NHibernate.ISession>();
            _mockPersonRoleRepo = new Mock<IPersonRoleRepo>();
            _addressTypeService = new Mock<IAddressTypeService>();
            _uploadpdfService = new Mock<IUploadAgreementPDFService>();
            _tenantClient = new TenantClientMock();
            _personAddressRepo = new PersonAddressMockRepo();
            _memberImportFileDataRepo = new Mock<IMemberImportFileDataRepo>();
            _consumerETLRepo = new Mock<IConsumerETLRepo>();
            _eventService = new Mock<IEventService>();
            _phoneNumberRepo = new Mock<IPhoneNumberRepo>();
            _heliosEventPublisher = new Mock<IHeliosEventPublisher<AgreementsVerifiedEventDto>>();
            // Create an instance of ConsumerService with mocked dependencies
            _consumerService = new ConsumerService(
                _mockConsumerLogger.Object,
                _mockMapper.Object,
                _mockConsumerRepo.Object,
                _mockPersonRepo.Object,
                _mockRoleRepo.Object,
                _mockSession.Object,
                _mockPersonRoleRepo.Object,
                _tenantClient.Object,
                _addressTypeService.Object, _uploadpdfService.Object, _personAddressRepo.Object, _phoneNumberRepo.Object, _memberImportFileDataRepo.Object ,_eventService.Object, _consumerETLRepo.Object, _heliosEventPublisher.Object);
        }
        [Fact]
        public async Task UpdateConsumers_ShouldReturnExpectedResults()
        {
            // Arrange
            var ConsumerDataMockDto = new List<ConsumerDataDto>
            {
                new ConsumerDataDto {
                Person = new PersonDto()
                {
                    PersonId = 120,
                    PersonCode = "per-45i741-df2e-4f5-ab26-670d444f1c",
                    FirstName = "sunny",
                    LastName = "rewards",
                    LanguageCode = "en-US",
                    MemberSince = DateTime.UtcNow,
                    Email = "sunnyrewards@gmail.com",
                    City = "New york",
                    Country = "US",
                    YearOfBirth = 1998,
                    PostalCode = "(555)777-8888",
                    PhoneNumber = "97867588788",
                    Region = "US",
                    DOB = DateTime.UtcNow,
                    Gender = "Female",
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 1,
                },

                Consumer = new ConsumerDto()
                {
                    ConsumerId = 3,
                    PersonId = 120,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    ConsumerCode = "cmr-bjuebdf-492f-46i4-bh55-5a2b0134cbc",
                    Registered = true,
                    Eligible = true,
                    MemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",
                    RegistrationTs = DateTime.Now,
                    EligibleStartTs = DateTime.Now,
                    EligibleEndTs = DateTime.Now,
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 1,
                    SubscriberMemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",
                          ConsumerAttribute = "{\n\"teest\": \"takke\"\n}",


                }
            }};
            var consumerModel = new ConsumerMockDto();
            var personModel = new PersonMockDto();
            var consumerDataResponseDto = new ConsumerDataResponseDto();

            _mockConsumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
            _mockConsumerRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerModel> { new ConsumerModel {  ConsumerId = 1,
            PersonId = 1,
            TenantCode = "ten-91532506c8d468e1d27704",
            ConsumerCode = "cmr--91532578681e1d27704",
            RegistrationTs = DateTime.UtcNow,
            EligibleStartTs = DateTime.UtcNow,
            EligibleEndTs = DateTime.UtcNow.AddDays(30),
            Registered = false,
            Eligible = true,
            MemberNbr = "69-676-815ec8aefa64",
            SubscriberMemberNbr = "69-676-815ec8aefa64",
      ConsumerAttribute = "{\n\"test\": \"take\"\n}",

                } });

            _mockPersonRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(personModel);
            var transactionMock = new Mock<ITransaction>();
            
            _mockSession.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _mockSession.Setup(s => s.UpdateAsync(It.IsAny<PersonModel>(), default));
            _mockSession.Setup(s => s.UpdateAsync(It.IsAny<ConsumerModel>(), default));
            _mockMapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                .Returns(new PersonDto());
            _mockMapper.Setup(mapper => mapper.Map<ConsumerDto>(It.IsAny<ConsumerModel>()))
                .Returns(new ConsumerDto());

            // Act
            var result = await _consumerService.UpdateConsumers(ConsumerDataMockDto,true);

            // Assert
          
            Assert.NotNull(result[0].Person);
            Assert.NotNull(result[0].Consumer);
        }

        
        [Fact]
        public async Task UpdateConsumers_ShouldReturn_nullResults()
        {
            // Arrange
            var ConsumerDataMockDto = new List<ConsumerDataDto>
            {
                new ConsumerDataDto {
                Person = new PersonDto()
                {
                    PersonId = 120,
                    PersonCode = "per-45i741-df2e-4f5-ab26-670d444f1c",
                    FirstName = "sunny",
                    LastName = "rewards",
                    LanguageCode = "en-US",
                    MemberSince = DateTime.UtcNow,
                    Email = "sunnyrewards@gmail.com",
                    City = "New york",
                    Country = "US",
                    YearOfBirth = 1998,
                    PostalCode = "(555)777-8888",
                    PhoneNumber = "97867588788",
                    Region = "US",
                    DOB = DateTime.UtcNow,
                    Gender = "Female",
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 1,
                },

                Consumer = new ConsumerDto()
                {
                    ConsumerId = 3,
                    PersonId = 120,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    ConsumerCode = "cmr-bjuebdf-492f-46i4-bh55-5a2b0134cbc",
                    Registered = true,
                    Eligible = true,
                    MemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",
                    RegistrationTs = DateTime.Now,
                    EligibleStartTs = DateTime.Now,
                    EligibleEndTs = DateTime.Now,
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 1,
                    SubscriberMemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",

                }
            }};
            var consumerModel = new ConsumerMockDto();
            var personModel = new PersonMockDto();
            var consumerDataResponseDto = new ConsumerDataResponseDto();

            _mockConsumerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
            _mockConsumerRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false))
               ;

            _mockPersonRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false))
                .ReturnsAsync(personModel);
            var transactionMock = new Mock<ITransaction>();

            _mockSession.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _mockSession.Setup(s => s.UpdateAsync(It.IsAny<PersonModel>(), default));
            _mockSession.Setup(s => s.UpdateAsync(It.IsAny<ConsumerModel>(), default));
            _mockMapper.Setup(mapper => mapper.Map<PersonDto>(It.IsAny<PersonModel>()))
                .Returns(new PersonDto());
            _mockMapper.Setup(mapper => mapper.Map<ConsumerDto>(It.IsAny<ConsumerModel>()))
                .Returns(new ConsumerDto());

            // Act
            var result = await _consumerService.UpdateConsumers(ConsumerDataMockDto, true);

            // Assert

            Assert.NotNull(result[0].Person);
        }

    }
}
