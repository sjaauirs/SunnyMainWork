namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class EtlTaskUpdateCsvRecordDto
    {
        public string Client_ID { get; set; } = string.Empty;
        public string Task_ID { get; set; } = string.Empty;
        public string User_ID { get; set; } = string.Empty;
        public bool Completion { get; set; }
        public string Task_Name { get; set; } = string.Empty;

        public string PartnerCode { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EtlTaskUpdateDto ToTaskUpdateDto(string environment)
        {
            return new EtlTaskUpdateDto()
            {
                MemberId = this.User_ID,
                PartnerCode = this.Client_ID,
                TaskStatus = this.Completion ? "COMPLETED" : "",
                TaskCode = this.Task_ID,
                Environment = environment,
                TaskName = this.Task_Name                
            };
        }
    }
}
