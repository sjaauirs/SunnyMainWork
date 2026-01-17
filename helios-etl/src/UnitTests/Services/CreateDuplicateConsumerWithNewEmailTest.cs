using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class CreateDuplicateConsumerWithNewEmailTest
    {
        private Mock<ILogger<CreateDuplicateConsumerWithNewEmail>> _loggerMock;
        private Mock<ITenantRepo> _tenantRepoMock;
        private Mock<IConsumerRepo> _consumerRepoMock;
        private Mock<IPersonRepo> _personRepoMock;
        private Mock<ICustomerRepo> _customerRepo;
        private Mock<ISponsorRepo> _sponsorRepo;
        private CreateDuplicateConsumerWithNewEmail createDuplicateConsumerWithNewEmail;
        private Mock<IMemberImportService> _memberImport;
        private Mock<IPersonAddressRepo> _personAddressRepo;
        private Mock<IPhoneNumberRepo> _phoneNumberRepo;


        public CreateDuplicateConsumerWithNewEmailTest()
        {
            _loggerMock = new Mock<ILogger<CreateDuplicateConsumerWithNewEmail>>();
            _consumerRepoMock = new Mock<IConsumerRepo>();
            _tenantRepoMock = new Mock<ITenantRepo>();
            _personRepoMock = new Mock<IPersonRepo>();
            _memberImport = new Mock<IMemberImportService>();
            _customerRepo = new Mock<ICustomerRepo>();
            _sponsorRepo = new Mock<ISponsorRepo>();
            _personAddressRepo = new Mock<IPersonAddressRepo>();
            _phoneNumberRepo = new Mock<IPhoneNumberRepo>();
            createDuplicateConsumerWithNewEmail = new CreateDuplicateConsumerWithNewEmail(_loggerMock.Object, _sponsorRepo.Object, _customerRepo.Object, _consumerRepoMock.Object, _personRepoMock.Object
               , _tenantRepoMock.Object, _memberImport.Object, _personAddressRepo.Object, _phoneNumberRepo.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateDuplicateConsumerWithNewEmail_ValidTenant_Success()
        {
            var etlExecutionContext = new EtlExecutionContext
            {
                TenantCode = "validTenantCode",
                ConsumerCode = "Test",
                NewEmail = "test123@gmail.com",
                CustomerCode = "877hh",
                CustomerLabel = "test",
                IsCreateDuplicateConsumer = true
            };

            var tenant = new ETLTenantModel { TenantCode = "validTenantCode", DeleteNbr = 0 };
            var sponsor = new ETLSponsorModel { CustomerId = 1, DeleteNbr = 0 };
            var token = new TokenResponseDto { JWT = "test" };
            var response = new MembersResponseDto { Consumers = new List<ConsumerDataResponseDto>(), ConsumerWallets = new List<Wallet.Core.Domain.Dtos.ConsumerWalletDataResponseDto>() };
            var customer = new CustomerModel { CustomerId = 1, CustomerCode = "877hh", CustomerName = "test" };

            var consumerModel = new ETLConsumerModel { PersonId = 1, TenantCode = "validTenantCode", DeleteNbr = 0, MemberNbr = "test", SubscriberMemberNbr = "test", EligibleEndTs = DateTime.Now.AddDays(1), EligibleStartTs = DateTime.Now };
            var person = await personModelData();
            _consumerRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);
            _tenantRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                .ReturnsAsync(tenant);
            _personRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLPersonModel, bool>>>(), false))
                .ReturnsAsync(person);

            _customerRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<CustomerModel, bool>>>(), false))
                .ReturnsAsync(customer);
            _sponsorRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLSponsorModel, bool>>>(), false))
                .ReturnsAsync(sponsor);
            _memberImport.Setup(x => x.ProcessBatchAsync(It.IsAny<List<MemberImportCSVDto>>(), etlExecutionContext)).ReturnsAsync((new List<ETLConsumerModel> { new ETLConsumerModel() }, new List<ETLPersonModel> { new ETLPersonModel() }));

            _personAddressRepo.Setup(repo => repo.FindOneAsync(
                It.IsAny<Expression<Func<ETLPersonAddressModel, bool>>>(), false))
                .ReturnsAsync(new ETLPersonAddressModel
                {
                    PersonId = person.PersonId,
                    AddressTypeId = (long)AddressTypeEnum.MAILING,
                    IsPrimary = true,
                    Line1 = "123 Main St",
                    City = "Testville",
                    State = "TS",
                    PostalCode = "12345"
                });

            _personAddressRepo.Setup(repo => repo.FindOneAsync(
                It.Is<Expression<Func<ETLPersonAddressModel, bool>>>(expr =>
                    expr.ToString().Contains("HOME") && expr.ToString().Contains("ETL")), false))
                .ReturnsAsync(new ETLPersonAddressModel
                {
                    PersonId = person.PersonId,
                    AddressTypeId = (long)AddressTypeEnum.HOME,
                    Source = "ETL",
                    Line1 = "456 Home Ave",
                    City = "Hometown",
                    State = "HT",
                    PostalCode = "67890"
                });

            _phoneNumberRepo.Setup(repo => repo.FindOneAsync(
                It.IsAny<Expression<Func<ETLPhoneNumberModel, bool>>>(), false))
                .ReturnsAsync(new ETLPhoneNumberModel
                {
                    PersonId = person.PersonId,
                    PhoneTypeId = (long)PhoneTypeEnum.MOBILE,
                    PhoneNumber = "1111111111",
                    IsPrimary = true
                });

            _phoneNumberRepo.Setup(repo => repo.FindOneAsync(
                It.Is<Expression<Func<ETLPhoneNumberModel, bool>>>(expr =>
                    expr.ToString().Contains("HOME") && expr.ToString().Contains("ETL")), false))
                .ReturnsAsync(new ETLPhoneNumberModel
                {
                    PersonId = person.PersonId,
                    PhoneTypeId = (long)PhoneTypeEnum.HOME,
                    Source = "ETL",
                    PhoneNumber = "1111111111"
                });

            await createDuplicateConsumerWithNewEmail.CreateDuplicateConsumer(etlExecutionContext);


            _memberImport.Verify(
               x => x.ProcessBatchAsync(It.IsAny<List<MemberImportCSVDto>>(), etlExecutionContext),
               Times.Once);

        }

        [Fact]
        public async System.Threading.Tasks.Task CreateDuplicateConsumerWithNewEmail_TenantNotFound_Should_Throw_ETLException()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext
            {
                TenantCode = "validTenantCode",
                ConsumerCode = "con-12swerfdtghyu",
                NewEmail = "test123@gmail.com",
                CustomerCode = "877hh",
                CustomerLabel = "test",
                IsCreateDuplicateConsumer = true
            };

            var tenant = new ETLTenantModel { TenantCode = "validTenantCode", DeleteNbr = 0 };
            var consumerModel = new ETLConsumerModel { PersonId = 1, TenantCode = "validTenantCode", DeleteNbr = 0, MemberNbr = "test", SubscriberMemberNbr = "test", EligibleEndTs = DateTime.Now.AddDays(1), EligibleStartTs = DateTime.Now };
            var person = await personModelData();
            _consumerRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerModel, bool>>>(), false))
                .ReturnsAsync(consumerModel);

            _personRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLPersonModel, bool>>>(), false))
                .ReturnsAsync(person);

            _tenantRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false));

            // Act
            var ex = await Assert.ThrowsAsync<ETLException>(() =>
             createDuplicateConsumerWithNewEmail.CreateDuplicateConsumer(etlExecutionContext));

            // Assert
            Assert.Equal((int)ETLExceptionCodes.NotFoundInDb, ex.ErrorCode);
        }

        public async Task<ETLPersonModel> personModelData()
        {
            var person = new ETLPersonModel
            {
                PersonId = 12345,
                PersonCode = "P12345",
                FirstName = "John",
                LastName = "Doe",
                LanguageCode = "EN",
                MemberSince = DateTime.UtcNow.AddYears(-5),
                Email = "john.doe@example.com",
                City = "Los Angeles",
                Country = "USA",
                YearOfBirth = 1990,
                PostalCode = "90001",
                PhoneNumber = "555-1234",
                Region = "California",
                Gender = "Male",
                DOB = new DateTime(1990, 1, 1),
                IsSpouse = false,
                IsDependent = true,
                SSN = "123-45-6789",
                SSNLast4 = "6789",
                MailingAddressLine1 = "123 Main St",
                MailingAddressLine2 = "Apt 4B",
                MailingState = "CA",
                MailingCountryCode = "US",
                HomePhoneNumber = "555-5678"
            };
            return person;
        }

    }
}
