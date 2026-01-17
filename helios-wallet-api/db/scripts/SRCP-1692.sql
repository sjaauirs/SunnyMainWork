
--This script is used to create a secondary redemption wallet for a specified tenant.
DO $$
DECLARE
    redemption_wallet_type_code varchar(50) := 'wat-274bd71345804f09928cf451dc0f6239';
    wallet_name_value varchar(80) := 'TENANT_MASTER_REDEMPTION:HSA';
    redemption_wallet_type_id int;
    wallet_customer_code_value varchar(50);
    current_year int;
    active_end_ts_value timestamp;
    tenant_code_value varchar(50) := 'ten-8d9e6f00eec8436a8251d55ff74b1642'; -- Specify your tenant code here
BEGIN 
    -- Fetch Primary redemption wallet type id
    SELECT wallet_type_id INTO redemption_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = redemption_wallet_type_code;

    -- Generate wallet customer code
    SELECT 'cus-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', '') INTO wallet_customer_code_value;

    -- Generate active_end_ts
    SELECT EXTRACT(YEAR FROM CURRENT_TIMESTAMP) INTO current_year;
    SELECT MAKE_TIMESTAMP(current_year, 12, 31, 23, 59, 59) AT TIME ZONE 'UTC' INTO active_end_ts_value;

    -- Create secondary redemption wallet for the given tenant
    RAISE NOTICE '---- started creating secondary wallet for tenant: %  -----', tenant_code_value;
    INSERT INTO wallet.wallet (wallet_type_id, customer_code, sponsor_code, tenant_code, wallet_code, wallet_name, active_start_ts, active_end_ts, balance, earn_maximum, create_ts, create_user, delete_nbr, total_earned, master_wallet, active)
    SELECT redemption_wallet_type_id,
           wallet_customer_code_value,
           NULL,
           tenant_code_value,
           'wal-' || REPLACE(CAST(gen_random_uuid() as varchar), '-', ''),
           wallet_name_value,
           NOW(), -- active_start_ts
           active_end_ts_value, -- active_end_ts
           0, -- Initial balance
           0, -- earn_maximum
           NOW(), -- create_ts
           'SYSTEM', -- create_user
           0, -- delete_nbr
           0, -- total_earned
           true, -- master_wallet
           true -- active
    WHERE NOT EXISTS (
        SELECT 1 FROM wallet.wallet 
        WHERE wallet_type_id = redemption_wallet_type_id
            AND tenant_code = tenant_code_value
            AND wallet_name = wallet_name_value
            AND master_wallet = true
    );

    IF FOUND THEN
        RAISE NOTICE '---- successfully created secondary redemption wallet for tenant_code: %  -----', tenant_code_value;
    ELSE
        RAISE NOTICE '---- Secondary redemption wallet already exists, tenant_code: %  -----', tenant_code_value;
    END IF;

END $$;
