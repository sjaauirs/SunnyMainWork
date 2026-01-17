using Microsoft.Extensions.Hosting;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    /// <summary>
    /// Parameters required for executing ETL
    /// </summary>
    public class EtlExecutionContext
    {
        public IHost? Host { get; set; }

        public string TenantCode { get; set; } = string.Empty;

        public int StartIndex { get; set; }

        public int MaxEnrollments { get; set; }

        public string PldFilePath { get; set; } = string.Empty;

        public bool EnableEnrollment { get; set; }

        public bool EnablePldProcessing { get; set; }

        public bool EnableCohorting { get; set; }

        public string TaskUpdateFilePath { get; set; } = string.Empty;

        public string CustomFormat { get; set; } = string.Empty;

        public bool EnableS3 { get; set;} = false;

        public string ScanS3FileTypes { get; set; } = string.Empty;

        public string MemberFilePath { get; set; } = string.Empty;

        public string EnrollmentFilePath { get; set; } = string.Empty;

        public bool EnableMemberLoad { get; set; } = false;
        public bool EnableMemberImport { get; set; } = false;
        public bool ProcessRecurringTasks { get; set; } = false;
        public bool ClearWalletEntries { get; set; } = false;
        public bool RedeemHSA { get; set; } = false;
        public bool FISCreateCards { get; set; } = false;
        public bool FIS30RecordFileLoad { get; set; } = false;
        public bool FIS60RecordFileLoad { get; set; } = false;
        public bool ProcessRetailProductSyncQueue { get; set; } = false;
        public bool ExecuteBenefitsFunding { get; set; } = false;
        public bool IsSubmitCard60Job { get; set; } = false;

        public byte[] MemberFileContents { get; set; } = Array.Empty<byte>();
        public byte[] EnrolmentFileContents { get; set; } = Array.Empty<byte>();
        
        public string EventingType { get; set; } = string.Empty;

        public byte[] MemberImportFileContents { get; set; } = Array.Empty<byte>();
        public byte[] TriviaImportFileContents { get; set; } = Array.Empty<byte>();
        public byte[] QuestionnaireImportFileContents { get; set; } = Array.Empty<byte>();
        public byte[] DepositIntructionEligibleConsumersFileContents { get; set; } = Array.Empty<byte>();
        public byte[] RedeemConsumerListFileContents { get; set; } = Array.Empty<byte>();
        public byte[] TaskImportFileContents { get; set; } = Array.Empty<byte>();

        public string TaskImportFilePath { get; set; } = string.Empty;
        public string TriviaImportFilePath { get; set; } = string.Empty;
        public string QuestionnaireImportFilePath { get; set; } = string.Empty;
        public string DepositInstructionFilePath { get; set; } = string.Empty;
        public string MemberImportFilePath { get; set; } = string.Empty;
        public string RedeemConsumerListFilePath { get; set; } = string.Empty;

        public bool EnableHealthMetricProcessing { get; set; } = false;
        public bool GenerateCardLoad { get; set; } = false;
        public string FISRecordFileName { get; set; } = string.Empty;
        public bool ProcessMonetaryTransactionsBatchFile { get; set; } = false;
        public string FISMonetaryTransactionsFileName { get; set; } = string.Empty;
        public bool ConsumerNonMonetaryTransactionsBatchFile { get; set; } = false;
        public string ConsumerNonMonetaryTransactionsFileName { get; set; } = string.Empty;
        public bool PerformExternalTxnSync { get; set; } = false;
        public int TaskId { get; set; }
        public string RollupPeriodTypeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public bool ProcessHealthTask { get; set; } = false;
        public string LocalDownloadFolderPath { get; set; } = string.Empty;
        public int BatchSize { get; set; }
        public bool ExecuteCohorting { get; set; } = false;
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerLabel { get; set; } = string.Empty;
        public bool IsEncryptAndCopy { get; set; } = false;
        public bool IsCreateDuplicateConsumer { get; set; } = false;
        public string FISEncryptAndCopyFileName { get; set; } = string.Empty;
        public bool DonotEncryptAndCopy { get; set; } = false;
        public string WalletTypeCode { get; set; } = string.Empty;
        public bool GenerateWalletBalancesReport { get; set; } = false;
        public bool IsUpdateUserInfoInFIS { get; set; } = false;
        public bool ConsumeSweepstakesWinnerReport { get; set; } = false;
        public long SweepstakesInstanceId { get; set; }
        public string Format { get; set; } = string.Empty;
        public string CutoffDate { get; set; } = string.Empty;
        public string CutoffTz { get; set; } = string.Empty;
        public bool GenerateSweepstakesEntriesReport { get; set; } = false;
        public bool DeleteIneligibleConsumers { get; set; } = false;
        public bool SubscriberOnly { get; set;} = false;
        public string BatchOperationGroupCode {  get; set; } = string.Empty;
        public string BatchActionType { get; set; } = String.Empty;
        public String ConsumerCode { get; set; } =string.Empty;
        public String NewEmail { get; set; } =string.Empty;
        public bool ExecuteRestoreCostcoBackup { get; set; } = false;
        public long MinEpochTs { get; set; }
        public long MaxEpochTs { get; set; }

        public string CohortConsumerImportFilePath { get; set; } = string.Empty;
        public string IncomingFilePath { get; set; } = string.Empty;
        public string IncomingBucketName { get; set; } = string.Empty;
        public string PublicfolderBucketName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string OutboundFilePath { get; set; } = string.Empty;
        public string OutboundBucketName { get; set; } = string.Empty;
        public string OutboundFileNamePattern { get; set; } = string.Empty;
        public bool FileCryptoProcessor { get; set; } = false;
        public string ArchiveFilePath { get; set; } = string.Empty;
        public string ArchiveBucketName { get; set; } = string.Empty;
        public byte[] CohortConsumerImportFileContents { get; set; } = Array.Empty<byte>();
        public string ConsumerListFile { get; set; } = string.Empty;
        public DateTime ETLStartTs { get; set; } = DateTime.Now;
        public string JobHistoryId { get; set; } = string.Empty;
        public string JobHistoryStatus { get; set; } = string.Empty;
        public string JobHistoryErrorLog { get; set; } = string.Empty;
        public string SyncTenantConfigOptions { get; set; } = string.Empty;
        public string ConsumerCodes { get; set; } = string.Empty;
        public bool ProcessNotificationRules { get; set; } = false;

        public bool ExtractCompletedConsumerTask { get; set; } = false;
        public bool TransferRedshiftToPostgres { get; set; } = false;
        public string SyncDataType { get; set; } = string.Empty;
        public string StartDate { get; set; }
        public string? EndDate { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string DateRangeStart { get; set; } = string.Empty;
        public string DateRangeEnd { get; set; } = string.Empty;
        public string Delimiter { get; set; } = string.Empty;
        public bool ShouldEncrypt { get; set; } = false;
        public bool RedshiftToSftp { get; set; } = false;
        public string DateFormat { get; set; } = string.Empty;
        public bool ShouldAddSourceFileName { get; set; } = false;
        public bool RemoveFooter { get; set; } = false;
        public bool HasHeader { get; set; } = true;
        public string CohortListFile { get; set; } = string.Empty;
        public string RedshiftDatabaseName { get; set; } = string.Empty;
        public bool EnableSftp { get; set; } = false;
        public string CohortCode { get; set; } = string.Empty;
        public bool ExecuteEventing { get; set; } = false;
        public bool ShouldMarkFileAsCompleted { get; set; } = false;
        public bool ShouldAppendTotalRowCount { get; set; } = false;
        public string PartnerCode { get; set; } = string.Empty;
        public int MessagingGroupCount { get; set; }
    }
}