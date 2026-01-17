namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class RedShiftMemberImportFileDataDto
    {
        public long MemberImportFileDataId { get; set; }
        public long MemberImportFileId { get; set; }
        public int RecordNumber { get; set; }
        public string? RawDataJson { get; set; }
        public string? FileName { get; set; }
        public DateTime CreateTs { get; set; }
        public DateTime? UpdateTs { get; set; }
        public string? CreateUser { get; set; }
        public string? UpdateUser { get; set; }
        public long DeleteNbr { get; set; }
        public  string? Age { get; set; }
        public  DateTime? Dob { get; set; }  
        public  string? City { get; set; }
        public  string? Email { get; set; }
        public  string? Action { get; set; }
        public  string? Gender { get; set; }
        public  string? Country { get; set; }
        public  string? MemNbr { get; set; }
        public  string? PlanId { get; set; }
        public  string? HomeCity { get; set; }
        public  string? LastName { get; set; }
        public  string? MemberId { get; set; }
        public  string? PlanType { get; set; }
        public  string? EmpOrDep { get; set; }
        public  string? FirstName { get; set; }
        public  string? HomeState { get; set; }
        public  bool? IsSsoUser { get; set; }
        public  string? MemberType { get; set; }
        public  string? MiddleName { get; set; }
        public  string? PostalCode { get; set; }
        public  string? RegionCode { get; set; }
        public  string? SubgroupId { get; set; }
        public  string? MobilePhone { get; set; }
        public  string? PartnerCode { get; set; }
        public  string? LanguageCode { get; set; }
        public  string? MailingState { get; set; }
        public  string? MemNbrPrefix { get; set; }
        public  DateTime? EligibilityEnd { get; set; }  
        public  string? HomePostalCode { get; set; }
        public  DateTime? EligibilityStart { get; set; }  
        public  string? HomePhoneNumber { get; set; }
        public  string? HomeAddressLine1 { get; set; }
        public  string? HomeAddressLine2 { get; set; }
        public  string? SubscriberMemNbr { get; set; }
        public  string? MailingCountryCode { get; set; }
        public  string? MailingAddressLine1 { get; set; }
        public  string? MailingAddressLine2 { get; set; }
        public  string? PersonUniqueIdentifier { get; set; }
        public  string? SubscriberMemNbrPrefix { get; set; }
        public string? PublishStatus { get; set; }
        public string? PublishingLockId { get; set; }
        public DateTime? PublishingLockTs { get; set; }
        public int PublishAttempts { get; set; }
    }
}
