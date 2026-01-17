ALTER TABLE admin.batch_job_report
ADD COLUMN IF NOT EXISTS validation_json JSONB NULL;