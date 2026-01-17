-- ROllback script to drop terms_of_service_code

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'task'
          AND table_name = 'terms_of_service'
          AND column_name = 'terms_of_service_code'
    ) THEN
        -- Log: Column exists, proceeding to drop
        RAISE NOTICE 'Column "terms_of_service_code" exists. Dropping column...';

        ALTER TABLE task.terms_of_service
        DROP COLUMN terms_of_service_code;

        -- Log: Column dropped
        RAISE NOTICE 'Column "terms_of_service_code" dropped successfully.';
    ELSE
        -- Log: Column does not exist
        RAISE NOTICE 'Column "terms_of_service_code" does not exist. No action needed.';
    END IF;

    -- Log: Completion
    RAISE NOTICE 'Script execution completed.';
END
$$;


-- Rollback script to update terms_of_service_code to default value
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE task.terms_of_service
    SET terms_of_service_code = 'DEFAULT_CODE'
    WHERE terms_of_service_code LIKE 'tos-%'
    RETURNING 1 INTO updated_count;

    RAISE NOTICE 'Updated % rows to DEFAULT_CODE.', updated_count;
END
$$;
