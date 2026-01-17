-- ============================================================================
-- 🔁 Rollback : remove 'displayAgreementsDeclineButton' from tenant_attr
-- 🧑 Author   : Charan
-- 📅 Date     : 2025-10-28
-- 🧾 Jira     : RES-917
-- ⚠️ Inputs   : KP-TENANT-CODE
-- 📤 Output   : Removes JSON key; logs status messages via RAISE NOTICE
-- 📝 Notes    : Reverses the addition of 'displayAgreementsDeclineButton'
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>'; -- replace with actual tenant code
    v_affected    INTEGER := 0;
BEGIN
    UPDATE tenant.tenant
       SET tenant_attr = tenant_attr - 'displayAgreementsDeclineButton'
     WHERE tenant_code = v_tenant_code
       AND delete_nbr = 0
       AND tenant_attr ? 'displayAgreementsDeclineButton';

    GET DIAGNOSTICS v_affected = ROW_COUNT;

    IF v_affected > 0 THEN
        RAISE NOTICE 'Removed displayAgreementsDeclineButton from tenant_attr for tenant: %', v_tenant_code;
    ELSE
        RAISE NOTICE 'No change: flag not present or tenant not found (tenant: %)', v_tenant_code;
    END IF;
END $$;
