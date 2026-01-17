
-- Add column to huser.consumer_login if it does not exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'huser'
          AND table_name = 'consumer_login'
          AND column_name = 'token_app'
    ) THEN
        EXECUTE 'ALTER TABLE huser.consumer_login ADD COLUMN token_app VARCHAR(50)';
    END IF;
END
$$;