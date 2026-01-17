BEGIN;

ALTER TABLE etl.member_import_file_data
ADD COLUMN record_processing_status BIGINT NOT NULL DEFAULT 0;

COMMIT;