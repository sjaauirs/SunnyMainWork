-- ============================================================================
-- 🔄 Script    : Rollback - Remove Added Task Colors from Tenant Attribute
-- 📌 Purpose   : Remove the newly added task color properties (missingActivity, syncText, syncLabelBgColor)
--                from the JSON field `tenant_attr.taskColors` for a given tenant.
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-09
-- 🧾 Jira      : BEN-957
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📥 Rollback  : Removes only the 3 new keys, preserving other taskColors data.
-- 📝 Notes     : Non-destructive rollback — only deletes specified keys.
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- Replace with actual tenant code
BEGIN
    RAISE NOTICE '🔄 Removing taskColors for tenant: %', v_tenant_code;

    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        tenant_attr,
        '{ux,taskColors}',
        (
            COALESCE(tenant_attr->'ux'->'taskColors', '{}'::jsonb)
            - 'missingActivity'
            - 'syncText'
            - 'syncLabelBgColor'
        ),
        true
    ),
    update_ts = NOW(),
    update_user = 'ROLLBACK_SCRIPT'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '✅ Removed ux.taskColors keys for tenant: %', v_tenant_code;
END $$;

-- ============================================================================
-- 🔄 Script    : Rollback - Remove Environment-Specific Icon URL
-- 📌 Purpose   : Removes `questionSyncUrlFinal` key from tenant_attr JSON.
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-09
-- 🧾 Jira      : BEN-957
-- ⚠️ Inputs    :
--    - v_tenant_code (Tenant Code, e.g., <KP-TENANT-CODE>)
-- 📥 Rollback  : Deletes only `questionSyncUrlFinal` key.
-- 📝 Notes     : Non-destructive rollback — keeps all other tenant_attr data intact.
-- ============================================================================

DO $$
DECLARE
  v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- Replace with actual tenant code
BEGIN
  RAISE NOTICE '🔄 Removing questionSyncUrlFinal icon for tenant: %', v_tenant_code;

  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr - 'questionSyncUrlFinal',
      update_user = 'ROLLBACK_SCRIPT',
      update_ts   = NOW()
  WHERE tenant_code = v_tenant_code
    AND delete_nbr  = 0;

  RAISE NOTICE '✅ Removed questionSyncUrlFinal from tenant_attr for tenant: %', v_tenant_code;
END $$;