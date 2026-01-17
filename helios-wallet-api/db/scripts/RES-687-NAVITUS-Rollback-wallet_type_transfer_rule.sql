-- ============================================================================
-- 🚀 Script    : wallet_type_transfer_rule_upsert_rollback.sql
-- 📌 Purpose   : Soft delete wallet type transfer rule records created or
--                updated by the main upsert script.
-- 🧑 Author    : Siva Krishna Reddy
-- 📅 Date      : 2025-11-26
-- 🧾 Jira      : RES-687
-- ⚠️ Inputs    : Array of NAVITUS-TENANT-CODES
-- 📤 Output    : Soft deletes matching records by setting delete_nbr = PK value
-- 🔗 Script URL: NA
-- 📝 Notes     : Execute if you want soft delete the inserted records
-- ============================================================================

DO
$$
DECLARE 
    v_tenant_codes        text[] := ARRAY['<NAVITUS-TENANT-CODE>', '<NAVITUS-TENANT-CODE>'];
    v_source_code         text    := 'wat-2d62dcaf2aa4424b9ff6c2ddb5895077'; -- Health Actions Reward
    v_target_code         text    := 'wat-c3b091232e974f98aeceb495d2a9f916'; -- Health Actions Sweepstakes Entries

    v_source_wallet_type_id bigint;
    v_target_wallet_type_id bigint;
    v_existing_id            bigint;
    v_loop_tenant            text;
BEGIN
    -- Fetch source wallet_type_id
    SELECT wallet_type_id INTO v_source_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = v_source_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_source_wallet_type_id IS NULL THEN
        RAISE EXCEPTION 'Rollback failed: Source wallet_type_code % not found', v_source_code;
    END IF;

    -- Fetch target wallet_type_id
    SELECT wallet_type_id INTO v_target_wallet_type_id
    FROM wallet.wallet_type
    WHERE wallet_type_code = v_target_code
      AND delete_nbr = 0
    LIMIT 1;

    IF v_target_wallet_type_id IS NULL THEN
        RAISE EXCEPTION 'Rollback failed: Target wallet_type_code % not found', v_target_code;
    END IF;

    -- Loop tenant codes for rollback
    FOREACH v_loop_tenant IN ARRAY v_tenant_codes
    LOOP
        -- Find existing active rule
        SELECT wallet_type_transfer_rule_id INTO v_existing_id
        FROM wallet.wallet_type_transfer_rule
        WHERE tenant_code = v_loop_tenant
          AND source_wallet_type_id = v_source_wallet_type_id
          AND target_wallet_type_id = v_target_wallet_type_id
          AND delete_nbr = 0
        LIMIT 1;

        IF v_existing_id IS NOT NULL THEN
            -- Soft delete using PK value
            UPDATE wallet.wallet_type_transfer_rule
            SET delete_nbr = v_existing_id,
                update_user = 'SYSTEM',
                update_ts = NOW()
            WHERE wallet_type_transfer_rule_id = v_existing_id;
            
            RAISE NOTICE 'Rollback: tenant=% , wallet_type_transfer_rule_id=% , source_wallet_type_id=% , target_wallet_type_id=%',
                v_loop_tenant, v_existing_id, v_source_wallet_type_id, v_target_wallet_type_id;
        END IF;
    END LOOP;
END
$$;
