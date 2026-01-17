using Npgsql;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Data;
using System.Globalization;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class RedshiftDataReader : IRedshiftDataReader
    {
        public async Task<List<RedShiftMemberImportFileDataDto>> FetchBatchAsync(string redshiftConnectionString, long? lastMemberImportFileId, int batchSize)
        {
            const string query = @"
                SELECT 
                    d.member_import_file_data_id,
                    d.member_import_file_id,
                    d.record_number,
                    d.raw_data_json,
                     d.member_id,
                    d.member_type,
                    d.last_name,
                    d.first_name,
                    d.gender,
                    d.age,
                    d.dob,
                    d.email,
                    d.city,
                    d.country,
                    d.postal_code,
                    d.mobile_phone,
                    d.emp_or_dep,
                    d.mem_nbr,
                    d.subscriber_mem_nbr,
                    d.eligibility_start,
                    d.eligibility_end,
                    d.mailing_address_line1,
                    d.mailing_address_line2,
                    d.mailing_state,
                    d.mailing_country_code,
                    d.home_phone_number,
                    d.action,
                    d.partner_code,
                    d.middle_name,
                    d.home_address_line1,
                    d.home_address_line2,
                    d.home_state,
                    d.home_city,
                    d.home_postal_code,
                    d.language_code,
                    d.region_code,
                    d.subscriber_mem_nbr_prefix,
                    d.mem_nbr_prefix,
                    d.plan_id,
                    d.plan_type,
                    d.subgroup_id,
                    d.is_sso_user,
                    d.person_unique_identifier,
                    d.create_ts,
                    d.update_ts,
                    d.create_user,
                    d.update_user,
                    d.publish_status,
                    d.publishing_lock_id,
                    d.publishing_lock_ts,
                    d.publish_attempts,     
                    f.file_name
                FROM etl_outbound.member_import_file_data AS d
                JOIN etl_outbound.member_import_file AS f
                    ON d.member_import_file_id = f.member_import_file_id
                 WHERE (@LastMemberImportFileId IS NULL OR d.member_import_file_data_id > @LastMemberImportFileId)
                ORDER BY d.member_import_file_data_id
                LIMIT @BatchSize;
            ";

            var result = new List<RedShiftMemberImportFileDataDto>();

            await using var conn = new NpgsqlConnection(redshiftConnectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.Add("@LastMemberImportFileId", NpgsqlTypes.NpgsqlDbType.Bigint).Value =
            (object?)lastMemberImportFileId ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@BatchSize", batchSize);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {

                var redShiftMemberImportFileDataDto = MapRedshiftToMemberImport(reader);
                result.Add(redShiftMemberImportFileDataDto);
            }

            return result;
        }

        public async Task<List<RedShiftMemberImportFileDataDto>> FetchAndClaimBatchAsync(
                string redshiftConnectionString, string partnerCode,
                string jobId,
                int batchSize)
        {
            const string claimQuery = @"
                -- Step 1: claim a batch of NOT_STARTED rows
                UPDATE etl_outbound.member_import_file_data
                SET publish_status = 'IN_PROGRESS',
                    publishing_lock_id = @JobId,
                    publishing_lock_ts = GETDATE(),
                    publish_attempts = publish_attempts + 1
                WHERE member_import_file_data_id IN (
                    SELECT member_import_file_data_id
                    FROM etl_outbound.member_import_file_data
                    WHERE publish_status = 'NOT_STARTED'
                      AND publishing_lock_id IS NULL
                      AND (partner_code = @PartnerCode)
                    ORDER BY member_import_file_data_id
                    LIMIT @BatchSize
                );";

            const string selectQuery = @"
                -- Step 2: fetch only rows claimed by this job
                SELECT 
                    d.member_import_file_data_id,
                    d.member_import_file_id,
                    d.record_number,
                    d.raw_data_json,
                    d.member_id,
                    d.member_type,
                    d.last_name,
                    d.first_name,
                    d.gender,
                    d.age,
                    d.dob,
                    d.email,
                    d.city,
                    d.country,
                    d.postal_code,
                    d.mobile_phone,
                    d.emp_or_dep,
                    d.mem_nbr,
                    d.subscriber_mem_nbr,
                    d.eligibility_start,
                    d.eligibility_end,
                    d.mailing_address_line1,
                    d.mailing_address_line2,
                    d.mailing_state,
                    d.mailing_country_code,
                    d.home_phone_number,
                    d.action,
                    d.partner_code,
                    d.middle_name,
                    d.home_address_line1,
                    d.home_address_line2,
                    d.home_state,
                    d.home_city,
                    d.home_postal_code,
                    d.language_code,
                    d.region_code,
                    d.subscriber_mem_nbr_prefix,
                    d.mem_nbr_prefix,
                    d.plan_id,
                    d.plan_type,
                    d.subgroup_id,
                    d.is_sso_user,
                    d.person_unique_identifier,
                    d.create_ts,
                    d.update_ts,
                    d.create_user,
                    d.update_user,
                    d.publish_status,
                    d.publishing_lock_id,
                    d.publishing_lock_ts,
                    d.publish_attempts,
                    NULL AS file_name
                FROM etl_outbound.member_import_file_data AS d
                WHERE d.publishing_lock_id = @JobId
                  AND d.publish_status = 'IN_PROGRESS'
                ORDER BY d.member_import_file_data_id;
            ";

            var result = new List<RedShiftMemberImportFileDataDto>();

            await using var conn = new NpgsqlConnection(redshiftConnectionString);
            await conn.OpenAsync();

            // Step 1: claim rows
            await using (var claimCmd = new NpgsqlCommand(claimQuery, conn))
            {
                claimCmd.Parameters.AddWithValue("@JobId", jobId.ToString());
                claimCmd.Parameters.AddWithValue("@BatchSize", batchSize);
                claimCmd.Parameters.AddWithValue("@PartnerCode", partnerCode.ToString());
                claimCmd.CommandTimeout = 120;
                await claimCmd.ExecuteNonQueryAsync();
            }

            // Step 2: retrieve claimed rows
            await using (var selectCmd = new NpgsqlCommand(selectQuery, conn))
            {
                selectCmd.Parameters.AddWithValue("@JobId", jobId.ToString());
                selectCmd.CommandTimeout = 120;
                await using var reader = await selectCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(MapRedshiftToMemberImport(reader));
                }
            }

            return result;
        }

        public async Task MarkPublishStatusAsync(string redshiftConnectionString, long rowId, string publishStatus)
        {
            const string sql = @"
                UPDATE etl_outbound.member_import_file_data
                SET publish_status = @PublishStatus
                WHERE member_import_file_data_id = @RowId;
            ";

            await using var conn = new NpgsqlConnection(redshiftConnectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.CommandTimeout = 120;
            cmd.Parameters.AddWithValue("@PublishStatus", publishStatus);
            cmd.Parameters.AddWithValue("@RowId", rowId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkPublishStatusBatchAsync(
            string redshiftConnectionString,
            IEnumerable<(long RowId, string PublishStatus)> updates)
        {
            const string tableName = "etl_outbound.member_import_file_data";

            var updatesList = updates.ToList();
            if (!updatesList.Any())
                return;

            // Build CASE WHEN clauses
            var cases = new StringBuilder();
            var ids = new List<long>();
            int i = 0;

            foreach (var u in updatesList)
            {
                cases.Append($"WHEN {u.RowId} THEN @p{i} ");
                ids.Add(u.RowId);
                i++;
            }

            string sql = $@"
                UPDATE {tableName}
                SET publish_status = CASE member_import_file_data_id
                    {cases}
                END
                WHERE member_import_file_data_id IN ({string.Join(",", ids)});
            ";

            await using var conn = new NpgsqlConnection(redshiftConnectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.CommandTimeout = 120;
            for (int j = 0; j < updatesList.Count; j++)
                cmd.Parameters.AddWithValue($"@p{j}", updatesList[j].PublishStatus);

            await cmd.ExecuteNonQueryAsync();
        }



        private RedShiftMemberImportFileDataDto MapRedshiftToMemberImport(NpgsqlDataReader reader)
        {
            var redShiftMemberImportFileDataDto = new RedShiftMemberImportFileDataDto();

            redShiftMemberImportFileDataDto.MemberImportFileDataId = reader.GetInt64(reader.GetOrdinal("member_import_file_data_id"));
            redShiftMemberImportFileDataDto.MemberImportFileId = reader.GetInt64(reader.GetOrdinal("member_import_file_id"));
            redShiftMemberImportFileDataDto.RecordNumber = reader.GetInt32(reader.GetOrdinal("record_number"));

            var rawDataJsonOrdinal = reader.GetOrdinal("raw_data_json");
            redShiftMemberImportFileDataDto.RawDataJson = reader.IsDBNull(rawDataJsonOrdinal) ? "{}" : string.IsNullOrWhiteSpace(reader.GetString(rawDataJsonOrdinal)) ? "{}" : reader.GetString(rawDataJsonOrdinal);

            redShiftMemberImportFileDataDto.MemberId = reader.GetString(reader.GetOrdinal("member_id"));
            redShiftMemberImportFileDataDto.MemberType = reader.IsDBNull(reader.GetOrdinal("member_type")) ? null : reader.GetString(reader.GetOrdinal("member_type"));
            redShiftMemberImportFileDataDto.LastName = reader.GetString(reader.GetOrdinal("last_name"));
            redShiftMemberImportFileDataDto.FirstName = reader.GetString(reader.GetOrdinal("first_name"));
            redShiftMemberImportFileDataDto.Gender = reader.IsDBNull(reader.GetOrdinal("gender")) ? null : reader.GetString(reader.GetOrdinal("gender"));
            redShiftMemberImportFileDataDto.Age = reader.IsDBNull(reader.GetOrdinal("age")) ? null : reader.GetInt32(reader.GetOrdinal("age")).ToString();
            redShiftMemberImportFileDataDto.Dob = reader.IsDBNull(reader.GetOrdinal("dob")) ? null : reader.GetDateTime(reader.GetOrdinal("dob"));
            redShiftMemberImportFileDataDto.Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email"));
            redShiftMemberImportFileDataDto.City = reader.IsDBNull(reader.GetOrdinal("city")) ? null : reader.GetString(reader.GetOrdinal("city"));
            redShiftMemberImportFileDataDto.Country = reader.IsDBNull(reader.GetOrdinal("country")) ? null : reader.GetString(reader.GetOrdinal("country"));
            redShiftMemberImportFileDataDto.PostalCode = reader.IsDBNull(reader.GetOrdinal("postal_code")) ? null : reader.GetString(reader.GetOrdinal("postal_code"));
            redShiftMemberImportFileDataDto.MobilePhone = reader.IsDBNull(reader.GetOrdinal("mobile_phone")) ? null : reader.GetString(reader.GetOrdinal("mobile_phone"));
            redShiftMemberImportFileDataDto.EmpOrDep = reader.IsDBNull(reader.GetOrdinal("emp_or_dep")) ? null : reader.GetString(reader.GetOrdinal("emp_or_dep"));
            redShiftMemberImportFileDataDto.MemNbr = reader.GetString(reader.GetOrdinal("mem_nbr"));
            redShiftMemberImportFileDataDto.SubscriberMemNbr = reader.IsDBNull(reader.GetOrdinal("subscriber_mem_nbr")) ? null : reader.GetString(reader.GetOrdinal("subscriber_mem_nbr"));
            redShiftMemberImportFileDataDto.EligibilityStart = reader.IsDBNull(reader.GetOrdinal("eligibility_start")) ? null : reader.GetDateTime(reader.GetOrdinal("eligibility_start"));
            redShiftMemberImportFileDataDto.EligibilityEnd = reader.IsDBNull(reader.GetOrdinal("eligibility_end")) ? null : reader.GetDateTime(reader.GetOrdinal("eligibility_end"));
            redShiftMemberImportFileDataDto.MailingAddressLine1 = reader.IsDBNull(reader.GetOrdinal("mailing_address_line1")) ? null : reader.GetString(reader.GetOrdinal("mailing_address_line1"));
            redShiftMemberImportFileDataDto.MailingAddressLine2 = reader.IsDBNull(reader.GetOrdinal("mailing_address_line2")) ? null : reader.GetString(reader.GetOrdinal("mailing_address_line2"));
            redShiftMemberImportFileDataDto.MailingState = reader.IsDBNull(reader.GetOrdinal("mailing_state")) ? null : reader.GetString(reader.GetOrdinal("mailing_state"));
            redShiftMemberImportFileDataDto.MailingCountryCode = reader.IsDBNull(reader.GetOrdinal("mailing_country_code")) ? null : reader.GetString(reader.GetOrdinal("mailing_country_code"));
            redShiftMemberImportFileDataDto.HomePhoneNumber = reader.IsDBNull(reader.GetOrdinal("home_phone_number")) ? null : reader.GetString(reader.GetOrdinal("home_phone_number"));
            redShiftMemberImportFileDataDto.Action = reader.GetString(reader.GetOrdinal("action"));
            redShiftMemberImportFileDataDto.PartnerCode = reader.GetString(reader.GetOrdinal("partner_code"));
            redShiftMemberImportFileDataDto.MiddleName = reader.IsDBNull(reader.GetOrdinal("middle_name")) ? null : reader.GetString(reader.GetOrdinal("middle_name"));
            redShiftMemberImportFileDataDto.HomeAddressLine1 = reader.IsDBNull(reader.GetOrdinal("home_address_line1")) ? null : reader.GetString(reader.GetOrdinal("home_address_line1"));
            redShiftMemberImportFileDataDto.HomeAddressLine2 = reader.IsDBNull(reader.GetOrdinal("home_address_line2")) ? null : reader.GetString(reader.GetOrdinal("home_address_line2"));
            redShiftMemberImportFileDataDto.HomeState = reader.IsDBNull(reader.GetOrdinal("home_state")) ? null : reader.GetString(reader.GetOrdinal("home_state"));
            redShiftMemberImportFileDataDto.HomeCity = reader.IsDBNull(reader.GetOrdinal("home_city")) ? null : reader.GetString(reader.GetOrdinal("home_city"));
            redShiftMemberImportFileDataDto.HomePostalCode = reader.IsDBNull(reader.GetOrdinal("home_postal_code")) ? null : reader.GetString(reader.GetOrdinal("home_postal_code"));
            redShiftMemberImportFileDataDto.LanguageCode = reader.IsDBNull(reader.GetOrdinal("language_code")) ? null : reader.GetString(reader.GetOrdinal("language_code"));
            redShiftMemberImportFileDataDto.RegionCode = reader.IsDBNull(reader.GetOrdinal("region_code")) ? null : reader.GetString(reader.GetOrdinal("region_code"));
            redShiftMemberImportFileDataDto.SubscriberMemNbrPrefix = reader.IsDBNull(reader.GetOrdinal("subscriber_mem_nbr_prefix")) ? null : reader.GetString(reader.GetOrdinal("subscriber_mem_nbr_prefix"));
            redShiftMemberImportFileDataDto.MemNbrPrefix = reader.IsDBNull(reader.GetOrdinal("mem_nbr_prefix")) ? null : reader.GetString(reader.GetOrdinal("mem_nbr_prefix"));
            redShiftMemberImportFileDataDto.PlanId = reader.IsDBNull(reader.GetOrdinal("plan_id")) ? null : reader.GetString(reader.GetOrdinal("plan_id"));
            redShiftMemberImportFileDataDto.PlanType = reader.IsDBNull(reader.GetOrdinal("plan_type")) ? null : reader.GetString(reader.GetOrdinal("plan_type"));
            redShiftMemberImportFileDataDto.SubgroupId = reader.IsDBNull(reader.GetOrdinal("subgroup_id")) ? null : reader.GetString(reader.GetOrdinal("subgroup_id"));
            redShiftMemberImportFileDataDto.IsSsoUser = reader.IsDBNull(reader.GetOrdinal("is_sso_user")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("is_sso_user"));
            redShiftMemberImportFileDataDto.PersonUniqueIdentifier = reader.GetString(reader.GetOrdinal("person_unique_identifier"));
            redShiftMemberImportFileDataDto.CreateTs = reader.GetDateTime(reader.GetOrdinal("create_ts"));
            redShiftMemberImportFileDataDto.UpdateTs = reader.IsDBNull(reader.GetOrdinal("update_ts")) ? null : reader.GetDateTime(reader.GetOrdinal("update_ts"));
            redShiftMemberImportFileDataDto.CreateUser = reader.GetString(reader.GetOrdinal("create_user"));
            redShiftMemberImportFileDataDto.UpdateUser = reader.IsDBNull(reader.GetOrdinal("update_user")) ? null : reader.GetString(reader.GetOrdinal("update_user"));
            redShiftMemberImportFileDataDto.FileName = reader.IsDBNull(reader.GetOrdinal("file_name")) ? null : reader.GetString(reader.GetOrdinal("file_name"));
            redShiftMemberImportFileDataDto.PublishStatus = reader.IsDBNull(reader.GetOrdinal("publish_status")) ? null : reader.GetString(reader.GetOrdinal("publish_status"));
            redShiftMemberImportFileDataDto.PublishingLockId = reader.IsDBNull(reader.GetOrdinal("publishing_lock_id")) ? null : reader.GetString(reader.GetOrdinal("publishing_lock_id"));
            redShiftMemberImportFileDataDto.PublishingLockTs = reader.IsDBNull(reader.GetOrdinal("publishing_lock_ts")) ? null : reader.GetDateTime(reader.GetOrdinal("publishing_lock_ts"));
            redShiftMemberImportFileDataDto.PublishAttempts = reader.IsDBNull(reader.GetOrdinal("publish_attempts")) ? 0 : reader.GetInt32(reader.GetOrdinal("publish_attempts"));
            return redShiftMemberImportFileDataDto;
        }

        public async Task<List<RedShiftCohortDataDto>> FetchAndClaimCohortBatchAsync(
                string redshiftConnectionString, string partnerCode, string jobId,
                int batchSize)
        {
            const string claimQuery = @"
                -- Step 1: claim a batch of NOT_STARTED rows
                UPDATE etl_outbound.consumer_cohort_import
                SET publish_status = 'IN_PROGRESS',
                    publishing_lock_id = @JobId
                WHERE consumer_cohort_import_id IN (
                    SELECT consumer_cohort_import_id
                    FROM etl_outbound.consumer_cohort_import
                    WHERE publish_status = 'NOT_STARTED'
                      AND (partner_code = @PartnerCode)
                      AND publishing_lock_id IS NULL
                    ORDER BY consumer_cohort_import_id
                    LIMIT @BatchSize
                );";

            const string selectQuery = @"
                -- Step 2: fetch only rows claimed by this job
                SELECT 
                    consumer_cohort_import_id,
                    partner_code,
                    cohort_name,
                    person_unique_identifier,
                    action,
                    create_ts,
                    publish_status,
                    publishing_lock_id
                FROM etl_outbound.consumer_cohort_import 
                WHERE publish_status = 'IN_PROGRESS'
                    AND publishing_lock_id = @JobId
                ORDER BY consumer_cohort_import_id;
            ";

            var result = new List<RedShiftCohortDataDto>();

            await using var conn = new NpgsqlConnection(redshiftConnectionString);
            await conn.OpenAsync();

            // Step 1: claim rows
            await using (var claimCmd = new NpgsqlCommand(claimQuery, conn))
            {
                claimCmd.CommandTimeout = 120;
                claimCmd.Parameters.AddWithValue("@BatchSize", batchSize);
                claimCmd.Parameters.AddWithValue("@JobId", jobId.ToString());
                claimCmd.Parameters.AddWithValue("@PartnerCode", partnerCode.ToString());
                await claimCmd.ExecuteNonQueryAsync();
            }

            // Step 2: retrieve claimed rows
            await using (var selectCmd = new NpgsqlCommand(selectQuery, conn))
            {
                selectCmd.CommandTimeout = 120;
                selectCmd.Parameters.AddWithValue("@JobId", jobId.ToString());
                await using var reader = await selectCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(MapRedshiftToCohort(reader));
                }
            }

            return result;
        }

        private RedShiftCohortDataDto MapRedshiftToCohort(NpgsqlDataReader reader)
        {
            var redShiftCohortDataDto = new RedShiftCohortDataDto();
            redShiftCohortDataDto.ConsumerCohortImportId = reader.GetInt64(reader.GetOrdinal("consumer_cohort_import_id"));
            redShiftCohortDataDto.PartnerCode = reader.IsDBNull(reader.GetOrdinal("partner_code")) ? null : reader.GetString(reader.GetOrdinal("partner_code"));
            redShiftCohortDataDto.PersonUniqueIdentifier = reader.IsDBNull(reader.GetOrdinal("person_unique_identifier")) ? null : reader.GetString(reader.GetOrdinal("person_unique_identifier"));
            redShiftCohortDataDto.CohortName = reader.IsDBNull(reader.GetOrdinal("cohort_name")) ? null : reader.GetString(reader.GetOrdinal("cohort_name"));
            redShiftCohortDataDto.Action = reader.IsDBNull(reader.GetOrdinal("action")) ? null : reader.GetString(reader.GetOrdinal("action"));
            redShiftCohortDataDto.CreateTs = reader.GetDateTime(reader.GetOrdinal("create_ts"));
            redShiftCohortDataDto.PublishStatus = reader.IsDBNull(reader.GetOrdinal("publish_status")) ? null : reader.GetString(reader.GetOrdinal("publish_status"));
            redShiftCohortDataDto.PublishingLockId = reader.IsDBNull(reader.GetOrdinal("publishing_lock_id")) ? null : reader.GetString(reader.GetOrdinal("publishing_lock_id"));
            return redShiftCohortDataDto;
        }

        public async Task MarkCohortPublishStatusAsync(string redshiftConnectionString, long rowId, string publishStatus)
        {
            const string sql = @"
                UPDATE etl_outbound.consumer_cohort_import
                SET publish_status = @PublishStatus
                WHERE consumer_cohort_import_id = @RowId;
            ";

            await using var conn = new NpgsqlConnection(redshiftConnectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.CommandTimeout = 120;
            cmd.Parameters.AddWithValue("@PublishStatus", publishStatus);
            cmd.Parameters.AddWithValue("@RowId", rowId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkCohortPublishStatusBatchAsync(
            string redshiftConnectionString,
            IEnumerable<(long RowId, string PublishStatus)> updates)
        {
            const string tableName = "etl_outbound.consumer_cohort_import";

            var updatesList = updates.ToList();
            if (!updatesList.Any())
                return;

            // Build CASE WHEN clauses
            var cases = new StringBuilder();
            var ids = new List<long>();
            int i = 0;

            foreach (var u in updatesList)
            {
                cases.Append($"WHEN {u.RowId} THEN @p{i} ");
                ids.Add(u.RowId);
                i++;
            }

            string sql = $@"
                UPDATE {tableName}
                SET publish_status = CASE consumer_cohort_import_id
                    {cases}
                END
                WHERE consumer_cohort_import_id IN ({string.Join(",", ids)});
            ";

            await using var conn = new NpgsqlConnection(redshiftConnectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.CommandTimeout = 120;
            for (int j = 0; j < updatesList.Count; j++)
                cmd.Parameters.AddWithValue($"@p{j}", updatesList[j].PublishStatus);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
