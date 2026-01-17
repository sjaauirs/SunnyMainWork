-- ============================================================================
-- 🚀 Script    : rollback_nav_benefitOptions_updates.sql
-- 📌 Purpose   : Rollback "menuNavigation", "hamburgerMenu", and "hamburgerMenuIcons"
--                under "benefitsOptions" for selected NAV tenants.
-- 🧑 Author    : Saurabh Jaiswal
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : BEN-1986
-- ⚙️ Notes     :
--                 • Removes only specified keys; other JSON content is untouched.
--                 • Safe to rerun.
-- ============================================================================

DO
$$
DECLARE
    -- 🔹 NAV tenant codes to rollback
    v_tenant_codes TEXT[] := ARRAY[
        '<NAV-TENANT-CODE>'
        -- Add more tenant codes as needed
    ];

    v_tenant_code TEXT;
    v_rowcount INT;
BEGIN
    RAISE NOTICE '🚨 Starting rollback for NAV tenants...';

    FOREACH v_tenant_code IN ARRAY v_tenant_codes LOOP
        RAISE NOTICE '➡️ Rolling back benefitOptions for tenant: %', v_tenant_code;

        UPDATE tenant.tenant
        SET tenant_option_json = tenant_option_json || jsonb_build_object(
            'benefitsOptions',
            (
                -- Remove the three keys but retain other nested data
                COALESCE(tenant_option_json->'benefitsOptions', '{}'::jsonb)
                - 'menuNavigation'
                - 'hamburgerMenu'
                - 'hamburgerMenuIcons'
            )
        ),
        update_ts   = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND tenant_option_json IS NOT NULL
          AND tenant_option_json <> '{}'::jsonb
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount > 0 THEN
            RAISE NOTICE '✅ Rolled back benefitOptions keys for tenant: %', v_tenant_code;
        ELSE
            RAISE NOTICE 'ℹ️ No rollback needed (tenant missing or keys absent): %', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE '🏁 Completed rollback for all listed tenants.';
END
$$;
