using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberImportCSVDto
    {
        [Required]
        public string member_id { get; set; }
        public string? member_type { get; set; }
        [Required]
        public string last_name { get; set; }
        [Required]
        public string first_name { get; set; }
        [Required]
        public string gender { get; set; }
        public string? age { get; set; }
        [Required(ErrorMessage = "Dob is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public string dob { get; set; }
        public string? email { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string postal_code { get; set; }
        public string mobile_phone { get; set; }
        public string emp_or_dep { get; set; }
        [Required]
        public string mem_nbr { get; set; }
        public string subscriber_mem_nbr { get; set; }
        [Required(ErrorMessage = "The eligibility start date is required.")]
        [DataType(DataType.Date, ErrorMessage = "The eligibility start date must be in a valid date format.")]
        public string eligibility_start { get; set; }
        [Required(ErrorMessage = "The eligibility end date is required.")]
        [DataType(DataType.Date, ErrorMessage = "The eligibility end date must be in a valid date format.")]
        public string eligibility_end { get; set; }
        public string mailing_address_line1 { get; set; }
        public string mailing_address_line2 { get; set; }
        public string mailing_state { get; set; }
        public string mailing_country_code { get; set; }
        public string home_phone_number { get; set; }
        [Required]
        public string action { get; set; }
        [Required]
        public string partner_code { get; set; }
        public string? middle_name { get; set; }
        public string? home_address_line1 { get; set; }
        public string? home_address_line2 { get; set; }
        public string? home_state { get; set; }
        public string? home_city { get; set; }
        public string? home_postal_code { get; set; }
        public string? language_code { get; set; }
        public string? region_code { get; set; }
        public string? subscriber_mem_nbr_prefix { get; set; }
        public string? mem_nbr_prefix { get; set; }
        public string? plan_id { get; set; }
        public string? plan_type { get; set; }
        public string? subgroup_id { get; set; }
        public bool? is_sso_user { get; set; }

        [Required]
        public string? person_unique_identifier { get; set; }

        public long? member_import_file_data_id { get; set; }
        public  string? raw_data_json { get; set; }
    }
}
