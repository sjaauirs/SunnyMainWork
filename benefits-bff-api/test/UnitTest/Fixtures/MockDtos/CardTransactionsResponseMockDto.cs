using SunnyBenefits.Fis.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class CardTransactionsResponseMockDto : CardTransactionsResponseDto
    {
        public CardTransactionsResponseMockDto()
        {
            CardTransactions = new List<CardTransactionsDetailsDto>
            {
               new CardTransactionsDetailsDto
                {
                    CardTransaction = new CardTransactionsDto
                    {
                     Account = "1234567812345678",
    TranDate = DateTime.UtcNow,
    PostDate = DateTime.UtcNow.AddDays(1),
    Description = "Purchase at XYZ Store",
    Reference = "REF123456789",
    Amt = 50.75m,
    Inserted = DateTime.UtcNow,
    Merchant = "XYZ Store",
    TxnType = "Purchase",
    RequestCode = "1001",
    StrategyName = "Standard",
    Comment = "No issues",
    MerchantNo = "123456",
    MCC = "5411",
    ResponseCode = "0",
    SettleAmount = 50.75m,
    CountryName = "United States",
    AuthCode = "AUTH1234",
    LocalAmount = 50.75m,
    ReasonDescription = "N/A",
    ResponseDescription = "Approved",
    UserId = "user123",
    CustomIndicator = "NA",
    TranResult = "Success",
    Reversed = false,
    ClientRefNum = "CLIENT12345",
    PurseNo = "PURSE01",
    PurseCanDoId = "PCAID1234",
    Pan = "1234567812345678",
    TxnLevel = "Level1",
    ApplyReq = "APPREQ01",
    SettleSeq = "SEQ001",
    SCode = "SC123",
    ReasonCode = "RC01",
    BuyerLoaderId = "BLID001",
    Approved = true,
    CardholderAdj = "1",
    ReasonId = "RI001",
    CardholderFee = "0",
    LocalTranCurrCode = "USD",
    IssuingCurrCode = "USD",
    LocalCurrCode = "840",
    MerchAddr = "123 Main St, Anytown, USA",
    PanNumber = "1234567812345678",
    CardStatus = "Active",
    CardStatusDesc = "Card is active",
    SourceDesc = "Visa",
    CreditCardNum = "4111111111111111",
    BuyerLast = "Doe",
    BuyerFirst = "John",
    BuyerMiddle = "A",
    DdaAccount = "DA123456",
    DdaRouting = "123456789",
    DdaInstitution = "BankOfAmerica",
    PosEntry = "Manual",
    PosCond = "Normal",
    AuthAmount = 50.75m,
    Variance = 0.00m,
    OrigAuthAmount = 50.75m,
    TerminalId = "TERM001",
    ExpDate = "12/25",
    AddressVerif = "Y",
    MccDescription = "Grocery Stores",
    MatchStatusDesc = "Matched",
    MatchTypeDesc = "Exact",
    BinCurrCode = "USD",
    TxnDate = DateTime.UtcNow,
    Aauid = "AAUID123456789",
    CountryCode = "US",
    PayExpDate = DateTime.UtcNow,
    AcqInstCode = "ACQ123",
    AcquirerId = "ACQID123",
    CaId = "CAID123",
    LogId = "LOG123456",
    CardPbmStatus = "Active",
    FxRate = "1.0000",
    UtcPostDate = DateTime.UtcNow,
    UtcInserted = DateTime.UtcNow,
    MerchantName = "XYZ Store",
    Tolerance = "0.00",
    PanProxyKey = "PANPROXY123",
    ProxyKey = "PROXY123456"
                } }
            };
        }
    }
}
