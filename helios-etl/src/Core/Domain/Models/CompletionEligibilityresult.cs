namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class CompletionEligibilityresult
    {
        public bool IsTaskCompletionValid { get; set; }
      
        public DateTime? CompletionDate { get; set; }
        public bool SkipValidation { get; set; } = false;
    }

}