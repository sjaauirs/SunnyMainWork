-- Remove column from huser.consumer_login if it exists
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer_login'
          AND column_name = 'token_app'
    ) THEN
        EXECUTE 'ALTER TABLE huser.consumer_login DROP COLUMN token_app';
    END IF;
END
$$;
