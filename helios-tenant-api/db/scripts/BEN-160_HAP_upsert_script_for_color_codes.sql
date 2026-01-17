-- ============================================================================
-- 🎨 Script    : Insert/Update seeMoreColor in tenant_attr JSONB
-- 📌 Purpose   : 
--   - Ensures the `tenant_attr` JSONB includes the correct values for 
--     `ux.commonColors.borderColor` and `ux.themeColors.seeMoreColor`.
--   - Inserts the values if missing or updates them if they differ.
-- 👨‍💻 Author   : Rakesh Pernati
-- 📅 Date      : 2025-10-28
-- 🧾 Jira      : BEN-160
-- ⚙️ Inputs    :
--      v_tenant_code                 → Tenant identifier (replace <HAP-TENANT-CODE>)
-- 📤 Output    :
--      - `tenant_attr` updated with headerBgColor = '#181D27'
--      - Notices raised for insert/update/no-change cases
-- 📝 Notes     :
--      - Does not update if values already match
--      - Raises warning if tenant not found or `tenant_attr` is empty
--      - Safe to re-run (idempotent)
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<HAP-TENANT-CODE>'; -- Input tenant code
    v_old_attr JSONB;
    v_new_attr JSONB;
    v_updated BOOLEAN := false;
    v_see_more_color TEXT := '#181D27';
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

    -- Insert/update seeMoreColor
    IF (v_new_attr #>> '{ux,themeColors,seeMoreColor}') IS DISTINCT FROM v_see_more_color THEN
        v_new_attr := jsonb_set(v_new_attr, '{ux,themeColors,seeMoreColor}', to_jsonb(v_see_more_color), true);
        v_updated := true;

        IF (v_old_attr #>> '{ux,themeColors,seeMoreColor}') IS NULL THEN
            RAISE NOTICE '✅ seeMoreColor inserted with value % for tenant %', v_see_more_color, v_tenant_code;
        ELSE
            RAISE NOTICE '✅seeMoreColor updated to value % for tenant %', v_see_more_color, v_tenant_code;
        END IF;
    ELSE
        RAISE NOTICE '⚠seeMoreColor already set to % for tenant % (no change)', v_see_more_color, v_tenant_code;
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
