using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class MemberModel : BaseModel
    {
        public virtual string? MemNbr { get; set; }
        public virtual string? FirstName { get; set; }
        public virtual string? LastName { get; set; }
        public virtual char? MiddleName { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }
        public virtual DateTime? DeceasedDate { get; set; }
        public virtual char? Gender { get; set; }
        public virtual string? Address1 { get; set; }
        public virtual string? Address2 { get; set; }
        public virtual string? Address3 { get; set; }
        public virtual string? City { get; set; }
        public virtual char? State { get; set; }
        public virtual string? ZipCode { get; set; }
        public virtual string? Country { get; set; }
        public virtual string? PhoneNumber { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? SocialSecurityNumber { get; set; }
        public virtual char? HispOrigin { get; set; }
        public virtual char? NeedInterpreter { get; set; }
        public virtual char? RaceCode1 { get; set; }
        public virtual char? RaceCode2 { get; set; }
        public virtual char? RaceCode3 { get; set; }
        public virtual string? HicNumber { get; set; }
        public virtual string?  MbiNumber { get; set; }
        public virtual string?  MrnNumber { get; set; }
        public virtual string?  guardLastName { get; set; }
        public virtual string?  guardFirstName { get; set; }
        public virtual char?  guardMiddleName { get; set; }
        public virtual char?  guardEmail { get; set; }
        public virtual int?  SpokenLanguageSource { get; set; }
        public virtual int?  WrittenLanguageSource { get; set; }
        public virtual int?  OtherLanguageSource { get; set; }
        public virtual int? SpokenLanguageId { get; set; }
        public virtual int? WrittenLanguageId { get; set; }
        public virtual int? OtherLanguageId { get; set; }
        public virtual int? RaceSource { get; set; }
        public virtual int? EthnicitySource { get; set; }

        //public virtual DateTime? Birthdate { get; set; }
                //public virtual string? EmployeeId { get; set; }
        //public virtual string? CoverageIndicator { get; set; }
        //public virtual string? PbpNbr { get; set; }
        //public virtual string? SnpType { get; set; }
        //public virtual string? AmpNbr { get; set; }
        //public virtual string? CinNumber { get; set; }
        //public virtual int? ProductId { get; set; }
        //public virtual int? ProductId2 { get; set; }
        //public virtual int? MedEligCatId { get; set; }
        //public virtual int? YearOfBirth { get; set; }
        //public virtual int? YearOfDeath { get; set; }
        //public virtual int? MonthOfDeath { get; set; }
        //public virtual int? DayOfDeath { get; set; }
    }
}
