namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberCsvDto
    {
        public string mem_nbr { get; set; }
        public string mem_lname { get; set; }
        public string mem_fname { get; set; }
        public string mem_mname { get; set; }
        public DateTime mem_dob { get; set; }
        public DateTime? deceased_dt { get; set; }
        public string mem_gender { get; set; }
        public string mem_addr1 { get; set; }
        public string mem_addr2 { get; set; }
        public string mem_addr3 { get; set; }
        public string mem_city { get; set; }
        public string mem_state { get; set; }
        public string mem_zip { get; set; }
        public string mem_county { get; set; }
        public string mem_county_code { get; set; }
        public string mem_phone { get; set; }
        public string mem_email { get; set; }
        public string mem_ssn { get; set; }
        public string mem_hisp_orig { get; set; }
        public string mem_need_interpreter { get; set; }
        public string race_code_1 { get; set; }
        public string race_code_2 { get; set; }
        public string race_code_3 { get; set; }
        public string hic_number { get; set; }
        public string mbi_number { get; set; }
        public string mrn_number { get; set; }
        public string guard_lname { get; set; }
        public string guard_fname { get; set; }
        public string guard_mname { get; set; }
        public string guard_email { get; set; }
        public string spoken_language_source { get; set; }
        public string written_language_source { get; set; }
        public string other_language_source { get; set; }
        public string spoken_language_id { get; set; }
        public string written_language_id { get; set; }
        public string other_language_id { get; set; }
        public string race_source { get; set; }
        public string ethnicity_source { get; set; }
    }


}