DROP TABLE IF EXISTS outbound.member_import_file_data;

CREATE TABLE IF NOT EXISTS outbound.member_import_file_data
(
    member_import_file_data_id BIGINT IDENTITY(1,1) NOT NULL,
    member_import_file_id BIGINT NOT NULL,
    record_number INTEGER NOT NULL,
    raw_data_json VARCHAR(MAX) NULL, -- JSONB not supported in Redshift
    

    -- Additional Columns
    member_id TEXT NOT NULL,
    member_type TEXT,
    last_name TEXT NOT NULL,
    first_name TEXT NOT NULL,
    gender TEXT NOT NULL,
    age TEXT,
    dob TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    email TEXT,
    city TEXT,
    country TEXT NOT NULL,
    postal_code TEXT,
    mobile_phone TEXT,
    emp_or_dep TEXT,
    mem_nbr TEXT NOT NULL,
    subscriber_mem_nbr TEXT,
    eligibility_start TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    eligibility_end TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    mailing_address_line1 TEXT,
    mailing_address_line2 TEXT,
    mailing_state TEXT,
    mailing_country_code TEXT,
    home_phone_number TEXT,
    action TEXT NOT NULL,
    partner_code TEXT NOT NULL,
    middle_name TEXT,
    home_address_line1 TEXT,
    home_address_line2 TEXT,
    home_state TEXT,
    home_city TEXT,
    home_postal_code TEXT,
    language_code TEXT,
    region_code TEXT,
    subscriber_mem_nbr_prefix TEXT,
    mem_nbr_prefix TEXT,
    plan_id TEXT,
    plan_type TEXT,
    subgroup_id TEXT,
    is_sso_user BOOLEAN,
    person_unique_identifier TEXT NOT NULL,
    create_ts TIMESTAMP NOT NULL,
    update_ts TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT NOT NULL,

    PRIMARY KEY (member_import_file_data_id)
)
DISTSTYLE KEY
DISTKEY(member_import_file_id)
SORTKEY(create_ts);

-- Sample insert STATEMENT

INSERT INTO outbound.member_import_file_data (
    member_import_file_id,
    record_number,
    raw_data_json,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr,
    region_code, member_id, subscriber_mem_nbr_prefix, mem_nbr, subscriber_mem_nbr, mem_nbr_prefix,
    member_type, plan_id, subgroup_id, plan_type, eligibility_start, eligibility_end, last_name,
    middle_name, first_name, dob, gender, email, home_phone_number, mobile_phone,
    home_address_line1, home_address_line2, home_city, home_state, home_postal_code,
    mailing_address_line1, mailing_address_line2, city, mailing_state, postal_code,
    language_code, action, partner_code, age, country, emp_or_dep, mailing_country_code,
    person_unique_identifier, is_sso_user
) VALUES (
    1, -- member_import_file_id
    1, -- record_number
    null, -- raw_data_json
    CURRENT_TIMESTAMP, -- create_ts
    NULL, -- update_ts
    'system', -- create_user
    NULL, -- update_user
    0, -- delete_nbr
    'CO', 6897816, 'smnp', '494657ce-217d-4e96-8e58-5085e57133f4', '494657ce-217d-4e96-8e58-5085e57133f4', 'mnp',
    'SU', '40513', '51', 'DHMO', '2024-01-01', '2026-12-31', 'Noli', 'A', 'Winnie', '1989-10-24', 'M',
    '3wnoli011@trellian.com', '269-680-5885', '612-901-3922', 'PO Box 79109', '3654 Grayhawk Junction',
    'Syracuse', 'New York', '14201', '123 Main Street', 'test1', 'Nashua', 'NH', '10005',
    'en-US', 'I', 'par-7e92b06aa4fe405198d27d2427bf3de4', 94, 'US', 'E', '840', '25dm18pitt3td0', true
);

INSERT INTO outbound.member_import_file_data (
    member_import_file_id,
    record_number,
    raw_data_json,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr,
    region_code, member_id, subscriber_mem_nbr_prefix, mem_nbr, subscriber_mem_nbr, mem_nbr_prefix,
    member_type, plan_id, subgroup_id, plan_type, eligibility_start, eligibility_end, last_name,
    middle_name, first_name, dob, gender, email, home_phone_number, mobile_phone,
    home_address_line1, home_address_line2, home_city, home_state, home_postal_code,
    mailing_address_line1, mailing_address_line2, city, mailing_state, postal_code,
    language_code, action, partner_code, age, country, emp_or_dep, mailing_country_code,
    person_unique_identifier, is_sso_user
) VALUES (
    1, -- member_import_file_id
    2, -- record_number
    null, -- raw_data_json
    CURRENT_TIMESTAMP, -- create_ts
    NULL, -- update_ts
    'system', -- create_user
    NULL, -- update_user
    0, -- delete_nbr
    'CO', 5270817, 'smnp', '7670e92a-44c0-4ae6-a2dd-cd388bd92414', '7670e92a-44c0-4ae6-a2dd-cd388bd92414', 'mnp',
'SU', '40513', '51', 'DHMO', '2024-01-01', '2026-12-31', 'Galland', 'A', 'Maxie', '1989-09-07', 'M',
'3mgallandre11@intel.com', '904-437-3360', '543-470-9206', 'PO Box 42628', '93 Weeping Birch Court',
'New York City', 'New York', '14201', '123 Main Street', 'test1', 'Nashua', 'NH', '10004',
'en-US', 'I', 'par-7e92b06aa4fe405198d27d2427bf3de4', 92, 'US', 'E', '840', '2s0511ds9jvkpl', true
);


INSERT INTO outbound.member_import_file_data (
    member_import_file_id,
    record_number,
    raw_data_json,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr,
    region_code, member_id, subscriber_mem_nbr_prefix, mem_nbr, subscriber_mem_nbr, mem_nbr_prefix,
    member_type, plan_id, subgroup_id, plan_type, eligibility_start, eligibility_end, last_name,
    middle_name, first_name, dob, gender, email, home_phone_number, mobile_phone,
    home_address_line1, home_address_line2, home_city, home_state, home_postal_code,
    mailing_address_line1, mailing_address_line2, city, mailing_state, postal_code,
    language_code, action, partner_code, age, country, emp_or_dep, mailing_country_code,
    person_unique_identifier, is_sso_user
) VALUES (
    1, -- member_import_file_id
    3, -- record_number
    null, -- raw_data_json
    CURRENT_TIMESTAMP, -- create_ts
    NULL, -- update_ts
    'system', -- create_user
    NULL, -- update_user
    0, -- delete_nbr
    'CO', 1269214, 'smnp', 'a5283d75-dc7f-4dba-bbae-b87489b34d34', 'a5283d75-dc7f-4dba-bbae-b87489b34d34', 'mnp',
'SU', '40513', '51', 'DHMO', '2024-01-01', '2026-12-31', 'Szreter', 'A', 'Lita', '1982-04-14', 'F',
'31szreter21@squidoo.com', '815-677-5579', '754-794-6239', 'PO Box 32135', '490 North Road',
'Utica', 'New York', '13202', '123 Main Street', 'test1', 'Nashua', 'NH', '90001',
'en-US', 'I', 'par-7e92b06aa4fe405198d27d2427bf3de4', 18, 'US', 'E', '840', '2kdcp63rvyytla', true
);






