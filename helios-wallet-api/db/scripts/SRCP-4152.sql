DO
$$
BEGIN
    -- Step 1: Add new column if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'wallet'
          AND table_name = 'wallet_type'
          AND column_name = 'config_json'
    ) THEN
        ALTER TABLE wallet.wallet_type
        ADD COLUMN config_json JSONB NOT NULL DEFAULT '{}';
        RAISE NOTICE 'Column config_json added successfully.';
    ELSE
        RAISE NOTICE 'Column config_json already exists. Skipping addition.';
    END IF;
 
    -- Step 2: Update config_json for wallet types with '$' and delete_nbr = 0
    UPDATE wallet.wallet_type
    SET config_json = jsonb_build_object('currency', 'USD')
    WHERE wallet_type_code not in ('wat-c3b091232e974f98aeceb495d2a9f916', 'wat-e2c6076b59db46febd8d76fd019ae0b0')
      AND delete_nbr = 0;
 
    RAISE NOTICE 'Successfully updated config_json to USD for $ wallet types where delete_nbr = 0.';
 
    -- Step 3: Update config_json for sweepstakes wallet types and delete_nbr = 0
    UPDATE wallet.wallet_type
    SET config_json = jsonb_build_object('currency', 'ENTRIES')
    WHERE wallet_type_code in ('wat-c3b091232e974f98aeceb495d2a9f916', 'wat-e2c6076b59db46febd8d76fd019ae0b0')
      AND delete_nbr = 0;
 
    RAISE NOTICE 'Successfully updated config_json to ENTRIES for sweepstakes wallet types where delete_nbr = 0.';
 
END;
$$;