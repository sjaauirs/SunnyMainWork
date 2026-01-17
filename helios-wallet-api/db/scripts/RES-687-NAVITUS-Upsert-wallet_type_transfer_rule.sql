-- ============================================================================
-- 🚀 Script    : wallet_type_transfer_rule_upsert.sql
-- 📌 Purpose   : Upsert wallet type transfer rules for multiple tenants based on
--                source and target wallet types. Inserts new rules or updates
--                existing ones with the provided transfer configuration JSON.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-11-26
-- 🧾 Jira      : RES-687
-- ⚠️ Inputs    : Array of NAVITUS-TENANT-CODES
-- 📤 Output    : Inserts or updates records in wallet.wallet_type_transfer_rule
--                table, ensuring correct configuration for each tenant.
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--                - Script automatically generates wallet_type_transfer_rule_code
--                - Ensures delete_nbr = 0 filtering for active records.
--                - Updates update_ts and update_user when record exists.
-- ============================================================================

DO
$$
DECLARE 
    v_tenant_codes        text[] := ARRAY['<NAVITUS-TENANT-CODE>', '<NAVITUS-TENANT-CODE>'];  
    v_source_code         text    := 'wat-2d62dcaf2aa4424b9ff6c2ddb5895077'; -- Health Actions Reward
    v_target_code         text    := 'wat-c3b091232e974f98aeceb495d2a9f916'; -- Health Actions Sweepstakes Entries
    v_transfer_json       jsonb   := '{"transferRatio": 1}'; 

    v_source_wallet_type_id bigint;
    v_target_wallet_type_id bigint;
    v_existing_id            bigint;
    v_loop_tenant            text;
    v_new_code               text;
BEGIN
    -- Fetch source wallet_type_id
    SELECT wallet_type_id INTO v_source_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = v_source_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_source_wallet_type_id IS NULL THEN
        RAISE EXCEPTION 'Source wallet_type_code % not found', v_source_code;
    END IF;

    -- Fetch target wallet_type_id
    SELECT wallet_type_id INTO v_target_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = v_target_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_target_wallet_type_id IS NULL THEN
        RAISE EXCEPTION 'Target wallet_type_code % not found', v_target_code;
    END IF;

    -- Loop tenant codes
    FOREACH v_loop_tenant IN ARRAY v_tenant_codes
    LOOP
        -- Check existing record
        SELECT wallet_type_transfer_rule_id INTO v_existing_id
        FROM wallet.wallet_type_transfer_rule
        WHERE tenant_code = v_loop_tenant
          AND source_wallet_type_id = v_source_wallet_type_id
          AND target_wallet_type_id = v_target_wallet_type_id
          AND delete_nbr = 0
        LIMIT 1;

        IF v_existing_id IS NOT NULL THEN
            
            UPDATE wallet.wallet_type_transfer_rule
            SET transfer_rule_json = v_transfer_json,
                update_ts = NOW(),
                update_user = 'SYSTEM'
            WHERE wallet_type_transfer_rule_id = v_existing_id;

            RAISE NOTICE 'UPDATED tenant=% , rule_id=% , source_wallet_type_id=% , target_wallet_type_id=%',
                v_loop_tenant, v_existing_id, v_source_wallet_type_id, v_target_wallet_type_id;

        ELSE
            
            v_new_code := 'wtr_' || gen_random_uuid();

            INSERT INTO wallet.wallet_type_transfer_rule
            (
                wallet_type_transfer_rule_code,
                tenant_code,
                source_wallet_type_id,
                target_wallet_type_id,
                transfer_rule_json,
                create_ts,
                create_user,
                delete_nbr
            )
            VALUES
            (
                v_new_code,
                v_loop_tenant,
                v_source_wallet_type_id,
                v_target_wallet_type_id,
                v_transfer_json,
                NOW(),
                'SYSTEM',
                0
            );

            RAISE NOTICE 'INSERTED tenant=% , new_code=% , source_wallet_type_id=% , target_wallet_type_id=%',
                v_loop_tenant, v_new_code, v_source_wallet_type_id, v_target_wallet_type_id;
        END IF;
    END LOOP;
END
$$;
