using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Mapping.ByCode;
using NHibernate.Util;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using static SunnyRewards.Helios.ETL.Core.Domain.Dtos.PreRunValidationJson;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class MemberImportFileDataService : AwsConfiguration, IMemberImportFileDataService
    {
        private readonly ILogger<IMemberImportFileDataService> _logger;
        private readonly IETLMemberImportFileDataRepo _memberImportFileDataRepo;
        private readonly ITenantRepo _tenantRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IPersonRepo _personRepo;
        private readonly ISession _session;
        private readonly IETLMemberImportFileRepo _memberImportFileRepo;
        private readonly BatchJobReportValidationJson _batchJobReportValidationJson;
        private readonly PerTenantConsumerCount _perTenantConsumerCount;
        private readonly IAwsS3Service _awsS3Service;
        private string _fileName;

        public static readonly List<string> ActionType = new List<string> { Constants.ADD, Constants.UPDATE, Constants.DELETE, Constants.CANCEL };


        private const string className = nameof(MemberImportFileDataService);

        public BatchJobReportValidationJson batchJobReportValidationJson => _batchJobReportValidationJson;

        public PerTenantConsumerCount perTenantConsumerCountData => _perTenantConsumerCount;

        private readonly Dictionary<string, Action<string, string, int>> actionForPreRun;
        private readonly Dictionary<string, Action<string>> actionForPreRunUpdate;

        string[] formats = { "MM/dd/yyyy", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss" };
        public MemberImportFileDataService(IVault vault, ILogger<IMemberImportFileDataService> logger, IConfiguration configuration, ITenantRepo tenantRepo, IConsumerRepo consumerRepo, IPersonRepo personRepo, ISession session,
        IETLMemberImportFileDataRepo memberImportFileDataRepo, IETLMemberImportFileRepo memberImportFileRepo, IAwsS3Service awsS3Service) : base(vault, configuration)
        {
            _logger = logger;
            _batchJobReportValidationJson = new BatchJobReportValidationJson
            {
                PreRun = new PreRunValidationJson.PreRun(),
                PostRun = new PostRunValidationJson.PostRun(),
            };
            _perTenantConsumerCount = new PerTenantConsumerCount();
            _memberImportFileDataRepo = memberImportFileDataRepo;
            _memberImportFileRepo = memberImportFileRepo;
            _tenantRepo = tenantRepo;
            _consumerRepo = consumerRepo;
            _personRepo = personRepo;
            _session = session;
            actionForPreRun = InitialiseActionCount();
            actionForPreRunUpdate = initialiseActionIfConsumserexists();
            _awsS3Service = awsS3Service;
            _fileName = string.Empty;
        }

        public Dictionary<string, Action<string>> initialiseActionIfConsumserexists()
        {
            return new Dictionary<string, Action<string>>
             {
            { ActionTypes.InsertCode, (tenant_code) => {
                 var hasTenant=_batchJobReportValidationJson.PreRun?.PerTenantData.FirstOrDefault(x=>x.TenantCode==tenant_code);
                    if(hasTenant!=null)
                    {
                     hasTenant.Counts.TotalAdd--;
                     hasTenant.Counts.ValidAdd--;
                    hasTenant.Counts.TotalUpdate++;
                    hasTenant.Counts.ValidUpdate++;

                }
                           } },
        };
        }


        public async Task<(long, bool)> saveMemberImportFileData(EtlExecutionContext etlExecutionContext)
        {
            const string _methodName = nameof(saveMemberImportFileData);
            ETLMemberImportFileModel memberImportFileModel = new ETLMemberImportFileModel();

            string memberImportFilePath = etlExecutionContext.MemberImportFilePath;
            var memberImportFileContents = etlExecutionContext.MemberImportFileContents;
            string fileName = Path.GetFileName(etlExecutionContext.MemberImportFilePath);
            _fileName = fileName;
            if (string.IsNullOrEmpty(memberImportFilePath) && (memberImportFileContents == null || memberImportFileContents.Length == 0))
            {
                _logger.LogError("{ClassName}.{MethodName} -{FileName} Consumers import file paths or S3 file content needs to be provided", className, _methodName, fileName + "_Error");
                return (memberImportFileModel.MemberImportFileId, false);
            }
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    memberImportFileModel.FileName = fileName;
                    memberImportFileModel.CreateTs = DateTime.Now;
                    memberImportFileModel.CreateUser = Constants.CreateUserAsETL;
                    memberImportFileModel.MemberImportCode = "mic-" + Guid.NewGuid().ToString("N");
                    memberImportFileModel.FileStatus = nameof(FileStatus.NOT_STARTED);
                    memberImportFileModel.MemberImportFileId = Convert.ToInt32(await _session.SaveAsync(memberImportFileModel));

                    if (memberImportFileModel.MemberImportFileId <= 0)
                    {
                        _logger.LogError("{ClassName}.{MethodName} -{fileName} The File for Consumers import file data not save in MemberImportFile staging table", className, _methodName, fileName + "_Error");
                        return (memberImportFileModel.MemberImportFileId, false);
                    }

                    var memberImportFileReader = memberImportFileContents?.Length > 0
                       ? new StreamReader(new MemoryStream(memberImportFileContents))
                       : new StreamReader(memberImportFilePath);


                    var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = "\t", // Set the delimiter to a tab character
                        HasHeaderRecord = true,
                        MissingFieldFound = null,
                        HeaderValidated = null
                    };
                    using var csvReader = new CsvReader(memberImportFileReader, csvConfiguration);
                    int record = 0;
                    while (await csvReader.ReadAsync())
                    {
                        ETLMemberImportFileDataModel memberImportFileDataModel = new ETLMemberImportFileDataModel();

                        try
                        {
                            var consumerCsvDto = csvReader.GetRecord<MemberImportCSVDto>();
                            memberImportFileDataModel.MemberImportFileId = memberImportFileModel.MemberImportFileId;
                            memberImportFileDataModel.RecordNumber = record++;
                            memberImportFileDataModel.RawDataJson = null;
                            memberImportFileDataModel.CreateTs = DateTime.Now;
                            memberImportFileDataModel.CreateUser = Constants.CreateUserAsETL;
                            PopulateMemberImportFileData(memberImportFileDataModel, consumerCsvDto);

                            memberImportFileDataModel.MemberImportFileDataId = Convert.ToInt32(await _session.SaveAsync(memberImportFileDataModel));
                          
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("{ClassName}.{MethodName} - {fileName} Consumers import file data not save in MemberImportFileData staging table with message {message} for record {date}", className, _methodName, ex.Message
                             , fileName + "_Error", memberImportFileDataModel.ToJson());
                            continue;
                        }

                    }
                    transaction.Commit();

                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, $"{className}.{_methodName}:{fileName + "_Error"} Error processing transaction: {ex.Message}");
                    _logger.LogError(ex, $"{className}.{_methodName}: {fileName + "_Error"} Error for MemberImportFileData transaction record: {fileName}");
                    transaction.Rollback();
                    _session.Clear();
                    return (memberImportFileModel.MemberImportFileId, false);

                }
            }
            return (memberImportFileModel.MemberImportFileId, true);
        }

        public async Task UpdateMemberImportFileDataRecordProcessingStatus(long memberImportFileDataId, long recordProcessingStatus)
        {
            var memberImportFileDataRecord = await _memberImportFileDataRepo.FindOneAsync(x => x.MemberImportFileDataId == memberImportFileDataId && x.DeleteNbr == 0);
            if (memberImportFileDataRecord != null)
            {
                memberImportFileDataRecord.RecordProcessingStatus = recordProcessingStatus;
                memberImportFileDataRecord.UpdateUser = Constants.UpdateUser;
                memberImportFileDataRecord.UpdateTs = DateTime.UtcNow;
                await _memberImportFileDataRepo.UpdateAsync(memberImportFileDataRecord);
            }
            
        }
        public async Task<List<ETLMemberImportFileDataModel>> GetMemberImportFileDataRecords(long memberImportFileId, int memberbatchSize)
        {
            const string _methodName = nameof(GetMemberImportFileDataRecords);
            var file = await _memberImportFileRepo.FindOneAsync(x => x.MemberImportFileId == memberImportFileId && x.DeleteNbr == 0);
            if (file != null)
            {
                try
                {
                    var filedata = await _memberImportFileDataRepo.GetBatchedData(memberImportFileId, memberbatchSize);

                    if (filedata.Count <= 0)
                    {
                        _logger.LogError("{ClassName}.{MethodName} -{fileName} : No Records found in MemberImportFileData staging table for memberImportFileId  {memberImportFileId}", className, _methodName, file?.FileName + "_Error", memberImportFileId);
                        return new List<ETLMemberImportFileDataModel>();
                    }
                    _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.TotalRecords = filedata.Count;


                    //TODO
                    // 1- Success 
                    // 0 - Not Process
                    //-1 - Invalid/Failed
                    foreach (var data in filedata.ToList())
                    {
                        await ValidateMemberImportFileDataRecords(data, file.FileName);
                    }
                    var perTenantData = _batchJobReportValidationJson.PreRun?.PerTenantData;
                    if (perTenantData != null)
                    {
                        foreach (var data in perTenantData)
                        {
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.TotalAdd += data.Counts.TotalAdd;
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.TotalUpdate += data.Counts.TotalUpdate;
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.TotalDelete += data.Counts.TotalDelete;
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.TotalCancel += data.Counts.TotalCancel;
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.ValidUpdate += data.Counts.ValidUpdate;
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.ValidAdd += data.Counts.ValidAdd;
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.ValidCancel += data.Counts.ValidCancel;
                            _batchJobReportValidationJson.PreRun.CrossTenantData.Counts.ValidDelete += data.Counts.ValidDelete;
                        }
                    }

                    return filedata.Where(x => !_batchJobReportValidationJson.PreRun.InvalidRecords.InvalidAddRecords.Contains(x.RecordNumber) &&
                                                !_batchJobReportValidationJson.PreRun.InvalidRecords.InvalidUpdateRecords.Contains(x.RecordNumber) &&
                                                !_batchJobReportValidationJson.PreRun.InvalidRecords.InvalidCancelRecords.Contains(x.RecordNumber) &&
                                                !_batchJobReportValidationJson.PreRun.InvalidRecords.InvalidDeleteRecords.Contains(x.RecordNumber))
                                   .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError("{ClassName}.{MethodName} -{FileName} :An error occured while fetching Records from MemberImportFileData staging table for memberImportFileId  {memberImportFileId} message: {message}", className, _methodName, file.FileName + "_Error", memberImportFileId, ex.Message);

                    return new List<ETLMemberImportFileDataModel>();
                }
            }
            else
            {
                _logger.LogError("{ClassName}.{MethodName} -{fileName} : No Records found in MemberImportFile staging table for memberImportFileId  {memberImportFileId}", className, _methodName, file?.FileName + "_Error", memberImportFileId);
                return new List<ETLMemberImportFileDataModel>();
            }
        }

        public async Task<long> GetBatchedDataCount(long memberImportFileId)
        {
            return await _memberImportFileDataRepo.GetBatchedDataCount(memberImportFileId);
        }

        public async Task ValidateMemberImportFileDataRecords(ETLMemberImportFileDataModel memberImportdata, string fileName)
        {
            const string _methodName = nameof(ValidateMemberImportFileDataRecords);

            try
            {
                string countType = string.Empty;
                var consumerCsvDto = ConvertToConsumerCsvDto(memberImportdata);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(consumerCsvDto, new ValidationContext(consumerCsvDto), validationResults, true);

                if (!consumerCsvDto.is_sso_user.GetValueOrDefault())
                {
                    if (string.IsNullOrWhiteSpace(consumerCsvDto.email))
                    {
                        validationResults.Add(new ValidationResult("Email is required when SSO is not enabled.", new[] { nameof(consumerCsvDto.email) }));
                        isValid = false;
                    }
                    else
                    {
                        var emailAttribute = new EmailAddressAttribute();
                        if (!emailAttribute.IsValid(consumerCsvDto.email))
                        {
                            validationResults.Add(new ValidationResult("Invalid email format.", new[] { nameof(consumerCsvDto.email) }));
                            isValid = false;
                        }
                    }
                }



                if (string.IsNullOrEmpty(consumerCsvDto.action))
                {
                    _logger.LogError("{ClassName}.{MethodName} -{FileName} : An error occured while ValidateMemberImportFileDataRecords: partner_code or Action  missing for memberImportFiledata {memberImportFile}"
                                           , className, _methodName, fileName + "_error", memberImportdata.ToJson());
                    return;
                }

                var tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == consumerCsvDto.partner_code && x.DeleteNbr == 0);
                if (tenant == null || string.IsNullOrEmpty(consumerCsvDto.partner_code))
                {
                    await ProcessCount(consumerCsvDto, memberImportdata.RecordNumber, string.Empty, Constants.INVALID);
                    _logger.LogError("{ClassName}.{MethodName} - {FileName} : An error occured while ValidateMemberImportFileDataRecords Tenant Not exists for data memberImportFileId  {memberImportFile}"
                        , className, _methodName, fileName + "_error", memberImportdata.ToJson());
                    await ProcessCount(consumerCsvDto, memberImportdata.RecordNumber, tenant?.TenantCode, Constants.TOTAL);


                    return;
                }
                var tenantOption = tenant.TenantOption != null ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption) : new TenantOption();
                if (!_batchJobReportValidationJson.PreRun.PerTenantData.Where(x => x.TenantCode == tenant.TenantCode).Any())
                    _batchJobReportValidationJson.PreRun.PerTenantData.Add(new Core.Domain.Dtos.PreRunValidationJson.TenantData { TenantCode = tenant.TenantCode });
                var validationMessages = new StringBuilder();
                if (!isValid || await eligibilityDateCheck(consumerCsvDto, validationMessages)
                    || await ValidateAge(consumerCsvDto, validationMessages))
                {
                    await UpdateMemberImportFileDataRecordProcessingStatus(memberImportdata.MemberImportFileDataId, (long)RecordProcessingStatusType.FAILED);


                    if (validationResults.Any())
                    {
                        var errorMessages = validationResults
                        .Where(x => !string.IsNullOrEmpty(x.ErrorMessage))
                        .Select(x => x.ErrorMessage);

                        foreach (var message in errorMessages)
                        {
                            validationMessages.AppendLine(message);
                        }
                    }
                    _logger.LogError("{ClassName}.{MethodName} -{FileName} : Validation failed for memberImportFile {memberImportFile} with message {message}"
                       , className, _methodName, memberImportdata.ToJson(), fileName + "_Error", validationMessages.ToString());
                    await ProcessCount(consumerCsvDto, memberImportdata.RecordNumber, tenant?.TenantCode, Constants.INVALID);
                }
                else
                {
                    await ProcessCount(consumerCsvDto, memberImportdata.RecordNumber, tenant?.TenantCode, Constants.VALID);

                    if (await IsMemberNumberExist(tenant.TenantCode, consumerCsvDto.member_id))
                    {
                        await UpdateCountIfMemberNbrExists(tenant?.TenantCode, consumerCsvDto.action);
                    }

                }
                await ProcessCount(consumerCsvDto, memberImportdata.RecordNumber, tenant?.TenantCode, Constants.TOTAL);


                var tenantData = _batchJobReportValidationJson.PreRun.PerTenantData.FirstOrDefault(x => x.TenantCode == tenant.TenantCode);
                if (tenantData != null)
                {
                    tenantData.Counts.TotalRecords++;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("{ClassName}.{MethodName} -{FileName} : An error occured while ValidateMemberImportFileDataRecords memberImportFileId  {memberImportFileId} message: {message}", className, _methodName, fileName + "_Error", memberImportdata.MemberImportFileDataId, ex.Message);

                return;
            }
        }


        private async Task<bool> IsConsumerExistWithEmail(MemberImportCSVDto? memberImportCSVDto, StringBuilder message)
        {
            const string methodName = nameof(IsConsumerExistWithEmail);

            if (memberImportCSVDto == null)
            {
                message.AppendLine("Invalid member data.");
                return true;
            }

            var isSSOUser = memberImportCSVDto.is_sso_user ?? false;
            var email = memberImportCSVDto.email;

            if (!isSSOUser && string.IsNullOrWhiteSpace(email))
            {
                message.AppendLine("Invalid Email Address: Email is required for non-SSO users.");
                return true;
            }

            try
            {
                var person = await _personRepo.FindOneAsync(p =>
                    p.PersonUniqueIdentifier != null &&
                    p.PersonUniqueIdentifier == memberImportCSVDto.person_unique_identifier &&
                    p.DeleteNbr == 0);

                if (person == null)
                    return false;

                var consumer = await _consumerRepo.FindOneAsync(c =>
                    c.PersonId == person.PersonId &&
                    c.DeleteNbr == 0);

                if (consumer != null)
                {
                    message.AppendLine($"A consumer already exists with Member Number '{consumer.MemberNbr}' and Person Unique Identifier '{person.PersonUniqueIdentifier}'.");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName} - {FileName}: An error occurred while checking consumer by email or identifier. Error: {ErrorMessage}",
                    className, methodName, _fileName + "_Error", ex.Message);

                return false;
            }
        }
        private async Task<bool> IsMemberNumberExist(string TenantCode, string MemberId)
        {
            var consumer = await _consumerRepo.FindOneAsync(consumer => consumer.TenantCode == TenantCode && consumer.MemberId == MemberId && consumer.DeleteNbr == 0);
            return consumer != null;
        }
        private async Task<bool> eligibilityDateCheck(MemberImportCSVDto consumerCsvDto, StringBuilder message)
        {
            DateTime.TryParseExact(consumerCsvDto.eligibility_start, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eligibilityStart);
            DateTime.TryParseExact(consumerCsvDto.eligibility_end, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eligibilityEnd);


            if (eligibilityEnd <= eligibilityStart)
            {

                message.AppendLine("Invalid eligibility_end or eligibility_start Date");

                return true;
            }
            return false;
        }
        private int CalculateAge(DateTime dob)
        {
            const string _methodName = nameof(CalculateAge);

            var today = DateTime.Today;
            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age)) age--;
            _logger.LogInformation("{ClassName}.{MethodName} - Age of the member based on the dob is:{age}.", className, _methodName,age);

            return age;
        }

        private async Task<bool> ValidateAge(MemberImportCSVDto consumerCsvDto, StringBuilder message)
        {
            int age;
            const string _methodName = nameof(ValidateAge);

            _logger.LogInformation("{ClassName}.{MethodName} - started age calculation.", className, _methodName);

            if (!string.IsNullOrEmpty(consumerCsvDto.age) && int.TryParse(consumerCsvDto.age, out int parsedAge) && parsedAge >= 18)
            {
                _logger.LogInformation("{ClassName}.{MethodName} -  age is valid and >= 18..", className, _methodName);
               return false;
            }
            else
            {
                // Fallback to calculating age using DOB
                DateTime dob;
                DateTime.TryParseExact(consumerCsvDto.dob.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dob);
                age = CalculateAge(dob);

                if (age < 18)
                {
                    message.AppendLine("Invalid age member age is less than 18 years");
                    return true; // Skip members under 18
                }
            }
            return false;
        }
        private async Task ProcessCount(MemberImportCSVDto consumerCsvDto, int recordNumber, string tenantcode, string validationType)
        {
            if (actionForPreRun.ContainsKey(consumerCsvDto.action))
            {
                actionForPreRun[consumerCsvDto.action](tenantcode, validationType, recordNumber);
            }
        }

        // Commenting to make this check before member-import job during eligibility file transformation
        //private async Task<bool> IsMailingAddressFieldRequired(ETLTenantModel tenantModel, MemberImportCSVDto memberDto, StringBuilder message)
        //{
        //    var tenantOption = tenantModel.TenantOption != null
        //? JsonConvert.DeserializeObject<TenantOption>(tenantModel.TenantOption)
        //: new TenantOption();

        //    //CardIssueFlowType != "IMMIDIATE" ByPass validation 
        //    if (tenantOption != null && tenantOption.BenefitsOptions?.CardIssueFlowType != null &&
        //       tenantOption.BenefitsOptions.CardIssueFlowType.Count > 0 &&
        //       tenantOption.BenefitsOptions.CardIssueFlowType.FirstOrDefault() != nameof(CardIssueFlowType.IMMEDIATE))
        //    {
        //        return false;
        //    }

        //    if (tenantOption != null && tenantOption.Apps != null && tenantOption.Apps.Contains(Constants.Benefits) && (string.IsNullOrWhiteSpace(memberDto.mailing_address_line1)
        //        || string.IsNullOrWhiteSpace(memberDto.mailing_state) || string.IsNullOrWhiteSpace(memberDto.mailing_country_code) ||
        //        string.IsNullOrWhiteSpace(memberDto.home_phone_number) || string.IsNullOrWhiteSpace(memberDto.city) || string.IsNullOrWhiteSpace(memberDto.country)
        //        || string.IsNullOrWhiteSpace(memberDto.postal_code)))
        //    {
        //        message.AppendLine("Mailing fields are required");
        //        return true;
        //    }
        //    return false;
        //}

        private Dictionary<string, Action<string, string, int>> InitialiseActionCount()
        {
            return new Dictionary<string, Action<string, string, int>>
    {
        {
            ActionTypes.InsertCode,
            (tenant_code, count_type,record_number) =>
            {
                HandleCountUpdate(tenant_code, count_type, Constants.ADD,record_number);
            }
        },
        {
            ActionTypes.UpdateCode,
            (tenant_code, count_type,record_number) =>
            {
                HandleCountUpdate(tenant_code, count_type, Constants.UPDATE,record_number);
            }
        },
        {
            ActionTypes.DeleteCode,
            (tenant_code, count_type, record_number) =>
            {
                HandleCountUpdate(tenant_code, count_type, Constants.DELETE,record_number);
            }
        },
        {
            ActionTypes.CancelCode,
            (tenant_code, count_type,record_number) =>
            {
                HandleCountUpdate(tenant_code, count_type, Constants.CANCEL,record_number);
            }
        },

    };
        }

        private void HandleCountUpdate(string tenant_code, string count_type, string actionType, int recordNumber)
        {
            // Determine the appropriate count property based on action type
            string totalCountType = $"Total{actionType}";
            string validCountType = $"Valid{actionType}";

            // Choose which count property to update based on the `count_type`
            if (count_type.Equals(Constants.TOTAL, StringComparison.OrdinalIgnoreCase))
            {
                UpdateCount(tenant_code, totalCountType);
            }
            else if (count_type.Equals(Constants.VALID, StringComparison.OrdinalIgnoreCase))
            {
                UpdateCount(tenant_code, validCountType);
            }
            else if (count_type.Equals(Constants.INVALID, StringComparison.OrdinalIgnoreCase))
            {
                HandleInvalidRecords(tenant_code, actionType, recordNumber);
            }
        }
        private void HandleInvalidRecords(string tenant_code, string actionType, int recordNumber)
        {
            // Determine the invalid record list based on action type
            string invalidRecordListName = $"Invalid{actionType}Records";
            var invalidRecordList = typeof(InvalidRecords).GetProperty(invalidRecordListName);
            var invalidRecords = invalidRecordList?.GetValue(_batchJobReportValidationJson.PreRun?.InvalidRecords) as List<int>;

            invalidRecords?.Add(recordNumber);

        }
        private void UpdateCount(string tenant_code, string countType)
        {

            // Update perTenantData counts
            var tenantData = _batchJobReportValidationJson.PreRun?.PerTenantData
                              .FirstOrDefault(x => x.TenantCode == tenant_code);

            if (tenantData != null)
            {
                var countProperty = typeof(Core.Domain.Dtos.PreRunValidationJson.Counts).GetProperty(countType);
                countProperty?.SetValue(tenantData.Counts,
                                       (int)countProperty?.GetValue(tenantData.Counts) + 1);
            }

        }
        public async Task GetCounsumerCount(string tenantcode)
        {
            if (!string.IsNullOrEmpty(tenantcode) && !_perTenantConsumerCount.TenantPreConsumerPreRunData.Where(x => x.TenantCode == tenantcode).Any())
            {
                _perTenantConsumerCount.TenantPreConsumerPreRunData.Add(new PerTenantPreConsumerCountData { TenantCode = tenantcode });
                var currentConsumerCount = await _consumerRepo.FindAsync(x => x.TenantCode == tenantcode && x.DeleteNbr == 0);
                var currentConsumerDeleteCount = await _consumerRepo.FindAsync(x => x.TenantCode == tenantcode && x.DeleteNbr != 0);
                var perTenantConsumerCount = _perTenantConsumerCount.TenantPreConsumerPreRunData.Where(x => x.TenantCode == tenantcode).FirstOrDefault();
                if (currentConsumerCount != null && currentConsumerDeleteCount != null && perTenantConsumerCount != null)
                {
                    perTenantConsumerCount.PerTenantConsumerCount.TotalConsumerCount = currentConsumerCount.Count();
                    perTenantConsumerCount.PerTenantConsumerCount.RemovedConsumerCount = currentConsumerDeleteCount.Count();
                }
            }

        }
        public async Task AddtenantforPostRun(MemberEnrollmentDetailDto[] memberEnrollmentDetailDtos, string actionType)
        {
            foreach (var item in memberEnrollmentDetailDtos.ToList())
            {
                var tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == item.EnrollmentDetail.PartnerCode && x.DeleteNbr == 0);
                if (tenant != null && tenant.TenantCode != null)
                {
                    var hastenant = _batchJobReportValidationJson.PostRun?.PerTenantData?.Where(x => x.TenantCode == tenant.TenantCode).FirstOrDefault();
                    if (hastenant == null)
                    {
                        _batchJobReportValidationJson.PostRun?.PerTenantData.Add(new Core.Domain.Dtos.PostRunValidationJson.TenantData { TenantCode = tenant.TenantCode });
                        hastenant = _batchJobReportValidationJson.PostRun?.PerTenantData?.Where(x => x.TenantCode == tenant.TenantCode).FirstOrDefault();

                    }
                    switch (actionType)
                    {
                        case ActionTypes.InsertDescription:
                            hastenant.Counts.ProcessedAdd += 1;
                            _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.ProcessedAdd += 1;
                            break;
                        case ActionTypes.UpdateDescription:
                            hastenant.Counts.ProcessedUpdate += 1;
                            _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.ProcessedUpdate += 1;

                            break;
                        case ActionTypes.CancelDescription:
                            hastenant.Counts.ProcessedCancel += 1;
                            _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.ProcessedCancel += 1;

                            break;
                        case ActionTypes.DeleteDescription:
                            hastenant.Counts.ProcessedDelete += 1;
                            _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.ProcessedDelete += 1;

                            break;
                    }

                    await GetCounsumerCount(tenant.TenantCode);

                }
            }
        }
        public async Task UpdateCountIfMemberNbrExists(string tenantcode, string action)
        {
            if (actionForPreRunUpdate.ContainsKey(action))
            {
                actionForPreRunUpdate[action](tenantcode);
            }

        }


        public async Task UpdateSuccessCount(string actionType, string tenantcode)
        {
            var tenantData = _batchJobReportValidationJson.PostRun?.PerTenantData.Where(x => x.TenantCode == tenantcode).FirstOrDefault();

            if (!string.IsNullOrEmpty(actionType) && tenantData != null)
            {
                switch (actionType)
                {
                    case ActionTypes.InsertDescription:
                        tenantData.Counts.SuccessfulAdd += 1;
                        _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.SuccessfulAdd += 1;
                        break;
                    case ActionTypes.UpdateDescription:
                        tenantData.Counts.SuccessfulUpdate += 1;
                        _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.SuccessfulUpdate += 1;

                        break;
                    case ActionTypes.CancelDescription:
                        tenantData.Counts.SuccessfulCancel += 1;
                        _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.SuccessfulCancel += 1;

                        break;
                    case ActionTypes.DeleteDescription:
                        tenantData.Counts.SuccessfulDelete += 1;
                        _batchJobReportValidationJson.PostRun.CrossTenantData.Counts.SuccessfulDelete += 1;

                        break;
                }
            }

        }
        public async Task GetCounsumerPostCount(string tenantcode, DateTime UpdatedTs)
        {
            const string _methodName = nameof(GetCounsumerPostCount);

            if (!string.IsNullOrEmpty(tenantcode) && !_perTenantConsumerCount.TenantPreConsumerPostRunData.Where(x => x.TenantCode == tenantcode).Any())
            {
                _perTenantConsumerCount.TenantPreConsumerPostRunData.Add(new PerTenantPostConsumerCountData { TenantCode = tenantcode });
                var currentConsumerCount = await _consumerRepo.FindAsync(x => x.TenantCode == tenantcode && x.DeleteNbr == 0 && x.UpdateTs < UpdatedTs);
                var UpdatedConsumerCount = await _consumerRepo.FindAsync(x => x.TenantCode == tenantcode && x.DeleteNbr == 0 && x.UpdateTs >= UpdatedTs);
                var currentConsumerDeleteCount = await _consumerRepo.FindAsync(x => x.TenantCode == tenantcode && x.DeleteNbr != 0);
                var PerTenantConsumerCount = _perTenantConsumerCount.TenantPreConsumerPostRunData.Where(x => x.TenantCode == tenantcode).FirstOrDefault();

                if (currentConsumerCount != null && currentConsumerDeleteCount != null && PerTenantConsumerCount != null)
                {

                    PerTenantConsumerCount.PerTenantConsumerCount.TotalConsumerCount = currentConsumerCount.Count();
                    PerTenantConsumerCount.PerTenantConsumerCount.RemovedConsumerCount = currentConsumerDeleteCount.Count();
                    PerTenantConsumerCount.PerTenantConsumerCount.UpdateConsumerCount = UpdatedConsumerCount.Count();
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName} -{FileName} Unable to calculate the post consumer count for tenant code {code}"
                   , className, _methodName, _fileName + "_Error", tenantcode);
                }
            }

        }
        public async Task VerifyCounsumerCount(string tenantcode)
        {
            const string _methodName = nameof(VerifyCounsumerCount);

            var PostRunCount = _perTenantConsumerCount.TenantPreConsumerPostRunData.Where(x => x.TenantCode == tenantcode).FirstOrDefault();
            var PreRunCount = _perTenantConsumerCount.TenantPreConsumerPreRunData.Where(x => x.TenantCode == tenantcode).FirstOrDefault();
            var PreRunValidationCount = _batchJobReportValidationJson.PreRun?.PerTenantData.Where(x => x.TenantCode == tenantcode).FirstOrDefault();
            if (PostRunCount != null && PreRunCount != null && PreRunValidationCount != null)
            {
                int AddCount = PreRunCount.PerTenantConsumerCount.TotalConsumerCount + PreRunValidationCount.Counts.ValidAdd - PreRunValidationCount.Counts.ValidCancel;
                int UpdateCount = PreRunValidationCount.Counts.ValidDelete + PreRunValidationCount.Counts.ValidUpdate;
                int CancelCount = PreRunCount.PerTenantConsumerCount.RemovedConsumerCount + PreRunValidationCount.Counts.ValidCancel;
                if (!(AddCount == PostRunCount.PerTenantConsumerCount.TotalConsumerCount))
                {
                    _logger.LogError("{ClassName}.{MethodName} - {FileName} There is missmatch in total consumer Add  count PreRunCount {PreRunCount}, PostRunCount {PostRunCount}"
                    , className, _methodName, _fileName + "_Error", AddCount.ToString(), PostRunCount.PerTenantConsumerCount.TotalConsumerCount.ToString());
                }
                if (!(UpdateCount == PostRunCount.PerTenantConsumerCount.UpdateConsumerCount))
                {
                    _logger.LogError("{ClassName}.{MethodName} - {FileName} There is missmatch in total consumer Update  count PreRunCount {PreRunCount}, PostRunCount {PostRunCount}"
                    , className, _methodName, _fileName + "_Error", UpdateCount.ToString(), PostRunCount.PerTenantConsumerCount.UpdateConsumerCount.ToString());
                }
                if (!(CancelCount == PostRunCount.PerTenantConsumerCount.RemovedConsumerCount))
                {
                    _logger.LogError("{ClassName}.{MethodName} -{FileName} There is missmatch in total consumer Add  count PreRunCount {PreRunCount}, PostRunCount {PostRunCount}"
                    , className, _methodName, _fileName + "_Error", CancelCount.ToString(), PostRunCount.PerTenantConsumerCount.RemovedConsumerCount.ToString());
                }

            }
        }
        public List<MemberImportComprehensiveReportDto> PrepareReportData(string processedFileName)
        {
            const string _methodName = nameof(PrepareReportData);
            var postRunTenant = _batchJobReportValidationJson.PostRun?.PerTenantData;
            var preRunTenant = _batchJobReportValidationJson.PreRun?.PerTenantData;
            List<MemberImportComprehensiveReportDto> reportDtos = new();
            if (preRunTenant != null && postRunTenant != null)
            {
                var validationjsondata = preRunTenant
    .GroupJoin(postRunTenant,
               preTenant => preTenant.TenantCode,
               postTenant => postTenant.TenantCode,
               (preTenant, postGroup) => new
               {
                   TenantCode = preTenant.TenantCode,
                   PreRunData = preTenant,
                   PostRunData = postGroup.FirstOrDefault() // Get the first match or null if no match
               });
                if (validationjsondata != null)
                {
                    foreach (var json in validationjsondata)
                    {
                        foreach (var action in ActionType)
                        {
                            MemberImportComprehensiveReportDto memberImportComprehensiveReportDto = new();
                            memberImportComprehensiveReportDto.FileType = Constants.MemberImportFileType;
                            memberImportComprehensiveReportDto.TenantCode = json.TenantCode;
                            memberImportComprehensiveReportDto.ActionType = action;
                            memberImportComprehensiveReportDto.FileName = processedFileName;

                            switch (action)
                            {
                                case Constants.ADD:
                                    memberImportComprehensiveReportDto.TotalRecordsCount = json.PreRunData.Counts.TotalAdd;
                                    memberImportComprehensiveReportDto.TotalValidRecordsCount = json.PreRunData.Counts.ValidAdd;
                                    memberImportComprehensiveReportDto.InvalidRecordCount = json.PreRunData.Counts.TotalAdd - json.PreRunData.Counts.ValidAdd;
                                    memberImportComprehensiveReportDto.ProcessedRecordsCount = json.PostRunData?.Counts.ProcessedAdd ?? 0;
                                    memberImportComprehensiveReportDto.SuccessfulRecordCount = json.PostRunData?.Counts.SuccessfulAdd ?? 0;
                                    break;
                                case Constants.UPDATE:

                                    memberImportComprehensiveReportDto.TotalRecordsCount = json.PreRunData.Counts.TotalUpdate;
                                    memberImportComprehensiveReportDto.TotalValidRecordsCount = json.PreRunData.Counts.ValidUpdate;
                                    memberImportComprehensiveReportDto.InvalidRecordCount = json.PreRunData.Counts.TotalUpdate - json.PreRunData.Counts.ValidUpdate;
                                    memberImportComprehensiveReportDto.ProcessedRecordsCount = json.PostRunData?.Counts.ProcessedUpdate ?? 0;
                                    memberImportComprehensiveReportDto.SuccessfulRecordCount = json.PostRunData?.Counts.SuccessfulUpdate ?? 0;

                                    break;
                                case Constants.CANCEL:

                                    memberImportComprehensiveReportDto.TotalRecordsCount = json.PreRunData.Counts.TotalCancel;
                                    memberImportComprehensiveReportDto.TotalValidRecordsCount = json.PreRunData.Counts.ValidCancel;
                                    memberImportComprehensiveReportDto.InvalidRecordCount = json.PreRunData.Counts.TotalCancel - json.PreRunData.Counts.ValidCancel;
                                    memberImportComprehensiveReportDto.ProcessedRecordsCount = json.PostRunData?.Counts.ProcessedCancel ?? 0;
                                    memberImportComprehensiveReportDto.SuccessfulRecordCount = json.PostRunData?.Counts.SuccessfulCancel ?? 0;

                                    break;
                                case Constants.DELETE:
                                    memberImportComprehensiveReportDto.TotalRecordsCount = json.PreRunData.Counts.TotalDelete;
                                    memberImportComprehensiveReportDto.TotalValidRecordsCount = json.PreRunData.Counts.ValidDelete;
                                    memberImportComprehensiveReportDto.InvalidRecordCount = json.PreRunData.Counts.TotalDelete - json.PreRunData.Counts.ValidDelete;
                                    memberImportComprehensiveReportDto.ProcessedRecordsCount = json.PostRunData?.Counts.ProcessedDelete ?? 0;
                                    memberImportComprehensiveReportDto.SuccessfulRecordCount = json.PostRunData?.Counts.SuccessfulDelete ?? 0;

                                    break;
                            }
                            reportDtos.Add(memberImportComprehensiveReportDto);
                        }

                    }
                }
                else
                {

                    _logger.LogError("{ClassName}.{MethodName} -{FileName} : Tenant code mismatch between pre {pre}and post run {post} json"
                    , className, _methodName, processedFileName + "_Error", preRunTenant.ToJson(), postRunTenant.ToJson());
                }

            }
            else
            {

                _logger.LogError("{ClassName}.{MethodName} -{FileName} :validation json  pre {pre}and post run {post} not found"
                , className, _methodName, processedFileName + "_Error", preRunTenant?.ToJson(), postRunTenant?.ToJson());
            }
            return reportDtos;
        }
        public async Task<string> CreateAndUploadCsv(string processedFileName)
        {
            const string _methodName = nameof(PrepareReportData);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," };
            // File name prefixed with local Download folder path.
            var fileName = $"Etl_Member_Import_Report_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.csv";
            List<MemberImportComprehensiveReportDto> reportData = PrepareReportData(processedFileName);
            var isUploaded = await _awsS3Service.CreateCsvAndUploadToS3<MemberImportComprehensiveReportDto>(csvConfiguration, reportData, fileName, GetAwsTmpS3BucketName());
            if (isUploaded)
                return Path.Combine(GetAwsTmpS3BucketName(), fileName);
            else
            {
                _logger.LogError("{ClassName}.{MethodName} -{FileName} :An error occured while uploading the file to tmp bucket"
                , className, _methodName, processedFileName + "_Error");
                return string.Empty;
            }
        }

        public async Task<IList<ETLMemberImportFileModel>> GetMemberImportFilesToImport()
        {
           return await _memberImportFileRepo.FindAsync(x => x.DeleteNbr == 0 && x.FileStatus == nameof(FileStatus.NOT_STARTED));
        }

        public async Task<bool> updateFileStatus(long memberImportFileId , FileStatus status)
        {
            var file =  await _memberImportFileRepo.FindOneAsync(x => x.DeleteNbr == 0 && x.MemberImportFileId == memberImportFileId);
            file.FileStatus = status.ToString();
            file.UpdateTs = DateTime.UtcNow;
           var updatedModel =  await _memberImportFileRepo.UpdateAsync(file);
            if(updatedModel!= null && updatedModel.FileStatus == status.ToString())
            {
                return true;
            }
            return false;
        }

        private void PopulateMemberImportFileData(ETLMemberImportFileDataModel memberImportFileData, MemberImportCSVDto consumerCsvDto)
        {
            memberImportFileData.MemberId = consumerCsvDto.member_id;
            memberImportFileData.MemberType = consumerCsvDto.member_type;
            memberImportFileData.LastName = consumerCsvDto.last_name;
            memberImportFileData.FirstName = consumerCsvDto.first_name;
            memberImportFileData.Gender = consumerCsvDto.gender;
            memberImportFileData.Age = consumerCsvDto.age;
            memberImportFileData.Dob = DateTime.TryParseExact(consumerCsvDto.dob, formats, null,DateTimeStyles.None, out var dob) ? dob : null;
            memberImportFileData.Email = consumerCsvDto.email;
            memberImportFileData.City = consumerCsvDto.city;
            memberImportFileData.Country = consumerCsvDto.country;
            memberImportFileData.PostalCode = consumerCsvDto.postal_code;
            memberImportFileData.MobilePhone = consumerCsvDto.mobile_phone;
            memberImportFileData.EmpOrDep = consumerCsvDto.emp_or_dep;
            memberImportFileData.MemNbr = consumerCsvDto.mem_nbr;
            memberImportFileData.SubscriberMemNbr = consumerCsvDto.subscriber_mem_nbr;
            memberImportFileData.EligibilityStart = DateTime.TryParseExact(consumerCsvDto.eligibility_start, formats, null, DateTimeStyles.None, out var eligibility_start) ? eligibility_start : null;
            memberImportFileData.EligibilityEnd = DateTime.TryParseExact(consumerCsvDto.eligibility_end, formats, null, DateTimeStyles.None, out var eligibility_end) ? eligibility_end : null;
            memberImportFileData.MailingAddressLine1 = consumerCsvDto.mailing_address_line1;
            memberImportFileData.MailingAddressLine2 = consumerCsvDto.mailing_address_line2;
            memberImportFileData.MailingState = consumerCsvDto.mailing_state;
            memberImportFileData.MailingCountryCode = consumerCsvDto.mailing_country_code;
            memberImportFileData.HomePhoneNumber = consumerCsvDto.home_phone_number;
            memberImportFileData.Action = consumerCsvDto.action;
            memberImportFileData.PartnerCode = consumerCsvDto.partner_code;
            memberImportFileData.MiddleName = consumerCsvDto.middle_name;
            memberImportFileData.HomeAddressLine1 = consumerCsvDto.home_address_line1;
            memberImportFileData.HomeAddressLine2 = consumerCsvDto.home_address_line2;
            memberImportFileData.HomeState = consumerCsvDto.home_state;
            memberImportFileData.HomeCity = consumerCsvDto.home_city;
            memberImportFileData.HomePostalCode = consumerCsvDto.home_postal_code;
            memberImportFileData.LanguageCode = consumerCsvDto.language_code;
            memberImportFileData.RegionCode = consumerCsvDto.region_code;
            memberImportFileData.SubscriberMemNbrPrefix = consumerCsvDto.subscriber_mem_nbr_prefix;
            memberImportFileData.MemNbrPrefix = consumerCsvDto.mem_nbr_prefix;
            memberImportFileData.PlanId = consumerCsvDto.plan_id;
            memberImportFileData.PlanType = consumerCsvDto.plan_type;
            memberImportFileData.SubgroupId = consumerCsvDto.subgroup_id;
            memberImportFileData.IsSsoUser = consumerCsvDto.is_sso_user;
            memberImportFileData.PersonUniqueIdentifier = consumerCsvDto.person_unique_identifier;
            memberImportFileData.RawDataJson = consumerCsvDto.raw_data_json;
        }

        public MemberImportCSVDto ConvertToConsumerCsvDto(ETLMemberImportFileDataModel memberImportFileData)
        {
            return new MemberImportCSVDto
            {
                member_id = memberImportFileData.MemberId,
                member_type = memberImportFileData.MemberType,
                last_name = memberImportFileData.LastName,
                first_name = memberImportFileData.FirstName,
                gender = memberImportFileData.Gender,
                age = memberImportFileData.Age,
                dob = memberImportFileData.Dob.HasValue?  memberImportFileData.Dob.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) :"",
                email = memberImportFileData.Email,
                city = memberImportFileData.City,
                country = memberImportFileData.Country,
                postal_code = memberImportFileData.PostalCode,
                mobile_phone = memberImportFileData.MobilePhone,
                emp_or_dep = memberImportFileData.EmpOrDep,
                mem_nbr = memberImportFileData.MemNbr,
                subscriber_mem_nbr = memberImportFileData.SubscriberMemNbr,
                eligibility_start = memberImportFileData.EligibilityStart.HasValue ? memberImportFileData.EligibilityStart.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : "",
                eligibility_end = memberImportFileData.EligibilityEnd.HasValue ? memberImportFileData.EligibilityEnd.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : "",
                mailing_address_line1 = memberImportFileData.MailingAddressLine1,
                mailing_address_line2 = memberImportFileData.MailingAddressLine2,
                mailing_state = memberImportFileData.MailingState,
                mailing_country_code = memberImportFileData.MailingCountryCode,
                home_phone_number = memberImportFileData.HomePhoneNumber,
                action = memberImportFileData.Action,
                partner_code = memberImportFileData.PartnerCode,
                middle_name = memberImportFileData.MiddleName,
                home_address_line1 = memberImportFileData.HomeAddressLine1,
                home_address_line2 = memberImportFileData.HomeAddressLine2,
                home_state = memberImportFileData.HomeState,
                home_city = memberImportFileData.HomeCity,
                home_postal_code = memberImportFileData.HomePostalCode,
                language_code = memberImportFileData.LanguageCode,
                region_code = memberImportFileData.RegionCode,
                subscriber_mem_nbr_prefix = memberImportFileData.SubscriberMemNbrPrefix,
                mem_nbr_prefix = memberImportFileData.MemNbrPrefix,
                plan_id = memberImportFileData.PlanId,
                plan_type = memberImportFileData.PlanType,
                subgroup_id = memberImportFileData.SubgroupId,
                is_sso_user = memberImportFileData.IsSsoUser,
                person_unique_identifier = memberImportFileData.PersonUniqueIdentifier,
                member_import_file_data_id = memberImportFileData.MemberImportFileDataId,
                raw_data_json = memberImportFileData.RawDataJson
            };
        }

    }
}
