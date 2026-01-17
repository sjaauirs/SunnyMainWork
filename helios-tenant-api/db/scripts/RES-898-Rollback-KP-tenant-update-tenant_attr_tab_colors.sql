-- ==================================================================================================
-- 🚀 Rollback Script : Remove or revert tabBar colors in tenant_attr.ux.tabBar
-- 📌 Purpose         : For a given tenant_code, rollback previously added/updated tabBar colors.
-- 🧑 Author          : Siva Krishna
-- 📅 Date            : 2025-10-16
-- 🧾 Jira            : RES-898(sub-task)
-- ⚠️ Inputs          : KP_TENANT_CODE
-- 📤 Output          : Removes ux->tabBar JSON node (only)
-- 🔗 Script URL      : NA
-- 📝 Notes           : Idempotent — safe to execute multiple times
-- ==================================================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP_TENANT_CODE>'; -- Input tenant code
    v_exists BOOLEAN;
BEGIN
    -- Check if tenant exists
    IF NOT EXISTS (SELECT 1 FROM tenant.tenant WHERE tenant_code = v_tenant_code AND delete_nbr = 0) THEN
        RAISE EXCEPTION '❌ Tenant with code "%" not found', v_tenant_code;
    END IF;

    -- Check if ux->tabBar exists
    SELECT (tenant_attr -> 'ux' -> 'tabBar') IS NOT NULL
    INTO v_exists
    FROM tenant.tenant
    WHERE tenant_code = v_tenant_code;

    IF v_exists THEN
        -- Remove only the tabBar node under ux
        UPDATE tenant.tenant
        SET tenant_attr = jsonb_set(
            tenant_attr,
            '{ux}',
            (tenant_attr -> 'ux') - 'tabBar'
        )
        WHERE tenant_code = v_tenant_code;

        RAISE NOTICE '♻️ Rolled back ux.tabBar colors for tenant: %', v_tenant_code;
    ELSE
        RAISE NOTICE 'ℹ️ No ux.tabBar found to rollback for tenant: %', v_tenant_code;
    END IF;

END $$;
