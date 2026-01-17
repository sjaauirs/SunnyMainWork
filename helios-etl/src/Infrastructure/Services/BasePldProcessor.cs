using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class BasePldProcessor
    {
        private readonly ILogger _logger;
        private readonly ISession _session;
        private readonly IPldParser _pldParser;
        private readonly IS3FileLogger _s3FileLogger;
        private const string className = nameof(BasePldProcessor);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        public BasePldProcessor(ILogger logger, ISession session, IPldParser pldParser, IS3FileLogger s3FileLogger)
        {
            _logger = logger;
            _session = session;
            _pldParser = pldParser;
            _s3FileLogger = s3FileLogger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="pldFilePath"></param>
        /// <returns></returns>
        protected async Task<List<ETLConsumerModel>> ProcessConsumerAttrUsingPldFile(string tenantCode, string pldFilePath = "",
            byte[]? pldFileContent = null)
        {
            const string methodName = nameof(ProcessConsumerAttrUsingPldFile);
            List<ETLConsumerModel> pldConsumers = new();
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing with PldFilePath={PldFilePath},TenantCode:{Code}", className, methodName, pldFilePath, tenantCode);

                List<PldRecordDto> pldData = ParsePldFile(pldFilePath, pldFileContent);

                if (pldData != null && pldData.Count > 0)
                {
                    Dictionary<string, PldRecordDto> memberPldData = new();
                    foreach (var pldRecord in pldData)
                    {
                        if (pldRecord.PldFieldData == null)
                        {
                            _logger.LogWarning("{ClassName}.{MethodName} - PldFieldData is null for PldRecord:{Record}", className, methodName, pldRecord);
                            continue;
                        }

                        if (!pldRecord.PldFieldData.ContainsKey("mbi_a_ben_s_med_ben_ide"))
                        {
                            _logger.LogWarning("{ClassName}.{MethodName} - mbi_a_ben_s_med_ben_ide key not found in PldRecord:{Record}", className, methodName, pldRecord);
                            continue;
                        }

                        string memNbr = pldRecord.PldFieldData["mbi_a_ben_s_med_ben_ide"];
                        if (!memberPldData.ContainsKey(memNbr)) { memberPldData[memNbr] = pldRecord; }

                        var consumer = await UpdateMember(tenantCode, memNbr, pldRecord);
                        if (consumer != null)
                        {
                            pldConsumers.Add(consumer);
                        }
                    }

                    if (memberPldData.Count <= 0)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName} - memberPldData has no records", className, methodName);
                    }
                }

                return pldConsumers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing ProcessConsumerAttrUsingPldFile for TenantCode:{Code},Pldfilepath:{Path} ", className, methodName, tenantCode, pldFilePath);

                // this log using for s3FileLogger
                await _s3FileLogger.AddErrorLogs(new S3LogContext()
                {
                    Message = ex.Message,
                    TenantCode = tenantCode,
                    Ex = ex
                });
                return pldConsumers;
            }
            finally
            {
                _logger.LogInformation($"ProcessConsumerAttrUsingPldFile : Exit");
            }
        }

        private async Task<ETLConsumerModel?> UpdateMember(string tenantCode, string memNbr, PldRecordDto pldRecord)
        {
            const string methodName = nameof(UpdateMember);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing update member for Memnbr:{Nbr},TenantCode:{Code}", className, methodName, memNbr, tenantCode);
            using var consumerTransaction = _session.BeginTransaction();
            try
            {
                var consumer = _session.Query<ETLConsumerModel>().Where(x => x.TenantCode == tenantCode &&
                    x.MemberNbr == memNbr && x.DeleteNbr == 0).FirstOrDefault();

                if (consumer != null)
                {
                    var consumerAttr = consumer.Attr ?? new JObject();
                    if (consumerAttr.ContainsKey("pld"))
                        consumerAttr.Remove("pld");

                    JToken jToken = JToken.FromObject(pldRecord.PldFieldData);

                    consumerAttr.Add("pld", jToken);
                    consumer.ConsumerAttribute = consumerAttr.ToJson();

                    await _session.UpdateAsync(consumer);
                    await consumerTransaction.CommitAsync();

                    _logger.LogInformation("{ClassName}.{MethodName} - Cosnumer updated member for Memnbr:{Nbr},TenantCode:{Code}", className, methodName, memNbr, tenantCode);
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName} - Consumer not found member for Memnbr:{Nbr},TenantCode:{Code}", className, methodName, memNbr, tenantCode);
                }

                return consumer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while updating member for Memnbr:{Nbr},TenantCode:{Code},ErrorCode:{Code},ERROR:{Msg}",
                    className, methodName, memNbr, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                await consumerTransaction.RollbackAsync();

                // this log using for s3FileLogger
                await _s3FileLogger.AddErrorLogs(new S3LogContext()
                {
                    Message = ex.Message,
                    TenantCode = tenantCode,
                    MemberNbr = memNbr,
                    Ex = ex
                });
                return null;
            }
        }

        /// <summary>
        /// Uses IPldParse to parse each line of the input PLD file and returns records
        /// </summary>
        /// <param name="pldFilePath"></param>
        /// <returns></returns>
        private List<PldRecordDto> ParsePldFile(string pldFilePath, byte[]? pldFileContent)
        {
            const string methodName = nameof(ParsePldFile);
            _logger.LogInformation("{ClassName}.{MethodName} - Loading PLD data from file: {Pld}", className, methodName, pldFilePath);

            List<PldRecordDto> pldData = new();
            StreamReader? reader = null;
            try
            {
                reader = pldFileContent?.Length > 0 ? new StreamReader(new MemoryStream(pldFileContent)) : new StreamReader(pldFilePath);

                int lineNum = 0;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var pldRec = _pldParser.ParsePldLine(line.Trim());
                    if (pldRec != null)
                    {
                        pldData.Add(pldRec);
                    }
                    else
                    {
                        _logger.LogWarning("{ClassName}.{MethodName} - Ignoring PLD data null for line#: {Line}", className, methodName, lineNum);
                    }
                    lineNum++;
                }
                reader?.Dispose();
            }
            catch (Exception ex)
            {
                reader?.Dispose();
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error processing PLD file: {Pld}, ErrorCode:{Code},ERROR: {Msg}", className, methodName, StatusCodes.Status500InternalServerError, pldFilePath, ex.Message);
            }

            return pldData;
        }
    }
}
