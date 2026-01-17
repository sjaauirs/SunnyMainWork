DO $$
BEGIN
    RAISE NOTICE '--- Starting rollback for huser.consumer.agreement_file_name ---';

    -- Step 1: Check if column exists and is JSON/JSONB
    IF EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'huser'
          AND table_name = 'consumer'
          AND column_name = 'agreement_file_name'
          AND data_type IN ('json', 'jsonb')
    ) THEN
        RAISE NOTICE 'Column is JSONB — proceeding with rollback.';

        -- Step 2: Update JSON values to plain text (keep only value)
        UPDATE huser.consumer c
        SET agreement_file_name = (
            SELECT value
            FROM jsonb_each_text(c.agreement_file_name)
            LIMIT 1
        )
        WHERE c.agreement_file_name IS NOT NULL
          AND jsonb_typeof(c.agreement_file_name) = 'object';

        -- Step 3: Convert column back to VARCHAR(255)
        ALTER TABLE huser.consumer
        ALTER COLUMN agreement_file_name TYPE character varying(255)
        COLLATE pg_catalog."default"
        USING agreement_file_name::TEXT;

        RAISE NOTICE 'Column reverted to character varying(255) successfully.';

    ELSE
        RAISE NOTICE 'Column is not JSONB — rollback not required.';
    END IF;

    RAISE NOTICE '--- Rollback completed successfully ---';
END $$;
