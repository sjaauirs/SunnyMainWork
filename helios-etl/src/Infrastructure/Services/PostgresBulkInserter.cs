using Npgsql;
using NpgsqlTypes;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class PostgresBulkInserter : IPostgresBulkInserter
    {
        public async Task BulkInsertAsync(string postgresConnectionString, List<ETLMemberImportFileDataModel> dataModels)
        {
                await using var conn = new NpgsqlConnection(postgresConnectionString);
            await conn.OpenAsync();

            using var writer = conn.BeginBinaryImport(@"
                COPY etl.member_import_file_data (
                    member_import_file_id,
                    record_number,
                    raw_data_json,
                    create_ts,
                    create_user,
                    delete_nbr,
                    region_code, member_id, subscriber_mem_nbr_prefix, mem_nbr, subscriber_mem_nbr, mem_nbr_prefix,
                    member_type, plan_id, subgroup_id, plan_type, eligibility_start, eligibility_end, last_name,
                    middle_name, first_name,dob, gender, email, home_phone_number, mobile_phone,
                    home_address_line1, home_address_line2, home_city, home_state, home_postal_code,
                    mailing_address_line1, mailing_address_line2, city, mailing_state, postal_code,
                    language_code, action, partner_code, age, country, emp_or_dep, mailing_country_code,
                    person_unique_identifier, is_sso_user

                ) FROM STDIN (FORMAT BINARY)"
            );

            foreach (var model in dataModels)
            {
                    await writer.StartRowAsync();
                    writer.Write(model.MemberImportFileId, NpgsqlTypes.NpgsqlDbType.Bigint);
                    writer.Write(model.RecordNumber, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(model.RawDataJson, NpgsqlTypes.NpgsqlDbType.Jsonb);
                    writer.Write(model.CreateTs, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    writer.Write(model.CreateUser, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.DeleteNbr, NpgsqlTypes.NpgsqlDbType.Bigint);

                    writer.Write(model.RegionCode, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MemberId, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.SubscriberMemNbrPrefix, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MemNbr, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.SubscriberMemNbr, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MemNbrPrefix, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MemberType, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.PlanId, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.SubgroupId, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.PlanType, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.EligibilityStart, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    writer.Write(model.EligibilityEnd, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    writer.Write(model.LastName, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MiddleName, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.FirstName, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.Dob, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    writer.Write(model.Gender, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.Email, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.HomePhoneNumber, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MobilePhone, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.HomeAddressLine1, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.HomeAddressLine2, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.HomeCity, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.HomeState, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.HomePostalCode, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MailingAddressLine1, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MailingAddressLine2, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.City, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MailingState, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.PostalCode, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.LanguageCode, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.Action, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.PartnerCode, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.Age, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.Country, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.EmpOrDep, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.MailingCountryCode, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.PersonUniqueIdentifier, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(model.IsSsoUser, NpgsqlTypes.NpgsqlDbType.Boolean);


            }
            await writer.CompleteAsync();
        }        
    }
}