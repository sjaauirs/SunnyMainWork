-- Check and create the OTC_BENEFITS_MASTER wallet type
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM wallet.wallet_type WHERE wallet_type_code = 'wat-4b364fg722f04034cv732b355d84f479'
    ) THEN
        RAISE NOTICE 'Wallet type "Over the Counter" already exists.';
    ELSE
        INSERT INTO wallet.wallet_type(
            wallet_type_code, wallet_type_name, create_ts, update_ts, create_user, update_user, delete_nbr,
            wallet_type_label, short_label, is_external_sync
        )
        VALUES (
            'wat-4b364fg722f04034cv732b355d84f479', 'Over the Counter', 
            '2024-01-31 15:50:01.065699', null, 'per-915325069cdb42c783dd4601e1d27704', null, 0,
            'OTC', 'OTC', false
        );
        RAISE NOTICE 'Wallet type "Over the Counter" created successfully.';
    END IF;
END $$;


-- OTC Funding Wallet for some test consumers
DO $$
DECLARE
    -- Declare variables for wallet creation
    wallet_type_code_value varchar(50) := 'wat-4b364fg722f04034cv732b355d84f479'; -- Specify your wallet type here 
    tenant_code_value varchar(50) := 'ten-ecada21e57154928a2bb959e8365b8b4';  -- Specify your tenant code here 
    wallet_name_value varchar(80) := 'OTC_BENEFITS'; -- Specify your wallet name here 
    consumer_code_list varchar(255) := 'cus-7b2199bea85a4ae7abb6d15ac0d59541';  -- Specify your consumer codes here as comma-separated values
    balance_value numeric(14,2) := 0.0; -- Specify your wallet balance here 
    master_wallet_value boolean := FALSE; -- Specify your master wallet here 
    consumer_role_value character := 'O'; -- Specify your consumer role here 
    earn_maximum_value numeric := 500; -- Specify your earn maximum here 
    wallet_type_id_value int; 
    current_year int;
    active_end_ts_value timestamp;
    consumer_code_value varchar(50);
    wallet_id_value bigint;
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

    -- Split the comma-separated consumer codes and iterate over each code
    FOREACH consumer_code_value IN ARRAY string_to_array(consumer_code_list, ',') LOOP
        -- Create wallet for the given tenant and consumer
        RAISE NOTICE 'Started creating wallet for tenant_code: %, wallet_type: %, wallet_name: %, customer_code: %', tenant_code_value, wallet_type_code_value, wallet_name_value, consumer_code_value;
        INSERT INTO wallet.wallet (wallet_type_id, customer_code, sponsor_code, tenant_code, wallet_code, wallet_name, active_start_ts, active_end_ts, balance, earn_maximum, create_ts, create_user, delete_nbr, total_earned, master_wallet, active)
        SELECT wallet_type_id_value,
               consumer_code_value,
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
                AND customer_code = consumer_code_value
                AND master_wallet = master_wallet_value
        ) RETURNING wallet_id INTO wallet_id_value;

        IF FOUND THEN
            RAISE NOTICE 'Successfully created wallet for tenant_code: %, wallet_type: %, wallet_name: %, customer_code: %', tenant_code_value, wallet_type_code_value, wallet_name_value, consumer_code_value;
            
            -- Check if consumer wallet exists
            IF EXISTS (
                SELECT 1 FROM wallet.consumer_wallet WHERE wallet_id = wallet_id_value AND tenant_code = tenant_code_value AND consumer_code = consumer_code_value
            ) THEN
                RAISE NOTICE 'Consumer Wallet already exists, tenant_code: %, wallet_id: %, consumer_code: %, consumer_role: %, earn_maximum: %', tenant_code_value, wallet_id_value, consumer_code_value, consumer_role_value, earn_maximum_value;
            ELSE
                INSERT INTO wallet.consumer_wallet(wallet_id, tenant_code, consumer_code, consumer_role, earn_maximum, create_ts, create_user, delete_nbr)
                VALUES (wallet_id_value,  -- wallet_id
                        tenant_code_value, -- tenant_code
                        consumer_code_value, -- consumer_code
                        consumer_role_value, -- consumer_role
                        earn_maximum_value, -- earn_maximum
                        NOW(), -- create_ts
                        'SYSTEM', -- create_user
                        0 -- delete_nbr
                       );
                RAISE NOTICE 'Consumer Wallet created successfully. tenant_code: %, wallet_id: %, consumer_code: %, consumer_role: %, earn_maximum: %', tenant_code_value, wallet_id_value, consumer_code_value, consumer_role_value, earn_maximum_value;
            END IF;
        ELSE
            RAISE NOTICE 'Wallet already exists, tenant_code: %, wallet_type: %, wallet_name: %, customer_code: %', tenant_code_value, wallet_type_code_value, wallet_name_value, consumer_code_value;
        END IF;
    END LOOP;
END $$;
