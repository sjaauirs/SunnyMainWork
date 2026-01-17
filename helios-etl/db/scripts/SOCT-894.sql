
-- Step 1: Add new column 
ALTER TABLE etl.member_import_file_data
    ADD COLUMN IF NOT EXISTS member_id TEXT ,
    ADD COLUMN IF NOT EXISTS member_type TEXT,
    ADD COLUMN IF NOT EXISTS last_name TEXT ,
    ADD COLUMN IF NOT EXISTS first_name TEXT ,
    ADD COLUMN IF NOT EXISTS gender TEXT ,
    ADD COLUMN IF NOT EXISTS age TEXT,
    ADD COLUMN IF NOT EXISTS dob TIMESTAMP WITHOUT TIME ZONE,
    ADD COLUMN IF NOT EXISTS email TEXT,
    ADD COLUMN IF NOT EXISTS city TEXT,
    ADD COLUMN IF NOT EXISTS country TEXT ,
    ADD COLUMN IF NOT EXISTS postal_code TEXT,
    ADD COLUMN IF NOT EXISTS mobile_phone TEXT,
    ADD COLUMN IF NOT EXISTS emp_or_dep TEXT,
    ADD COLUMN IF NOT EXISTS mem_nbr TEXT ,
    ADD COLUMN IF NOT EXISTS subscriber_mem_nbr TEXT,
    ADD COLUMN IF NOT EXISTS eligibility_start TIMESTAMP WITHOUT TIME ZONE ,
    ADD COLUMN IF NOT EXISTS eligibility_end TIMESTAMP WITHOUT TIME ZONE ,
    ADD COLUMN IF NOT EXISTS mailing_address_line1 TEXT,
    ADD COLUMN IF NOT EXISTS mailing_address_line2 TEXT,
    ADD COLUMN IF NOT EXISTS mailing_state TEXT,
    ADD COLUMN IF NOT EXISTS mailing_country_code TEXT,
    ADD COLUMN IF NOT EXISTS home_phone_number TEXT,
    ADD COLUMN IF NOT EXISTS action TEXT ,
    ADD COLUMN IF NOT EXISTS partner_code TEXT ,
    ADD COLUMN IF NOT EXISTS middle_name TEXT,
    ADD COLUMN IF NOT EXISTS home_address_line1 TEXT,
    ADD COLUMN IF NOT EXISTS home_address_line2 TEXT,
    ADD COLUMN IF NOT EXISTS home_state TEXT,
    ADD COLUMN IF NOT EXISTS home_city TEXT,
    ADD COLUMN IF NOT EXISTS home_postal_code TEXT,
    ADD COLUMN IF NOT EXISTS language_code TEXT,
    ADD COLUMN IF NOT EXISTS region_code TEXT,
    ADD COLUMN IF NOT EXISTS subscriber_mem_nbr_prefix TEXT,
    ADD COLUMN IF NOT EXISTS mem_nbr_prefix TEXT,
    ADD COLUMN IF NOT EXISTS plan_id TEXT,
    ADD COLUMN IF NOT EXISTS plan_type TEXT,
    ADD COLUMN IF NOT EXISTS subgroup_id TEXT,
    ADD COLUMN IF NOT EXISTS is_sso_user BOOLEAN,
    ADD COLUMN IF NOT EXISTS person_unique_identifier TEXT ;


-- Step 2: fill data in new column for existing records

UPDATE etl.member_import_file_data
SET
  age = raw_data_json ->> 'age',
  dob = CASE 
    WHEN raw_data_json ->> 'dob' ~ '^\d{2}/\d{2}/\d{4}$' THEN 
      TO_DATE(raw_data_json ->> 'dob', 'MM/DD/YYYY')
    WHEN raw_data_json ->> 'dob' ~ '^\d{4}-\d{2}-\d{2}' THEN 
      (raw_data_json ->> 'dob')::timestamp::date
    ELSE NULL
  END,
  city = raw_data_json ->> 'city',
  email = raw_data_json ->> 'email',
  action = raw_data_json ->> 'action',
  gender = raw_data_json ->> 'gender',
  country = raw_data_json ->> 'country',
  mem_nbr = raw_data_json ->> 'mem_nbr',
  plan_id = raw_data_json ->> 'plan_id',
  home_city = raw_data_json ->> 'home_city',
  last_name = raw_data_json ->> 'last_name',
  member_id = raw_data_json ->> 'member_id',
  plan_type = raw_data_json ->> 'plan_type',
  emp_or_dep = raw_data_json ->> 'emp_or_dep',
  first_name = raw_data_json ->> 'first_name',
  home_state = raw_data_json ->> 'home_state',
  is_sso_user = (raw_data_json ->> 'is_sso_user')::boolean,
  member_type = raw_data_json ->> 'member_type',
  middle_name = raw_data_json ->> 'middle_name',
  postal_code = raw_data_json ->> 'postal_code',
  region_code = raw_data_json ->> 'region_code',
  subgroup_id = raw_data_json ->> 'subgroup_id',
  mobile_phone = raw_data_json ->> 'mobile_phone',
  partner_code = raw_data_json ->> 'partner_code',
  language_code = raw_data_json ->> 'language_code',
  mailing_state = raw_data_json ->> 'mailing_state',
  mem_nbr_prefix = raw_data_json ->> 'mem_nbr_prefix',
  eligibility_end = CASE 
    WHEN raw_data_json ->> 'eligibility_end' ~ '^\d{2}/\d{2}/\d{4}$' THEN 
      TO_DATE(raw_data_json ->> 'eligibility_end', 'MM/DD/YYYY')
    WHEN raw_data_json ->> 'eligibility_end' ~ '^\d{4}-\d{2}-\d{2}' THEN 
      (raw_data_json ->> 'eligibility_end')::timestamp::date
    ELSE NULL
  END, 
  home_postal_code = raw_data_json ->> 'home_postal_code',
  eligibility_start = 
  CASE 
    WHEN raw_data_json ->> 'eligibility_start' ~ '^\d{2}/\d{2}/\d{4}$' THEN 
      TO_DATE(raw_data_json ->> 'eligibility_start', 'MM/DD/YYYY')
    WHEN raw_data_json ->> 'eligibility_start' ~ '^\d{4}-\d{2}-\d{2}' THEN 
      (raw_data_json ->> 'eligibility_start')::timestamp::date
    ELSE NULL
  END,
  home_phone_number = raw_data_json ->> 'home_phone_number',
  home_address_line1 = raw_data_json ->> 'home_address_line1',
  home_address_line2 = raw_data_json ->> 'home_address_line2',
  subscriber_mem_nbr = raw_data_json ->> 'subscriber_mem_nbr',
  mailing_country_code = raw_data_json ->> 'mailing_country_code',
  mailing_address_line1 = raw_data_json ->> 'mailing_address_line1',
  mailing_address_line2 = raw_data_json ->> 'mailing_address_line2',
  person_unique_identifier = raw_data_json ->> 'person_unique_identifier',
  subscriber_mem_nbr_prefix = raw_data_json ->> 'subscribermem_nbr_prefix'
WHERE raw_data_json IS NOT NULL
  AND raw_data_json::text <> '{}'
  AND Delete_nbr =0;

-- step3: Make raw_data_json as nullable
ALTER TABLE etl.member_import_file_data
ALTER COLUMN raw_data_json DROP NOT NULL;

