-- Check and drop the huser.consumer_activity table if it exists

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables 
        WHERE table_schema = 'huser'
        AND table_name = 'consumer_activity'
    ) THEN
        EXECUTE 'DROP TABLE huser.consumer_activity';
        RAISE NOTICE 'Table huser.consumer_activity dropped successfully.';
    ELSE
        RAISE NOTICE 'Table huser.consumer_activity does not exist.';
    END IF;
END $$;
