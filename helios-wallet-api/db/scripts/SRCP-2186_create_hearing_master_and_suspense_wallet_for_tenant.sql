-- Check and create the "Hearing" wallet type
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-29d568uiy2154735934d6ty8afac2edf' -- Specify your wallet type here 
    ) THEN
        RAISE NOTICE 'Wallet type "Hearing" already exists.';
    ELSE
        INSERT INTO wallet.wallet_type(
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync
        )
        VALUES (
            'wat-29d568uiy2154735934d6ty8afac2edf', 'Hearing', NOW(), 'per-915325069cdb42c783dd4601e1d27704', 0,
            'Hearing', 'Hearing', false
        );
        RAISE NOTICE 'Wallet type "Hearing" created successfully.';
    END IF;
END $$;

-- Create "Hearing" Benefits Master Funding Wallet for Tenant
DO $$
DECLARE
    wallet_type_code_value varchar(50) := 'wat-29d568uiy2154735934d6ty8afac2edf'; -- Specify your wallet type here 
    tenant_code_value varchar(50) := 'ten-ecada21e57154928a2bb959e8365b8b4';  -- Specify your tenant code here 
    wallet_name_value varchar(80) := 'HEARING'; -- Specify your wallet name here 
    balance_value numeric(14,2) := 1000000000; -- Specify your wallet balance here 
    master_wallet_value boolean := true; -- Specify your master wallet here 
    wallet_type_id_value int; 
    current_year int;
    active_end_ts_value timestamp;
BEGIN 
    -- Fetch wallet type id
    SELECT wallet_type_id INTO wallet_type_id_value  
    FROM wallet.wallet_type  
    WHERE wallet_type_code = wallet_type_code_value;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Wallet type % not found', wallet_type_code_value;
    END IF;

    -- Calculate current year
    SELECT EXTRACT(YEAR FROM CURRENT_TIMESTAMP) INTO current_year;

    -- Calculate active end timestamp
    active_end_ts_value := TO_TIMESTAMP(current_year || '-12-31 23:59:59', 'YYYY-MM-DD HH24:MI:SS');

    -- Create wallet for the given tenant
    RAISE NOTICE 'Started creating wallet for tenant_code: %, wallet_type: %, wallet_name: %', tenant_code_value, wallet_type_code_value, wallet_name_value;
    INSERT INTO wallet.wallet (wallet_type_id, customer_code, sponsor_code, tenant_code, wallet_code, wallet_name, active_start_ts, active_end_ts, balance, earn_maximum, create_ts, create_user, delete_nbr, total_earned, master_wallet, active)
    SELECT wallet_type_id_value,
           'cus-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''),
           NULL,
           tenant_code_value,
           'wal-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''),
           wallet_name_value,
           NOW(), -- active_start_ts
           active_end_ts_value, -- active_end_ts
           balance_value, -- Initial balance
           0, -- earn_maximum
           NOW(), -- create_ts
           'SYSTEM', -- create_user
           0, -- delete_nbr
           0, -- total_earned
           master_wallet_value, -- master_wallet
           true -- active
    WHERE NOT EXISTS (
        SELECT 1 FROM wallet.wallet 
        WHERE wallet_type_id = wallet_type_id_value
            AND tenant_code = tenant_code_value
            AND wallet_name = wallet_name_value
            AND master_wallet = master_wallet_value
    );

    IF FOUND THEN
        RAISE NOTICE 'Successfully created wallet for tenant_code: %, wallet_type: %, wallet_name: %', tenant_code_value, wallet_type_code_value, wallet_name_value;
    ELSE
        RAISE NOTICE 'Wallet already exists, tenant_code: %, wallet_type: %, wallet_name: %', tenant_code_value, wallet_type_code_value, wallet_name_value;
    END IF;

END $$;

-- Check and create the "SUSPENSE_WALLET" wallet type
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-bc8f4f7c028d479f900f0af794e385c8' -- Specify your wallet type here 
    ) THEN
        RAISE NOTICE 'Wallet type "SUSPENSE_WALLET" already exists.';
    ELSE
        INSERT INTO wallet.wallet_type(
            wallet_type_code, wallet_type_name, create_ts, create_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync
        )
        VALUES (
            'wat-bc8f4f7c028d479f900f0af794e385c8', 'SUSPENSE_WALLET', NOW(), 'per-915325069cdb42c783dd4601e1d27704', 0,
            'SUSPENSE', 'SUSPENSE', false
        );
        RAISE NOTICE 'Wallet type "SUSPENSE_WALLET" created successfully.';
    END IF;
END $$;

-- Create TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_HEARING Benefits Master Funding Wallet for Tenant
DO $$
DECLARE
    wallet_type_code_value varchar(50) := 'wat-bc8f4f7c028d479f900f0af794e385c8'; -- Specify your wallet type here 
    tenant_code_value varchar(50) := 'ten-ecada21e57154928a2bb959e8365b8b4';  -- Specify your tenant code here 
    wallet_name_value varchar(80) := 'TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_HEARING'; -- Specify your wallet name here 
    balance_value numeric(14,2) := 0.0; -- Specify your wallet balance here 
    master_wallet_value boolean := true; -- Specify your master wallet here 
    wallet_type_id_value int; 
    current_year int;
    active_end_ts_value timestamp;
BEGIN 
    -- Fetch wallet type id
    SELECT wallet_type_id INTO wallet_type_id_value  
    FROM wallet.wallet_type  
    WHERE wallet_type_code = wallet_type_code_value;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Wallet type % not found', wallet_type_code_value;
    END IF;

    -- Calculate current year
    SELECT EXTRACT(YEAR FROM CURRENT_TIMESTAMP) INTO current_year;

    -- Calculate active end timestamp
    active_end_ts_value := TO_TIMESTAMP(current_year || '-12-31 23:59:59', 'YYYY-MM-DD HH24:MI:SS');

    -- Create wallet for the given tenant
    RAISE NOTICE 'Started creating wallet for tenant_code: %, wallet_type: %, wallet_name: %', tenant_code_value, wallet_type_code_value, wallet_name_value;
    INSERT INTO wallet.wallet (wallet_type_id, customer_code, sponsor_code, tenant_code, wallet_code, wallet_name, active_start_ts, active_end_ts, balance, earn_maximum, create_ts, create_user, delete_nbr, total_earned, master_wallet, active)
    SELECT wallet_type_id_value,
           'cus-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''),
           NULL,
           tenant_code_value,
           'wal-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''),
           wallet_name_value,
           NOW(), -- active_start_ts
           active_end_ts_value, -- active_end_ts
           balance_value, -- Initial balance
           0, -- earn_maximum
           NOW(), -- create_ts
           'SYSTEM', -- create_user
           0, -- delete_nbr
           0, -- total_earned
           master_wallet_value, -- master_wallet
           true -- active
    WHERE NOT EXISTS (
        SELECT 1 FROM wallet.wallet 
        WHERE wallet_type_id = wallet_type_id_value
            AND tenant_code = tenant_code_value
            AND wallet_name = wallet_name_value
            AND master_wallet = master_wallet_value
    );

    IF FOUND THEN
        RAISE NOTICE 'Successfully created wallet for tenant_code: %, wallet_type: %, wallet_name: %', tenant_code_value, wallet_type_code_value, wallet_name_value;
    ELSE
        RAISE NOTICE 'Wallet already exists, tenant_code: %, wallet_type: %, wallet_name: %', tenant_code_value, wallet_type_code_value, wallet_name_value;
    END IF;

END $$;
