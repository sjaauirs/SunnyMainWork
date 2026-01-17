namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{ 
    /// <summary>
    /// Class to associate File Record Number with an error Message
    /// </summary>
    public class RecordError
    {
        public RecordError(int recordNbr)
        {
            RecordNbr = recordNbr;    
        }
        public int RecordNbr { get; set; }
        public string ErrorMessage { get; set; } = String.Empty;   
    }
}
