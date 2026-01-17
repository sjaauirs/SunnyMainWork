using Amazon.S3;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NHibernate;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.FIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class PerformExternalTxnSyncServiceTest
    {
        private Mock<ILogger<PerformExternalTxnSyncService>> loggerMock;
        private Mock<ITenantRepo> tenantRepoMock;
        private Mock<ITenantAccountRepo> tenantAccountRepoMock;
        private Mock<IMonetaryTransactionRepo> monetaryTransactionRepoMock;
        private Mock<IConsumerAccountRepo> consumerAccountRepoMock;
        private Mock<IWalletTypeRepo> walletTypeRepoMock;
        private Mock<IWalletRepo> walletRepoMock;
        private Mock<ITransactionRepo> transactionRepoMock;
        private PerformExternalTxnSyncService service;
        private Mock<ISession> session;
        private Mock<ITransaction> transaction;


        public PerformExternalTxnSyncServiceTest() {

             loggerMock = new Mock<ILogger<PerformExternalTxnSyncService>>();
             tenantRepoMock = new Mock<ITenantRepo>();
             tenantAccountRepoMock = new Mock<ITenantAccountRepo>();
             monetaryTransactionRepoMock = new Mock<IMonetaryTransactionRepo>();
             consumerAccountRepoMock = new Mock<IConsumerAccountRepo>();
             walletTypeRepoMock = new Mock<IWalletTypeRepo>();
             walletRepoMock = new Mock<IWalletRepo>();
             transactionRepoMock = new Mock<ITransactionRepo>();
             session = new Mock<ISession>();
            transaction = new Mock<ITransaction>();

             service = new PerformExternalTxnSyncService(
                loggerMock.Object,
                tenantAccountRepoMock.Object,
                monetaryTransactionRepoMock.Object,
                session.Object, // Mock ISession as needed for your tests
                consumerAccountRepoMock.Object,
                walletTypeRepoMock.Object,
                walletRepoMock.Object,
                tenantRepoMock.Object,
                transactionRepoMock.Object
            );
        }
        [Fact]
        public async System.Threading.Tasks.Task PerformExternalTxnSync_ValidTenantCode_CallsProcessMonetaryTransaction()
        {
            var etlExecutionContext = new EtlExecutionContext
            {
                TenantCode = "validTenantCode"
            };
            ETLConsumerAccountModel consumerModel=new ETLConsumerAccountModel { ConsumerCode=It.IsAny<String>()};
            ETLWalletTypeModel walletTypeModel =new ETLWalletTypeModel { WalletTypeId=1 , WalletTypeCode=It.IsAny<String>()};
            ETLWalletModel walletModel =new ETLWalletModel { WalletTypeId=1,WalletCode= It.IsAny<String>() };
            ETLTransactionModel txnModel =new ETLTransactionModel { TransactionId=1,TransactionCode= It.IsAny<String>() };
            var tenant = new ETLTenantModel { TenantCode = "validTenantCode", DeleteNbr = 0 };
            tenantRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantModel, bool>>>(), false))
                          .ReturnsAsync(tenant);
            var tenantAccountModel = new ETLTenantAccountModel { TenantCode = "validTenantCode", DeleteNbr = 0, TenantConfigJson = JsonConvert.SerializeObject(new FISTenantConfigDto { PurseConfig = new FISPurseConfigDto { Purses = new List<FISPurseDto> { new FISPurseDto { PurseNumber = 789, PurseWalletType = It.IsAny<string>() } } } }) };
            tenantAccountRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTenantAccountModel, bool>>>(), false))
                                 .ReturnsAsync(tenantAccountModel);

            var monetarydetails= new List<ETLMonetaryTransactionModel>
            { new ETLMonetaryTransactionModel{
                MonetaryTransactionId = 1,
                RecordType = 'A',
                IssuerClientId = 12345,
                ClientName = "Sample Client",
                SubProgramId = 6789,
                SubProgramName = "Sample SubProgram",
                Bin = 54321,
                BinCurrencyAlpha = "USD",
                BinCurrencyCode = "840",
                BankName = "Sample Bank",
                Pan = "1234567890123456",
                CardNumber = "7890123456789012",
                AuthorizationAmount = 100.50m,
                AuthorizationCode = "AUTH1234",
                TxnLocalAmount = 95.75m,
                TxnLocDateTime = DateTime.UtcNow,
                TxnSign = -1,
                TransactionCurrencyCode = 840,
                TransactionCurrencyAlpha = "USD",
                TxnTypeCode = 123,
                ReasonCode = 456,
                DerivedRequestCode = 789,
                ResponseCode = 101,
                MatchStatusCode = 1,
                MatchTypeCode = 2,
                InitialLoadDateFlag = DateTime.UtcNow.Date,
                Mcc = 1234,
                MerchantCurrencyAlpha = "USD",
                MerchantCurrencyCode = "840",
                MerchantName = "Sample Merchant",
                MerchantNumber = "MERCHANT123",
                ReferenceNumber = "REF123",
                PaymentMethodId = 1,
                SettleAmount = 95.00m,
                WcsUtcPostDate = DateTime.UtcNow,
                SourceCode = 987,
                AcquirerReferenceNumber = "ACQREF123",
                AcquirerId = 56789,
                AddressVerificationResponse = 'Y',
                AdjustAmount = 10.25m,
                AuthorizationResponse = "Approved",
                AvsInformation = "AVS Info",
                Denomination = 50.00m,
                DirectAccessNumber = "DIRECT123",
                CardNumberProxy = "PROXY123",
                FudgeAmt = 5.00m,
                MatchStatusDescription = "Match Status",
                MatchTypeDescription = "Match Type",
                MccDescription = "MCC Description",
                MerchantZip = "12345",
                MerchantCity = "Sample City",
                MerchantCountryCode = "US",
                MerchantCountryName = "United States",
                MerchantProvince = "Sample Province",
                MerchantState = "Sample State",
                MerchantStreet = "Sample Street",
                Pin = 1234,
                PosData = "POS Data",
                PosEntryCode = 1,
                PosEntryDescription = "POS Entry Desc",
                PurseNo = 789,
                ReasonCodeDescription = "Reason Desc",
                DerivedRequestCodeDescription = "Derived Req Desc",
                ResponseDescription = "Response Desc",
                RetrievalRefNo = "RETR123",
                Reversed = 0,
                SourceDescription = "Source Desc",
                TerminalNumber = "TERM123",
                TxnTypeName = "ValueLoad",
                UserId = "user123",
                UserFirstName = "John",
                UserLastName = "Doe",
                WcsLocalPostDate = DateTime.UtcNow.Date,
                Comment = "Sample Comment",
                ClientReferenceNumber = "CLIENTREF123",
                ClientSpecificId = "SPECIFICID123",
                ActualRequestCode = 987,
                ActualRequestCodeDescription = "Actual Req Desc",
                CardholderClientUniqueId = "CARDHOLDER123",
                PanProxyNumber = "PANPROXY123",
                TxnUid = "TXN123",
                PurseName = "Sample Purse",
                PurseStatus = "Active",
                PurseCreationDate = DateTime.UtcNow.Date,
                PurseEffectiveDate = DateTime.UtcNow.Date,
                PurseExpirationDate = DateTime.UtcNow.Date.AddYears(1),
                PurseStatusDate = DateTime.UtcNow.Date,
                AssociationSource = "Association Source",
                ReasonId = 789,
                ReasonDescription = "Reason Desc",
                Variance = 1.5m,
                ProcessCode = "Process Code",
                TokenUniqueReferenceId = "TOKENREF123",
                PanUniqueReferenceId = "PANUNIQ123",
                TokenTransactionId = "TOKENTXN123",
                TokenStatus = 'A',
                TokenStatusDescription = "Token Status Desc",
                NetworkReferenceId = "NETREF123",
                MultiClearingIndication = "Multi Clearing",
                AuthorizationBalance = 100.00m,
                SettleBalance = 95.00m,
                WcsLocalInserted = DateTime.UtcNow,
                WcsUtcInserted = DateTime.UtcNow,
                WcsUtcUpdated = DateTime.UtcNow,
                DiscountAmount = 5.00m,
                AchEffectiveDate = DateTime.UtcNow.Date,
                CardPresent = 1,
                DeviceType = "Device Type",
                SpendCategory = "Spend Category",
                FilterType = "Filter Type",
                PaymentAccountReference = "PAYREF123",
                OriginalMerchantNumber = "ORGMERCHANT123",
                CreateTimestamp = DateTime.UtcNow,
                DeleteNbr = 0
            }
            };
            IEnumerable<ETLTenantAccountModel> data = new List<ETLTenantAccountModel>
{
    new ETLTenantAccountModel { TenantCode = "validTenantCode", DeleteNbr = 0 ,TenantAccountId=2 },
    // Add more instances as needed
};
            monetaryTransactionRepoMock.Setup(x=>x.FindAsync(It.IsAny<Expression<Func<ETLMonetaryTransactionModel, bool>>>(), false)).ReturnsAsync(monetarydetails);
            session.Setup(x => x.BeginTransaction()).Returns(transaction.Object);
            consumerAccountRepoMock.Setup(x=>x.FindOneAsync(It.IsAny<Expression<Func<ETLConsumerAccountModel, bool>>>(), false)).ReturnsAsync(consumerModel);
            walletTypeRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLWalletTypeModel, bool>>>(), false)).ReturnsAsync(walletTypeModel);
            walletRepoMock.Setup(x => x.GetWalletByConsumerAndWalletType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(walletModel);
            transactionRepoMock.Setup(x => x.GetMaxTransactionIdByWallet(It.IsAny<long>())).ReturnsAsync(It.IsAny<long>());
            transactionRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ETLTransactionModel, bool>>>(), false)).ReturnsAsync(txnModel);
            session.Setup(x => x.SaveAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);
            session.Setup(x => x.Query<ETLTenantAccountModel>())
                   .Returns(() => data.AsQueryable());
            session.Setup(x => x.UpdateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                   .Returns<object, CancellationToken>((obj, cancellationToken) =>
                   {
                       
                       return System.Threading.Tasks.Task.CompletedTask;
                   });
            transaction.Setup(x => x.Commit());
            // Act
            await service.PerformExternalTxnSync(etlExecutionContext);

            // Assert
            monetaryTransactionRepoMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<ETLMonetaryTransactionModel, bool>>>(),false), Times.Once);
        }
    }
}
