DO $$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM pg_tables 
        WHERE schemaname = 'etl' 
          AND tablename = 'batch_job_report'
    ) THEN
        EXECUTE 'ALTER TABLE etl.batch_job_report SET SCHEMA admin';
    END IF;
 
    IF EXISTS (
        SELECT 1 
        FROM pg_tables 
        WHERE schemaname = 'etl' 
          AND tablename = 'batch_job_detail_report'
    ) THEN
        EXECUTE 'ALTER TABLE etl.batch_job_detail_report SET SCHEMA admin';
    END IF;
END $$;