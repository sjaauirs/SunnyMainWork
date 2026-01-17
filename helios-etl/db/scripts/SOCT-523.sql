ALTER TABLE etl.member_import_file
ADD COLUMN IF NOT EXISTS "file_status" VARCHAR(50) NOT NULL DEFAULT 'NOT_STARTED';

--update all existing record as Completed
update etl.member_import_file set file_status = 'COMPLETED'