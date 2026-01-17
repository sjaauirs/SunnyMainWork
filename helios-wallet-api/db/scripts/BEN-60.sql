-- =============================================================================
-- Script  : Create wallet types for OTC, Grocery, Copay Assist,
--           DOT, Daily Living Support, and Comprehensive Living Support
-- Jira    : BEN-60
-- Purpose : Define and add new wallet types required for HAP tenant.
-- =============================================================================




--===============================================================================
-- 1) Insert "OTC(Over the Counter)"
--===============================================================================

-- Insert "Over the Counter" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-4b364fg722f04034cv732b355d84f479'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync , config_json
        ) VALUES (
            'wat-4b364fg722f04034cv732b355d84f479', 'Over the Counter', NOW(), 'SYSTEM', 0,
            'OTC', 'OTC', false , '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC" (false) already exists.';
    END IF;
END $$;

-- Insert "Over the Counter" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-4b364ed612f04034bf732b355d84f368'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync , config_json
        ) VALUES (
            'wat-4b364ed612f04034bf732b355d84f368', 'Over the Counter', NOW(), 'SYSTEM', 0,
            'OTC', 'OTC', true , '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC" (true) already exists.';
    END IF;
END $$;

-- Insert "OTC Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-bc8f4f7c028d479f900f0af794e385c8'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync , config_json
        ) VALUES (
            'wat-bc8f4f7c028d479f900f0af794e385c8', 'OTC Suspense', NOW(), 'SYSTEM', 0,
            'OTC Suspense', 'OTC', false ,'{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC Suspense" already exists.';
    END IF;
END $$;

--===============================================================================
-- 2) Insert "OTC and Grocery"
--===============================================================================

-- Insert "OTC and Grocery" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-3509a5788e5246b18221582031cd10a3'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-3509a5788e5246b18221582031cd10a3', 'OTC and Grocery', NOW(), 'SYSTEM', 0,
            'OTC and Grocery', 'CFO', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC and Grocery" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC and Grocery" (false) already exists.';
    END IF;
END $$;

-- Insert "OTC and Grocery" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-bb06d4c12ac84213bc59bc2093421264'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-bb06d4c12ac84213bc59bc2093421264', 'OTC and Grocery', NOW(), 'SYSTEM', 0,
            'OTC and Grocery', 'CFO', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC and Grocery" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC and Grocery" (true) already exists.';
    END IF;
END $$;

-- Insert "OTC and Grocery Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-7502f7583a414a53b5bba944a58aeec9'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-7502f7583a414a53b5bba944a58aeec9', 'OTC and Grocery Suspense', NOW(), 'SYSTEM', 0,
            'OTC and Grocery Suspense', 'CFO', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC and Grocery Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC and Grocery Suspense" already exists.';
    END IF;
END $$;


--===============================================================================
-- 3) Insert "OTC and Copay Assist"
--===============================================================================

-- Insert "OTC and Copay Assist" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-14cfd51de64c46e4b927a7e8984474ea'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-14cfd51de64c46e4b927a7e8984474ea', 'OTC and Copay Assist', NOW(), 'SYSTEM', 0,
            'OTC and Copay Assist', 'OCP', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC and Copay Assist" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC and Copay Assist" (false) already exists.';
    END IF;
END $$;

-- Insert "OTC and Copay Assist" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-7be788fb5115443eb0ead237b6c46cc4'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-7be788fb5115443eb0ead237b6c46cc4', 'OTC and Copay Assist', NOW(), 'SYSTEM', 0,
            'OTC and Copay Assist', 'OCP', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC and Copay Assist" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC and Copay Assist" (true) already exists.';
    END IF;
END $$;

-- Insert "OTC and Copay Assist Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-1f1b825b43dd4d42a560606247ab26f8'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-1f1b825b43dd4d42a560606247ab26f8', 'OTC and Copay Assist Suspense', NOW(), 'SYSTEM', 0,
            'OTC and Copay Assist Suspense', 'OCP', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC and Copay Assist Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC and Copay Assist Suspense" already exists.';
    END IF;
END $$;


--===============================================================================
-- 4) Insert "OTC, Grocery and Copay Assist"
--===============================================================================

-- Insert "OTC, Grocery and Copay Assist" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-2422da2eb57b4a2c9acb24e4d593fba7'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-2422da2eb57b4a2c9acb24e4d593fba7', 'OTC, Grocery and Copay Assist', NOW(), 'SYSTEM', 0,
            'OTC, Grocery and Copay Assist', 'OGP', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC, Grocery and Copay Assist" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC, Grocery and Copay Assist" (false) already exists.';
    END IF;
END $$;

-- Insert "OTC, Grocery and Copay Assist" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-aca6aa177739432980e094b86567db7d'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-aca6aa177739432980e094b86567db7d', 'OTC, Grocery and Copay Assist', NOW(), 'SYSTEM', 0,
            'OTC, Grocery and Copay Assist', 'OGP', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC, Grocery and Copay Assist" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC, Grocery and Copay Assist" (true) already exists.';
    END IF;
END $$;

-- Insert "OTC, Grocery and Copay Assist Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-20bbb6af4d194fd5954ccfe955ee5bfb'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-20bbb6af4d194fd5954ccfe955ee5bfb', 'OTC, Grocery and Copay Assist Suspense', NOW(), 'SYSTEM', 0,
            'OTC, Grocery and Copay Assist Suspense', 'OGP', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "OTC, Grocery and Copay Assist Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "OTC, Grocery and Copay Assist Suspense" already exists.';
    END IF;
END $$;

--===============================================================================
-- 5) Insert "DOT (OTC, Dental, Vision, Hearing, and Transportation)"
--===============================================================================

-- Insert "DOT" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-7ab9caa63bb14a6093649fbf3b97b0b4'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-7ab9caa63bb14a6093649fbf3b97b0b4', 'DOT (OTC, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'DOT', 'DOT', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "DOT" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "DOT" (false) already exists.';
    END IF;
END $$;

-- Insert "DOT" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-5b0d5378af774c0381b67ed3e77d2fdd'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-5b0d5378af774c0381b67ed3e77d2fdd', 'DOT (OTC, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'DOT', 'DOT', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "DOT" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "DOT" (true) already exists.';
    END IF;
END $$;

-- Insert "DOT Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-4d931cf47a46485d94fd7d80051a7749'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-4d931cf47a46485d94fd7d80051a7749', 'DOT Suspense (OTC, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'DOT Suspense', 'DOT', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "DOT Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "DOT Suspense" already exists.';
    END IF;
END $$;


--===============================================================================
-- 6) Insert "UGT (OTC, Dental, Vision, Hearing, and Transportation)"
--===============================================================================

-- Insert "UGT" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-49812db3d9814dbca8eae2eba91722af'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-49812db3d9814dbca8eae2eba91722af', 'UGT (OTC, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'UGT', 'UGT', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "UGT" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "UGT" (false) already exists.';
    END IF;
END $$;

-- Insert "UGT" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-e207db6a8a0a460fbe852ce9c3fcbd54'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-e207db6a8a0a460fbe852ce9c3fcbd54', 'UGT (OTC, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'UGT', 'UGT', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "UGT" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "UGT" (true) already exists.';
    END IF;
END $$;

-- Insert "UGT Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-3274e7cf318f4ba3a61228112d60229f'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-3274e7cf318f4ba3a61228112d60229f', 'UGT Suspense (OTC, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'UGT Suspense', 'UGT', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "UGT Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "UGT Suspense" already exists.';
    END IF;
END $$;



--===============================================================================
-- 7) Insert "DOT with Grocery (OTC, Grocery, Dental, Vision, Hearing, and Transportation)"
--===============================================================================

-- Insert "DOT with Grocery" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-2ea762719bac47349aac36e7b2ade583'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-2ea762719bac47349aac36e7b2ade583', 'DOT with Grocery (OTC, Grocery, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'DOT with Grocery', 'OGT', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "DOT with Grocery" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "DOT with Grocery" (false) already exists.';
    END IF;
END $$;

-- Insert "DOT with Grocery" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-c583162a9130457289a09e28daaedc2e'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-c583162a9130457289a09e28daaedc2e', 'DOT with Grocery (OTC, Grocery, Dental, Vision, Hearing, and Transportation)', NOW(), 'SYSTEM', 0,
            'DOT with Grocery', 'OGT', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "DOT with Grocery" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "DOT with Grocery" (true) already exists.';
    END IF;
END $$;

-- Insert "DOT with Grocery Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-6b61d3cab56c4e98b56da9f157e3133b'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-6b61d3cab56c4e98b56da9f157e3133b', 'DOT with Grocery Suspense', NOW(), 'SYSTEM', 0,
            'DOT with Grocery Suspense', 'OGT', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "DOT with Grocery Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "DOT with Grocery Suspense" already exists.';
    END IF;
END $$;


--===============================================================================
-- 8) Insert "Daily Living Support" (OTC, Grocery, Home Modifications, Utilities, Pay at the Pump/Rideshare, Pest Control)
--===============================================================================

-- Insert "Daily Living Support" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-44b999834ec344c88c1f6fdbeb401626'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-44b999834ec344c88c1f6fdbeb401626', 'Daily Living Support', NOW(), 'SYSTEM', 0,
            'Daily Living Support', 'HFO', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Daily Living Support" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Daily Living Support" (false) already exists.';
    END IF;
END $$;

-- Insert "Daily Living Support" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-4fe0417bda474f7baa0e344b5c132778'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-4fe0417bda474f7baa0e344b5c132778', 'Daily Living Support', NOW(), 'SYSTEM', 0,
            'Daily Living Support', 'HFO', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Daily Living Support" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Daily Living Support" (true) already exists.';
    END IF;
END $$;

-- Insert "Daily Living Support Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-af38a931039b45bb938b39f60b6bd697'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-af38a931039b45bb938b39f60b6bd697', 'Daily Living Support Suspense', NOW(), 'SYSTEM', 0,
            'Daily Living Support Suspense', 'HFO', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Daily Living Support Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Daily Living Support Suspense" already exists.';
    END IF;
END $$;


--===============================================================================
-- 9) Insert "Comprehensive Living Support" (OTC, Grocery, Copay Assist, Home Modifications, Utilities, Pay at the Pump/Rideshare, Pest Control)
--===============================================================================

-- Insert "Comprehensive Living Support" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-fd76b4c2afad4eafae53d4c7dfc3dc84'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-fd76b4c2afad4eafae53d4c7dfc3dc84', 'Comprehensive Living Support', NOW(), 'SYSTEM', 0,
            'Comprehensive Living Support', 'HFC', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Comprehensive Living Support" (false) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Comprehensive Living Support" (false) already exists.';
    END IF;
END $$;

-- Insert "Comprehensive Living Support" (is_external_sync = true)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-98c4dcf5510047fe88c238e1fc35f0fa'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-98c4dcf5510047fe88c238e1fc35f0fa', 'Comprehensive Living Support', NOW(), 'SYSTEM', 0,
            'Comprehensive Living Support', 'HFC', true, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Comprehensive Living Support" (true) inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Comprehensive Living Support" (true) already exists.';
    END IF;
END $$;

-- Insert "Comprehensive Living Support Suspense" (is_external_sync = false)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM wallet.wallet_type 
        WHERE wallet_type_code = 'wat-0aa152d8533d454db8faf62d3e87d5e8'
    ) THEN
        INSERT INTO wallet.wallet_type (
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync, config_json
        ) VALUES (
            'wat-0aa152d8533d454db8faf62d3e87d5e8', 'Comprehensive Living Support Suspense', NOW(), 'SYSTEM', 0,
            'Comprehensive Living Support Suspense', 'HFC', false, '{"currency": "USD"}'
        );
        RAISE NOTICE 'Wallet type "Comprehensive Living Support Suspense" inserted.';
    ELSE
        RAISE NOTICE 'Wallet type "Comprehensive Living Support Suspense" already exists.';
    END IF;
END $$;