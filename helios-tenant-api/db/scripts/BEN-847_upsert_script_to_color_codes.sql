-- ============================================================================
-- 🎨 Script    : Insert/Update borderColor & borderColor1 in tenant_attr JSONB
-- 📌 Purpose   : 
--   - Ensures the `tenant_attr` JSONB includes the correct values for 
--     `ux.commonColors.borderColor` and `ux.commonColors.borderColor1`.
--   - Inserts the values if missing or updates them if they differ.
-- 👨‍💻 Author   : Rakesh Pernati
-- 📅 Date      : 2025-10-13
-- 🧾 Jira      : BEN-582
-- ⚙️ Inputs    :
--      v_tenant_code                 → Tenant identifier (replace <KP-TENANT-CODE>)
-- 📤 Output    :
--      - `tenant_attr` updated with borderColor = '#0078B3' and borderColor1 = '#B3B7C1'
--      - Notices raised for insert/update/no-change cases
-- 📝 Notes     :
--      - Does not update if values already match
--      - Raises warning if tenant not found or `tenant_attr` is empty
--      - Safe to re-run (idempotent)
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;
    v_border_color TEXT := '#0078B3';
    v_border_color1 TEXT := '#B3B7C1';
BEGIN
    -- Fetch current tenant_attr
    SELECT tenant_attr
    INTO v_old_attr
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0
      AND tenant_attr IS NOT NULL
      AND tenant_attr::text <> '{}';

    IF NOT FOUND THEN
        RAISE WARNING '⚠️ No tenant found or tenant_attr is empty for tenant_code: %', v_tenant_code;
        RETURN;
    END IF;

    v_new_attr := v_old_attr;

    -- Insert/update borderColor
    IF (v_new_attr #>> '{ux,commonColors,borderColor}') IS DISTINCT FROM v_border_color THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,commonColors,borderColor}', to_jsonb(v_border_color), true);
        v_updated := true;

        IF (v_old_attr #>> '{ux,commonColors,borderColor}') IS NULL THEN
            RAISE NOTICE '✅ borderColor inserted with value % for tenant %', v_border_color, v_tenant_code;
        ELSE
            RAISE NOTICE '✅borderColor updated to value % for tenant %', v_border_color, v_tenant_code;
        END IF;
    ELSE
        RAISE NOTICE '⚠️borderColor already set to % for tenant % (no change)', v_border_color, v_tenant_code;
    END IF;

    -- Insert/update borderColor1
    IF (v_new_attr #>> '{ux,commonColors,borderColor1}') IS DISTINCT FROM v_border_color1 THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,commonColors,borderColor1}', to_jsonb(v_border_color1), true);
        v_updated := true;

        IF (v_old_attr #>> '{ux,commonColors,borderColor1}') IS NULL THEN
            RAISE NOTICE '✅borderColor1 inserted with value % for tenant %', v_border_color1, v_tenant_code;
        ELSE
            RAISE NOTICE '✅borderColor1 updated to value % for tenant %', v_border_color1, v_tenant_code;
        END IF;
    ELSE
        RAISE NOTICE '⚠️borderColor1 already set to % for tenant % (no change)', v_border_color1, v_tenant_code;
    END IF;

    -- Update only if something changed
    IF v_updated THEN
        UPDATE tenant.tenant
        SET tenant_attr = v_new_attr
        WHERE tenant_code = v_tenant_code
          AND delete_nbr = 0;

        RAISE NOTICE '✅tenant_attr updated successfully for tenant %', v_tenant_code;
    ELSE
        RAISE NOTICE '✅No changes made, tenant_attr already has correct values for tenant %', v_tenant_code;
    END IF;
END $$;
