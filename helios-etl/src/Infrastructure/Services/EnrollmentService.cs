using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class EnrollmentService : BasePldProcessor, IEnrollmentService
    {
        private readonly ILogger<EnrollmentService> _logger;
        private readonly NHibernate.ISession _session;
        private readonly ITenantRepo _tenantRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IPersonRepo _personRepo;
        private readonly IPldParser _pldParser;
        private readonly IAwsS3Service _awsS3Service;
        private readonly IS3FileLogger _s3FileLogger;
        private readonly IDataFeedClient _dataFeedClient;
        private const string className=nameof(EnrollmentService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        /// <param name="tenantRepo"></param>
        /// <param name="consumerRepo"></param>
        /// <param name="personRepo"></param>
        /// <param name="pldParser"></param>
        /// <param name="awsS3Service"></param>
        /// <param name="s3FileLogger"></param>
        /// <param name="dataFeedClient"></param>
        public EnrollmentService(
            ILogger<EnrollmentService> logger,
            NHibernate.ISession session,
            ITenantRepo tenantRepo,
            IConsumerRepo consumerRepo,
            IPersonRepo personRepo,
            IPldParser pldParser, IAwsS3Service awsS3Service, IS3FileLogger s3FileLogger
            , IDataFeedClient dataFeedClient) : base(logger, session, pldParser, s3FileLogger)
        {
            _logger = logger;
            _session = session;
            _tenantRepo = tenantRepo;
            _consumerRepo = consumerRepo;
            _personRepo = personRepo;
            _pldParser = pldParser;
            _awsS3Service = awsS3Service;
            _s3FileLogger = s3FileLogger;
            _dataFeedClient = dataFeedClient;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberFileRecs"></param>
        /// <param name="enrollmentFileRecs"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private MemberEnrollmentDetailDto[] CreateMemberDtos(List<MemberCsvDto> memberFileRecs, List<EnrollmentCsvDto> enrollmentFileRecs, ETLTenantModel tenant, bool subscriberOnly = false)
        {
            var memberDtos = new List<MemberEnrollmentDetailDto>();

            foreach (var memberRec in memberFileRecs)
            {
                var enrollmentRec = enrollmentFileRecs.FirstOrDefault(e => e.mem_nbr == memberRec.mem_nbr);

                if (enrollmentRec == null)
                {
                    continue; // invalid rec
                }

                var memberDto = new MemberEnrollmentDetailDto
                {
                    MemberDetail = new MemberDetailDto
                    {
                        FirstName = memberRec.mem_fname,
                        LastName = memberRec.mem_lname,
                        LanguageCode = "en-US",
                        MemberSince = enrollmentRec.enr_start,
                        Email = memberRec.mem_email,
                        City = memberRec.mem_city,
                        Country = memberRec.mem_county ?? "US",
                        PostalCode = memberRec.mem_zip,
                        PhoneNumber = memberRec.mem_phone,
                        Region = " ",
                        Dob = memberRec.mem_dob,
                        Gender = memberRec.mem_gender == "M" ? "MALE" :
                                 memberRec.mem_gender == "F" ? "FEMALE" :
                                 memberRec.mem_gender == "O" ? "OTHER" : null,
                        MailingAddressLine1 = memberRec.mem_addr1,
                        MailingAddressLine2 = memberRec.mem_addr2,
                        MailingState = memberRec.mem_state,
                        MailingCountryCode = memberRec.mem_county_code,
                        HomePhoneNumber = memberRec.mem_phone,
                    },
                    EnrollmentDetail = new EnrollmentDetailDto
                    {
                        PartnerCode = tenant.PartnerCode, // You need to replace "Tenant.PartnerCode" with the actual value
                        MemberNbr = memberRec.mem_nbr,
                        SubscriberMemberNbr = enrollmentRec.enr_subscriber_num,
                        RegistrationTs = enrollmentRec.enr_start,
                        EligibleStartTs = enrollmentRec.enr_start,
                        EligibleEndTs = enrollmentRec.enr_end,
                        SubscriberOnly = subscriberOnly
                    }
                };

                memberDtos.Add(memberDto);
            }


            return memberDtos.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> ProcessTenantEnrollments(EtlExecutionContext etlExecutionContext)
        {
            const string methodName=nameof(ProcessTenantEnrollments);
            var tenantCode = etlExecutionContext.TenantCode;

            if (string.IsNullOrEmpty(tenantCode))
            {
                _logger.LogWarning("{ClassName}.{MethodName} - no tenant code supplied", className, methodName);
                throw new ETLException(ETLExceptionCodes.NullValue, "Tenant code not supplied");
            }

            string memberFilePath = etlExecutionContext.MemberFilePath;
            string enrollmentFilePath = etlExecutionContext.EnrollmentFilePath;

            var memberFileContents = etlExecutionContext.MemberFileContents;
            var enrolmentFileContents = etlExecutionContext.EnrolmentFileContents;

            if (string.IsNullOrEmpty(memberFilePath) && string.IsNullOrEmpty(enrollmentFilePath)
                && memberFileContents.Length <= 0 && enrolmentFileContents.Length <= 0)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - member and enrollment file paths or S3 file content needs to be provided", className, methodName);
                throw new ETLException(ETLExceptionCodes.NullValue, "Member and enrollment file paths or S3 file content needs to be provided");
            }

            var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenant == null)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - Invalid tenant code supplied ,TenantCode:{Code}", className, methodName,tenantCode);
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code supplied ,TenantCode: {tenantCode}");
            }

            var memberFileReader = memberFileContents?.Length > 0
                ? new StreamReader(new MemoryStream(memberFileContents))
                : new StreamReader(memberFilePath);

            var enrolmentFileReader = enrolmentFileContents?.Length > 0
                ? new StreamReader(new MemoryStream(enrolmentFileContents))
                : new StreamReader(enrollmentFilePath);

            using var memberCsv = new CsvReader(memberFileReader, new CsvConfiguration(CultureInfo.InvariantCulture));
            using var enrollMentCsv = new CsvReader(enrolmentFileReader, new CsvConfiguration(CultureInfo.InvariantCulture));

            var memberCsvDtoList = new List<MemberCsvDto>();
            var enrCsvDtoList = new List<EnrollmentCsvDto>();

            var batchSize = 100;
            var totalRecordsProcessed = 0;

            var etlConsumers = new List<ETLConsumerModel>();
            var etlPersons = new List<ETLPersonModel>();

            while (await memberCsv.ReadAsync() && await enrollMentCsv.ReadAsync())
            {
                var memberCsvDto = memberCsv.GetRecord<MemberCsvDto>();
                if (memberCsvDto != null)
                {
                    memberCsvDtoList.Add(memberCsvDto);
                    totalRecordsProcessed++;
                }

                var enrCsvDto = enrollMentCsv.GetRecord<EnrollmentCsvDto>();
                if (enrCsvDto != null)
                {
                    enrCsvDtoList.Add(enrCsvDto);
                    totalRecordsProcessed++;
                }

                // Check if either of the lists has reached the batch size
                if (memberCsvDtoList.Count >= batchSize || enrCsvDtoList.Count >= batchSize)
                {
                    // Process the current batch for both CSVs
                    (var consumers, var persons) = await ProcessBatchAsync(memberCsvDtoList, enrCsvDtoList, tenant, etlExecutionContext);

                    if (consumers?.Count > 0)
                        etlConsumers.AddRange(consumers);

                    if (persons?.Count > 0)
                        etlPersons.AddRange(persons);

                    // Clear the lists for the next batch
                    memberCsvDtoList.Clear();
                    enrCsvDtoList.Clear();
                }
            }

            // Process the remaining records (if any)
            if (memberCsvDtoList.Count > 0 || enrCsvDtoList.Count > 0)
            {
                // Process the current batch for both CSVs
                (var consumers, var persons) = await ProcessBatchAsync(memberCsvDtoList, enrCsvDtoList, tenant, etlExecutionContext);

                if (consumers?.Count > 0)
                    etlConsumers.AddRange(consumers);

                if (persons?.Count > 0)
                    etlPersons.AddRange(persons);
            }

            return (etlConsumers, etlPersons);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberCsvDtoList"></param>
        /// <param name="enrCsvDtoList"></param>
        /// <param name="tenant"></param>
        /// <param name="etlExecutionContext"></param>
        /// <param name="pldFilePath"></param>
        /// <returns></returns>
        private async Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> ProcessBatchAsync(List<MemberCsvDto> memberCsvDtoList,
            List<EnrollmentCsvDto> enrCsvDtoList, ETLTenantModel tenant, EtlExecutionContext etlExecutionContext)
        {

            var memberEnrDetailList = CreateMemberDtos(memberCsvDtoList, enrCsvDtoList, tenant, etlExecutionContext.SubscriberOnly);

            var etlConsumers = new List<ETLConsumerModel>();
            var etlPersons = new List<ETLPersonModel>();

            if (memberEnrDetailList != null)
            {
                var authHeaders = new Dictionary<string, string>
                {
                    { "X-API-KEY", tenant.ApiKey }
                };

                _logger.LogInformation("{ClassName}.ProcessbatchAsync - Invoking datafeed api to create members in ETL with MemberNumbers:{Numbers}",className,memberEnrDetailList.Select(e=>e.EnrollmentDetail.MemberNbr).ToList());  

                var membersResponseDto = await _dataFeedClient.Post<MembersResponseDto>("data-feed/members", new MemberEnrollmentRequestDto()
                {
                    Members = memberEnrDetailList
                }, authHeaders);

                if (membersResponseDto.Consumers == null || membersResponseDto.Consumers.Count == 0)
                    _logger.LogError("{ClassName}.ProcessbatchAsync - Consumers are not created for MemberNumbers:{Numbers},ErrorsList:{Errors}", className, memberEnrDetailList.Select(e => e.EnrollmentDetail.MemberNbr).ToList(),membersResponseDto.ExtendedErrors);

                return (etlConsumers, etlPersons);

                // execute PLD processing if enabled
                if (etlExecutionContext.EnablePldProcessing && !string.IsNullOrEmpty(etlExecutionContext.PldFilePath))
                {
                    // Process consumer with attrs using pld file
                    List<ETLConsumerModel> pldConsumers = await ProcessConsumerAttrUsingPldFile(tenant.TenantCode, etlExecutionContext.PldFilePath);

                    // add consumers to main list for processing cohorts if not already present in list
                    var loadedConsumerCodes = membersResponseDto.Consumers.Select(x => x.Consumer.ConsumerCode).ToList();
                    foreach (var pldConsumer in pldConsumers)
                    {
                        if (!loadedConsumerCodes.Contains(pldConsumer.ConsumerCode))
                        {
                            var person = _personRepo.FindOneAsync(x => x.PersonId == pldConsumer.PersonId).Result;
                            if (person != null)
                            {
                                etlPersons.Add(person);
                                etlConsumers.Add(pldConsumer);
                            }
                        }
                    }
                }
            }
            return (etlConsumers, etlPersons);
        }
    }
}
