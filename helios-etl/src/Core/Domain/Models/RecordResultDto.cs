namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    /// <summary>
    /// Represents the details of an ETL error.
    /// </summary>
    public class EtlErrorDetailRecord
    {
        public EtlErrorDetailRecord(int code, string Message, string Trace)
        {
            ErrorCode = code;
            ErrorMessage = Message;
            StackTrace = Trace;
        }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }

    /// <summary>
    /// Represents a detailed record for job reporting,
    /// including file and record numbers and potential error details.
    /// </summary>
    public class JobReportDetailRecord
    {
        public JobReportDetailRecord(int fileNbr , int recordNbr)
        {
            FileNbr = fileNbr;
            RecordNbr = recordNbr;
        }
        public int FileNbr { get; set; } = 0;
        public int RecordNbr { get; set; } = 0;
        public EtlErrorDetailRecord? ErrorDetails { get; set; } 

    }
}
