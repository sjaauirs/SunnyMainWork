namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class EnrollmentCsvDto
    {
        public string mem_nbr { get; set; }
        public DateTime enr_start { get; set; }
        public DateTime enr_end { get; set; }
        public string ben_medical { get; set; }
        public string ben_dent { get; set; }
        public string ben_rx { get; set; }
        public string ben_mh_inp { get; set; }
        public string ben_mh_int { get; set; }
        public string ben_mh_amb { get; set; }
        public string ben_cd_inp { get; set; }
        public string ben_cd_int { get; set; }
        public string ben_cd_amb { get; set; }
        public string ben_hospice { get; set; }
        public string ben_dis { get; set; }
        public string enr_subscriber_num { get; set; }
        public string hp_employee { get; set; }
        public string emp_nbr { get; set; }
        public int med_elig_cat_id { get; set; }
        public string? product_id { get; set; }
        public string? product_id_2 { get; set; }
        public string coverage_indicator { get; set; }
        public string pbp_nbr { get; set; }
        public string snp_type { get; set; }
        public string amp_nbr { get; set; }
        public string cin_number { get; set; }
    }
}
