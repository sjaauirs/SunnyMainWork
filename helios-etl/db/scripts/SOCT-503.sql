DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables 
        WHERE table_schema = 'etl' 
          AND table_name = 'redshift_sync_status'
    ) THEN
        CREATE TABLE etl.redshift_sync_status (
            redshift_sync_status_id BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
            last_loaded_at TIMESTAMP,
            records_processed INT DEFAULT 0,
            error_message TEXT,
            data_type VARCHAR(250) NOT NULL,
            create_ts TIMESTAMP NOT NULL,
            update_ts TIMESTAMP,
            create_user VARCHAR(50) NOT NULL,
            update_user VARCHAR(50),
            delete_nbr BIGINT NOT NULL
        );
        RAISE NOTICE '✅ Table etl.redshift_sync_status created successfully.';
    ELSE
        RAISE NOTICE 'ℹ️ Table etl.redshift_sync_status already exists. Skipping creation.';
    END IF;
 
    -- Proceed to grant privileges only if table exists
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables 
        WHERE table_schema = 'etl' 
          AND table_name = 'redshift_sync_status'
    ) THEN
        GRANT ALL ON TABLE etl.redshift_sync_status TO hadminusr;
        GRANT DELETE, INSERT, UPDATE, SELECT ON TABLE etl.redshift_sync_status TO happusr;
        GRANT SELECT ON TABLE etl.redshift_sync_status TO hrousr;
        RAISE NOTICE '✅ Privileges granted on etl.redshift_sync_status.';
    END IF;
END
$$;
