-- script to add new column terms_of_service_code 

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'task' 
          AND table_name = 'terms_of_service' 
          AND column_name = 'terms_of_service_code'
    ) THEN
        -- Log: Column does not exist, proceeding to add
        RAISE NOTICE 'Column "terms_of_service_code" does not exist. Adding column...';

        ALTER TABLE task.terms_of_service
        ADD COLUMN terms_of_service_code VARCHAR(50) NOT NULL DEFAULT 'DEFAULT_CODE';

        -- Log: Column added successfully
        RAISE NOTICE 'Column "terms_of_service_code" added successfully with default value "DEFAULT_CODE".';
    ELSE
        -- Log: Column already exists
        RAISE NOTICE 'Column "terms_of_service_code" already exists. Skipping addition.';
    END IF;
	
    RAISE NOTICE 'Updating terms_of_service_code where it is set to DEFAULT_CODE...';

    UPDATE task.terms_of_service
    SET terms_of_service_code = 'tos-' || REPLACE(gen_random_uuid()::text, '-', '')
    FROM (
        SELECT terms_of_service_id, gen_random_uuid() AS new_uuid
        FROM task.terms_of_service
        WHERE terms_of_service_code = 'DEFAULT_CODE'
    ) AS u
    WHERE task.terms_of_service.terms_of_service_id = u.terms_of_service_id;

    RAISE NOTICE 'Update completed.';
    -- Log: End of the block
    RAISE NOTICE 'Script execution completed.';
END
$$;

