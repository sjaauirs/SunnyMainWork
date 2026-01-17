BEGIN;

ALTER TABLE etl.member_import_file_data
DROP COLUMN record_processing_status;

COMMIT;