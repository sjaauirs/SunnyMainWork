-- ===================================================================================
-- Author      : Pernati Rakesh
-- Purpose     : Restore tenant configuration
--               - Reset DST and UTC offset to defaults
--               - Re-add 'myRewards' into benefitsOptions.hamburgerMenu
--               - Remove 'noAvailblePurseImageUrl' from tenant_attr
--               - Restore period_start_ts and period_end_ts to 2025 range
-- 
-- Jira Task   : BEN-228
-- ===================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- 👈 replace with your tenant code
BEGIN
    RAISE NOTICE 'Starting rollback for tenant_code=%', v_tenant_code;

    -- Safety check
    IF NOT EXISTS (
        SELECT 1
        FROM tenant.tenant
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0
    ) THEN
        RAISE WARNING 'No active tenant found with tenant_code=% (delete_nbr=0)', v_tenant_code;
        RETURN;
    END IF;

    -- Rollback update
    UPDATE tenant.tenant
    SET 
        dst_enabled = FALSE,
        utc_time_offset = NULL,
        -- 🔹 Add "myRewards" back into benefitsOptions.hamburgerMenu (only if missing)
        tenant_option_json = jsonb_set(
            tenant_option_json,
            '{benefitsOptions,hamburgerMenu}',
            (
                SELECT jsonb_agg(DISTINCT elem)
                FROM (
                    SELECT unnest(ARRAY[
                        jsonb_array_elements(tenant_option_json->'benefitsOptions'->'hamburgerMenu'),
                        to_jsonb('myRewards'::text)
                    ]) elem
                ) t
            ),
            FALSE
        ),
        -- 🔹 Remove noAvailblePurseImageUrl key from tenant_attr
        tenant_attr = tenant_attr - 'noAvailblePurseImageUrl',
        -- 🔹 Restore period_start_ts and period_end_ts to 2025
        period_start_ts = TIMESTAMP '2025-01-01 00:00:00',
        period_end_ts   = TIMESTAMP '2025-12-31 23:59:59'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE 'Rollback completed successfully for tenant_code=%', v_tenant_code;
END $$;
