-- Insert "Healthy Living" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-35a2d66fc0024dce9a889e46a2e29c01'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync , config_json
        ) VALUES (
            'wat-35a2d66fc0024dce9a889e46a2e29c01', 'Healthy Living', NOW(), 'SYSTEM', 0,
            'Healthy Living', 'HLTHLVNG', false , '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Healthy Living" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Healthy Living" (false) already exists.';
    END IF;
END $$;

-- Insert "Healthy Living" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-a42e0b5cf3df4e0fbd431db58c415cad'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync , config_json
        ) VALUES (
            'wat-a42e0b5cf3df4e0fbd431db58c415cad', 'Healthy Living', NOW(), 'SYSTEM', 0,
            'Healthy Living', 'HLTHLVNG', true , '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Healthy Living" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Healthy Living" (true) already exists.';
    END IF;
END $$;

-- Insert "Healthy Living Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-e5f7c1ddef3d49cbaf33ec63e9ea6b12'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync , config_json
        ) VALUES (
            'wat-e5f7c1ddef3d49cbaf33ec63e9ea6b12', 'Healthy Living Suspense', NOW(), 'SYSTEM', 0,
            'Healthy Living Suspense', 'HLTHLVNG', false ,'{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Healthy Living Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Healthy Living Suspense" already exists.';
    END IF;
END $$;
