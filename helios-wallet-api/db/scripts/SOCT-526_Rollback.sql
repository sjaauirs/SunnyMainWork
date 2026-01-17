-- Rollback: Delete "Healthy Living" (is_external_sync = false)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-35a2d66fc0024dce9a889e46a2e29c01'
    ) THEN
        DELETE FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-35a2d66fc0024dce9a889e46a2e29c01';
        RAISE NOTICE 'Rolled back wallet type "Healthy Living" (false).';
    ELSE
        RAISE NOTICE 'Wallet type "Healthy Living" (false) not found.';
    END IF;
END $$;

-- Rollback: Delete "Healthy Living" (is_external_sync = true)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-a42e0b5cf3df4e0fbd431db58c415cad'
    ) THEN
        DELETE FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-a42e0b5cf3df4e0fbd431db58c415cad';
        RAISE NOTICE 'Rolled back wallet type "Healthy Living" (true).';
    ELSE
        RAISE NOTICE 'Wallet type "Healthy Living" (true) not found.';
    END IF;
END $$;

-- Rollback: Delete "Healthy Living Suspense"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-e5f7c1ddef3d49cbaf33ec63e9ea6b12'
    ) THEN
        DELETE FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-e5f7c1ddef3d49cbaf33ec63e9ea6b12';
        RAISE NOTICE 'Rolled back wallet type "Healthy Living Suspense".';
    ELSE
        RAISE NOTICE 'Wallet type "Healthy Living Suspense" not found.';
    END IF;
END $$;
