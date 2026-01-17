-- ============================================================================
-- 🚀 Script: Update activeTabBgColor for Navitus
-- 📌 Purpose: Update activeTabBgColor for Navitus
-- 🧑 Author  : Preeti
-- 📅 Date    : 09/22/2026
-- 🧾 Jira    : SUN-466
-- ⚠️  Inputs: NAVITUS-TENANT-CODE
-- ============================================================================
DO
$$
DECLARE
   v_tenant_code TEXT := '<NAVITUS-TENANT-CODE>';
   v_new_value  TEXT := '#326F91';
BEGIN
   UPDATE tenant.tenant
   SET tenant_attr = jsonb_set(
       tenant_attr::jsonb,
       '{ux,taskTileColors,activeTabBgColor}',
       to_jsonb(v_new_value), 
       true
   )
   WHERE tenant_code = v_tenant_code;
END;
$$;