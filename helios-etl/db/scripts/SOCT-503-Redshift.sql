--Redshift table creation
CREATE TABLE IF NOT EXISTS outbound.member_import_file
(
    member_import_file_id BIGINT IDENTITY(1,1) NOT NULL,
    member_import_code VARCHAR(50) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    create_ts TIMESTAMP NOT NULL,
    update_ts TIMESTAMP,
    create_user VARCHAR(50) NOT NULL,
    update_user VARCHAR(50),
    delete_nbr BIGINT NOT NULL,
    PRIMARY KEY (member_import_file_id)
)
DISTSTYLE KEY
DISTKEY(member_import_file_id)
SORTKEY(create_ts);


CREATE TABLE IF NOT EXISTS outbound.member_import_file_data
(
    member_import_file_data_id BIGINT IDENTITY(1,1) NOT NULL,
    member_import_file_id BIGINT NOT NULL,
    record_number INTEGER NOT NULL,
    raw_data_json VARCHAR(MAX) NOT NULL, -- JSONB is not supported in Redshift
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

-- Sample data creation
INSERT INTO outbound.member_import_file (
    member_import_code,
    file_name,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr
)
VALUES (
    'mic-2b1894a514044bd2b85cf9d5642fc322',           -- member_import_code
    'members_2025_05_26.txt',   -- file_name
    CURRENT_TIMESTAMP,          -- create_ts
    NULL,                       -- update_ts
    'system_user',              -- create_user
    NULL,                       -- update_user
    0                           -- delete_nbr
);


INSERT INTO outbound.member_import_file_data (
    member_import_file_id,
    record_number,
    raw_data_json,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr
)
VALUES (
    1,                                      -- member_import_file_id (assumes FK exists)
    1,                                      -- record_number
    '{
  "age": "90",
  "dob": "11/25/1983",
  "city": "Nashua",
  "email": "test_redshift101@yopmail.com",
  "action": "I",
  "gender": "F",
  "country": "US",
  "mem_nbr": "cd0667e9c47947988b15d8527bcf7e9c",
  "plan_id": "40513",
  "home_city": "Rochester",
  "last_name": "Sictornes",
  "member_id": "6608365",
  "plan_type": "DHMO",
  "emp_or_dep": "E",
  "first_name": "Daniella",
  "home_state": "New York",
  "is_sso_user": true,
  "member_type": "SU",
  "middle_name": "A",
  "postal_code": "10001",
  "region_code": "CO",
  "subgroup_id": "51",
  "mobile_phone": "642-846-0076",
  "partner_code": "par-7e92b06aa4fe405198d27d2427bf3de4",
  "language_code": "en-US",
  "mailing_state": "NH",
  "mem_nbr_prefix": "mnp",
  "eligibility_end": "12/31/2026",
  "home_postal_code": "13202",
  "eligibility_start": "01/01/2024",
  "home_phone_number": "886-397-0380",
  "home_address_line1": "Suite 28",
  "home_address_line2": "440 Schiller Parkway",
  "subscriber_mem_nbr": "cd0667e9c47947988b15d8527bcf7e9c",
  "mailing_country_code": "840",
  "mailing_address_line1": "123 Main Street",
  "mailing_address_line2": "test1",
  "person_unique_identifier": "9fuqgezuly3123",
  "subscriber_mem_nbr_prefix": "smnp"
}', -- raw_data_json as string
    CURRENT_TIMESTAMP,                      -- create_ts
    NULL,                                   -- update_ts
    'system_user',                          -- create_user
    NULL,                                   -- update_user
    0                                       -- delete_nbr
);


INSERT INTO outbound.member_import_file_data (
    member_import_file_id,
    record_number,
    raw_data_json,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr
)
VALUES (
    1,                                      -- member_import_file_id (assumes FK exists)
    2,                                      -- record_number
    '{
  "age": "90",
  "dob": "11/25/1983",
  "city": "Nashua",
  "email": "test_redshift102@yopmail.com",
  "action": "I",
  "gender": "F",
  "country": "US",
  "mem_nbr": "4bb0ee6266e047c6b87f861f25dcbf22",
  "plan_id": "40513",
  "home_city": "Rochester",
  "last_name": "Sictornes2",
  "member_id": "6608365",
  "plan_type": "DHMO",
  "emp_or_dep": "E",
  "first_name": "Daniella2",
  "home_state": "New York",
  "is_sso_user": true,
  "member_type": "SU",
  "middle_name": "A",
  "postal_code": "10001",
  "region_code": "CO",
  "subgroup_id": "51",
  "mobile_phone": "642-846-0076",
  "partner_code": "par-7e92b06aa4fe405198d27d2427bf3de4",
  "language_code": "en-US",
  "mailing_state": "NH",
  "mem_nbr_prefix": "mnp",
  "eligibility_end": "12/31/2026",
  "home_postal_code": "13202",
  "eligibility_start": "01/01/2024",
  "home_phone_number": "886-397-0380",
  "home_address_line1": "Suite 28",
  "home_address_line2": "440 Schiller Parkway",
  "subscriber_mem_nbr": "4bb0ee6266e047c6b87f861f25dcbf22",
  "mailing_country_code": "840",
  "mailing_address_line1": "123 Main Street",
  "mailing_address_line2": "test1",
  "person_unique_identifier": "ad8a78a4e8a046abaa1b49157e5e6d53",
  "subscriber_mem_nbr_prefix": "smnp"
}', -- raw_data_json as string
    CURRENT_TIMESTAMP,                      -- create_ts
    NULL,                                   -- update_ts
    'system_user',                          -- create_user
    NULL,                                   -- update_user
    0                                       -- delete_nbr
);


INSERT INTO outbound.member_import_file_data (
    member_import_file_id,
    record_number,
    raw_data_json,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr
)
VALUES (
    1,                                      -- member_import_file_id (assumes FK exists)
    3,                                      -- record_number
    '{
  "age": "90",
  "dob": "11/25/1983",
  "city": "Nashua",
  "email": "test_redshift103@yopmail.com",
  "action": "I",
  "gender": "F",
  "country": "US",
  "mem_nbr": "d6123e5a3cf443a597b30c713c242d15",
  "plan_id": "40513",
  "home_city": "Rochester",
  "last_name": "Sictornes3",
  "member_id": "6608365",
  "plan_type": "DHMO",
  "emp_or_dep": "E",
  "first_name": "Daniella3",
  "home_state": "New York",
  "is_sso_user": true,
  "member_type": "SU",
  "middle_name": "A",
  "postal_code": "10001",
  "region_code": "CO",
  "subgroup_id": "51",
  "mobile_phone": "642-846-0076",
  "partner_code": "par-7e92b06aa4fe405198d27d2427bf3de4",
  "language_code": "en-US",
  "mailing_state": "NH",
  "mem_nbr_prefix": "mnp",
  "eligibility_end": "12/31/2026",
  "home_postal_code": "13202",
  "eligibility_start": "01/01/2024",
  "home_phone_number": "886-397-0380",
  "home_address_line1": "Suite 28",
  "home_address_line2": "440 Schiller Parkway",
  "subscriber_mem_nbr": "d6123e5a3cf443a597b30c713c242d15",
  "mailing_country_code": "840",
  "mailing_address_line1": "123 Main Street",
  "mailing_address_line2": "test1",
  "person_unique_identifier": "da40e25b27794079b1235a01998c60dd",
  "subscriber_mem_nbr_prefix": "smnp"
}', -- raw_data_json as string
    CURRENT_TIMESTAMP,                      -- create_ts
    NULL,                                   -- update_ts
    'system_user',                          -- create_user
    NULL,                                   -- update_user
    0                                       -- delete_nbr
);

INSERT INTO outbound.member_import_file_data (
    member_import_file_id,
    record_number,
    raw_data_json,
    create_ts,
    update_ts,
    create_user,
    update_user,
    delete_nbr
)
VALUES (
    1,                                      -- member_import_file_id (assumes FK exists)
    4,                                      -- record_number
    '{
  "age": "90",
  "dob": "11/25/1983",
  "city": "Nashua",
  "email": "test_redshift104@yopmail.com",
  "action": "I",
  "gender": "F",
  "country": "US",
  "mem_nbr": "ce90e0314ec344baac392001e4c43fdb",
  "plan_id": "40513",
  "home_city": "Rochester",
  "last_name": "Sictornes4",
  "member_id": "6608365",
  "plan_type": "DHMO",
  "emp_or_dep": "E",
  "first_name": "Daniella4",
  "home_state": "New York",
  "is_sso_user": true,
  "member_type": "SU",
  "middle_name": "A",
  "postal_code": "10001",
  "region_code": "CO",
  "subgroup_id": "51",
  "mobile_phone": "642-846-0076",
  "partner_code": "par-7e92b06aa4fe405198d27d2427bf3de4",
  "language_code": "en-US",
  "mailing_state": "NH",
  "mem_nbr_prefix": "mnp",
  "eligibility_end": "12/31/2026",
  "home_postal_code": "13202",
  "eligibility_start": "01/01/2024",
  "home_phone_number": "886-397-0380",
  "home_address_line1": "Suite 28",
  "home_address_line2": "440 Schiller Parkway",
  "subscriber_mem_nbr": "ce90e0314ec344baac392001e4c43fdb",
  "mailing_country_code": "840",
  "mailing_address_line1": "123 Main Street",
  "mailing_address_line2": "test1",
  "person_unique_identifier": "67f3825983c2484f8eb51994574fba0b",
  "subscriber_mem_nbr_prefix": "smnp"
}', -- raw_data_json as string
    CURRENT_TIMESTAMP,                      -- create_ts
    NULL,                                   -- update_ts
    'system_user',                          -- create_user
    NULL,                                   -- update_user
    0                                       -- delete_nbr
);