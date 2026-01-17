

ALTER TABLE etl.member_import_file_data
    DROP COLUMN IF EXISTS member_id,
    DROP COLUMN IF EXISTS member_type,
    DROP COLUMN IF EXISTS last_name,
    DROP COLUMN IF EXISTS first_name,
    DROP COLUMN IF EXISTS gender,
    DROP COLUMN IF EXISTS age,
    DROP COLUMN IF EXISTS dob,
    DROP COLUMN IF EXISTS email,
    DROP COLUMN IF EXISTS city,
    DROP COLUMN IF EXISTS country,
    DROP COLUMN IF EXISTS postal_code,
    DROP COLUMN IF EXISTS mobile_phone,
    DROP COLUMN IF EXISTS emp_or_dep,
    DROP COLUMN IF EXISTS mem_nbr,
    DROP COLUMN IF EXISTS subscriber_mem_nbr,
    DROP COLUMN IF EXISTS eligibility_start,
    DROP COLUMN IF EXISTS eligibility_end,
    DROP COLUMN IF EXISTS mailing_address_line1,
    DROP COLUMN IF EXISTS mailing_address_line2,
    DROP COLUMN IF EXISTS mailing_state,
    DROP COLUMN IF EXISTS mailing_country_code,
    DROP COLUMN IF EXISTS home_phone_number,
    DROP COLUMN IF EXISTS action,
    DROP COLUMN IF EXISTS partner_code,
    DROP COLUMN IF EXISTS middle_name,
    DROP COLUMN IF EXISTS home_address_line1,
    DROP COLUMN IF EXISTS home_address_line2,
    DROP COLUMN IF EXISTS home_state,
    DROP COLUMN IF EXISTS home_city,
    DROP COLUMN IF EXISTS home_postal_code,
    DROP COLUMN IF EXISTS language_code,
    DROP COLUMN IF EXISTS region_code,
    DROP COLUMN IF EXISTS subscriber_mem_nbr_prefix,
    DROP COLUMN IF EXISTS mem_nbr_prefix,
    DROP COLUMN IF EXISTS plan_id,
    DROP COLUMN IF EXISTS plan_type,
    DROP COLUMN IF EXISTS subgroup_id,
    DROP COLUMN IF EXISTS is_sso_user,
    DROP COLUMN IF EXISTS person_unique_identifier;
	
	
	
	-- Revert to not nullable
	ALTER TABLE etl.member_import_file_data
ALTER COLUMN raw_data_json SET NOT NULL;
