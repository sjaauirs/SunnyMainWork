
-- Check and drop the admin.event_handler_result table if it exists
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables 
        WHERE table_schema = 'admin'
        AND table_name = 'event_handler_result'
    ) THEN
        EXECUTE 'DROP TABLE admin.event_handler_result';
        RAISE NOTICE 'Table admin.event_handler_result dropped successfully.';
    ELSE
        RAISE NOTICE 'Table admin.event_handler_result does not exist.';
    END IF;
END $$;

-- Check and drop the admin.event_handler_script table if it exists
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables 
        WHERE table_schema = 'admin'
        AND table_name = 'event_handler_script'
    ) THEN
        EXECUTE 'DROP TABLE admin.event_handler_script';
        RAISE NOTICE 'Table admin.event_handler_script dropped successfully.';
    ELSE
        RAISE NOTICE 'Table admin.event_handler_script does not exist.';
    END IF;
END $$;
