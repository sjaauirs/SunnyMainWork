-- ============================================================================
-- 🚀 Script    : Script to add flag "displayAlternateDialer" in tenant_attr with default value false
-- 📌 Purpose   : Based on this flag UI will flip the dialer in UI
-- 🧑 Author    : Siva Krishna
-- 📅 Date      : 24-09-2025
-- 🧾 Jira      : RES-150 & RES-560(Sub-task)
-- ⚠️ Inputs    : No Input required
-- 📤 Output    : It will add the flag "displayAlternateDialer" with default value for all tenants in tenant.tenant table
-- 🔗 Script URL: NA
-- 📝 Notes     : NA
-- ============================================================================

DO $$
DECLARE
    v_updated_count INT := 0;
BEGIN
    RAISE NOTICE '[Information] Starting update: Inserting "displayAlternateDialer": false into tenant.tenant.tenant_attr';

    UPDATE tenant.tenant t
    SET tenant_attr = COALESCE(tenant_attr, '{}'::jsonb) || jsonb_build_object('displayAlternateDialer', false)
    WHERE t.delete_nbr = 0
      AND NOT (COALESCE(tenant_attr, '{}'::jsonb) ? 'displayAlternateDialer');

    GET DIAGNOSTICS v_updated_count = ROW_COUNT;

    IF v_updated_count > 0 THEN
        RAISE NOTICE '[Information] Successfully added flag to % row(s)', v_updated_count;
    ELSE
        RAISE NOTICE '[Information] No updates required — flag already exists in all rows';
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE '[Error] Unexpected error occurred: %', SQLERRM;
        RAISE;
END $$;
