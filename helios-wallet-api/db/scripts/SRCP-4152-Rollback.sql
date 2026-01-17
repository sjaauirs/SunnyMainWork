DO $$ 
DECLARE 
    column_exists BOOLEAN;
BEGIN
    -- Check if the column exists
    SELECT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'wallet'
          AND table_name = 'wallet_type'
          AND column_name = 'config_json'
    ) INTO column_exists;

    -- Drop column if it exists
    IF column_exists THEN
        ALTER TABLE wallet.wallet_type 
        DROP COLUMN config_json;
        RAISE NOTICE 'Column config_json dropped successfully from task.wallet_type.';
    ELSE
        RAISE NOTICE 'Column config_json does not exist in wallet.wallet_type.';
    END IF;
END $$;