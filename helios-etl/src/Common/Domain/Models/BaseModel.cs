namespace SunnyRewards.Helios.ETL.Common.Domain.Models
{
    public class BaseModel
    {
        /// <summary>
        /// this will be mapped to primary key of a table
        /// </summary>
        public virtual int Id { get; set; }
        public virtual DateTime CreateTs { get; set; }
        public virtual DateTime UpdateTs { get; set; }
        public virtual string? CreateUser { get; set; }
        public virtual string? UpdateUser { get; set; }
        public virtual long DeleteNbr { get; set; }
    }
}
