using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberEnrollmentRequestDto
    {
        /// <summary>
        /// Array of Members to be added/updated in the Rewards System
        /// </summary>
        public MemberEnrollmentDetailDto[] Members { get; set; } = new MemberEnrollmentDetailDto[0];
    }
}
