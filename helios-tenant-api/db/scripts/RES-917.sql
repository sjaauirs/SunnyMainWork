-- ============================================================================
-- 🚀 Script    : add 'displayAgreementsDeclineButton' to tenant_attr
-- 📌 Purpose   : Add 'displayAgreementsDeclineButton' to tenant_attr
-- 🧑 Author    : Charan
-- 📅 Date      : 2025-10-28
-- 🧾 Jira      : RES-917
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Updates JSON structure; logs status messages via RAISE NOTICE
-- 🔗 Script URL: NA
-- 📝 Notes     : 
--   - Adds 'displayAgreementsDeclineButton' to tenant_attr.
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';
BEGIN
    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(tenant_attr, '{displayAgreementsDeclineButton}', 'true', true)
    WHERE tenant_code = v_tenant_code
    AND delete_nbr = 0;

    RAISE NOTICE 'Added displayAgreementsDeclineButton to tenant_attr for tenant: %', v_tenant_code;
END $$;