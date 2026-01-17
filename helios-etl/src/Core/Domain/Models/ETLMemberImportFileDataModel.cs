using SunnyRewards.Helios.ETL.Common.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLMemberImportFileDataModel : BaseModel
    {
        public virtual long MemberImportFileDataId { get; set; }
        public virtual long MemberImportFileId { get; set; }
        public virtual int RecordNumber { get; set; }
        public virtual string? RawDataJson { get; set; }
        public virtual string? Age { get; set; }
        public virtual DateTime? Dob { get; set; }  
        public virtual string? City { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? Action { get; set; }
        public virtual string? Gender { get; set; }
        public virtual string? Country { get; set; }
        public virtual string? MemNbr { get; set; }
        public virtual string? PlanId { get; set; }
        public virtual string? HomeCity { get; set; }
        public virtual string? LastName { get; set; }
        public virtual string? MemberId { get; set; }
        public virtual string? PlanType { get; set; }
        public virtual string? EmpOrDep { get; set; }
        public virtual string? FirstName { get; set; }
        public virtual string? HomeState { get; set; }
        public virtual bool? IsSsoUser { get; set; }
        public virtual string? MemberType { get; set; }
        public virtual string? MiddleName { get; set; }
        public virtual string? PostalCode { get; set; }
        public virtual string? RegionCode { get; set; }
        public virtual string? SubgroupId { get; set; }
        public virtual string? MobilePhone { get; set; }
        public virtual string? PartnerCode { get; set; }
        public virtual string? LanguageCode { get; set; }
        public virtual string? MailingState { get; set; }
        public virtual string? MemNbrPrefix { get; set; }
        public virtual DateTime? EligibilityEnd { get; set; }  
        public virtual string? HomePostalCode { get; set; }
        public virtual DateTime? EligibilityStart { get; set; }  
        public virtual string? HomePhoneNumber { get; set; }
        public virtual string? HomeAddressLine1 { get; set; }
        public virtual string? HomeAddressLine2 { get; set; }
        public virtual string? SubscriberMemNbr { get; set; }
        public virtual string? MailingCountryCode { get; set; }
        public virtual string? MailingAddressLine1 { get; set; }
        public virtual string? MailingAddressLine2 { get; set; }
        public virtual string? PersonUniqueIdentifier { get; set; }
        public virtual string? SubscriberMemNbrPrefix { get; set; }
        public virtual long RecordProcessingStatus { get; set; }
    }
}          