-- Rollback: Delete inserted sample data from outbound schema (if applicable)
-- NOTE: These deletions assume no other data existed before the inserts

DELETE FROM outbound.member_import_file_data
WHERE member_import_file_id = 1
  AND record_number IN (1, 2, 3, 4)
  AND create_user = 'system_user';

DELETE FROM outbound.member_import_file
WHERE member_import_code = 'mic-2b1894a514044bd2b85cf9d5642fc322'
  AND file_name = 'members_2025_05_26.txt'
  AND create_user = 'system_user';

-- Rollback: Drop Redshift tables from etl schema

DROP TABLE IF EXISTS outbound.member_import_file_data;

DROP TABLE IF EXISTS outbound.member_import_file;
