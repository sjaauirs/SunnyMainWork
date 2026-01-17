-- ============================================================================
-- 🚀 Script    : Update Task Colors in Tenant Attribute
-- 📌 Purpose   : Add new task color configuration properties (missingActivity, syncText, syncLabelBgColor)
--                inside the JSON field `tenant_attr.taskColors` for a given tenant.
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-09
-- 🧾 Jira      : BEN-957
-- ⚠️ Inputs    : KP-TENANT-CODE
-- 📤 Output    : Updates tenant_attr JSON to include new taskColors for visual consistency in the platform.
-- 📝 Notes     : Safely merges new keys without overwriting existing taskColors.
-- ============================================================================

DO $$
DECLARE
    v_tenant_code TEXT := '<KP-TENANT-CODE>';  -- Replace with actual tenant code
BEGIN
    RAISE NOTICE '🔹 Updating ux.taskColors for tenant: %', v_tenant_code;

    UPDATE tenant.tenant
    SET tenant_attr = jsonb_set(
        tenant_attr,
        '{ux,taskColors}',  -- nested inside ux
        COALESCE(tenant_attr->'ux'->'taskColors', '{}'::jsonb) || jsonb_build_object(
            'missingActivity', '#0078B3',
            'syncText', '#3E8C1C',
            'syncLabelBgColor', '#DDEDD7'
        ),
        true
    ),
    update_ts = NOW(),
    update_user = 'SYSTEM'
    WHERE tenant_code = v_tenant_code
      AND delete_nbr = 0;

    RAISE NOTICE '✅ ux.taskColors updated successfully for tenant: %', v_tenant_code;
END $$;

-- ============================================================================
-- 🚀 Script    : Add Environment-Specific Icon URLs in Tenant Attributes
-- 📌 Purpose   : Adds env-specific icon URLs under tenant_attr -> assets -> icons
-- 🧑 Author    : Preeti
-- 📅 Date      : 2025-10-09
-- 🧾 Jira      : BEN-957
-- ⚠️ Inputs    :
--    - v_tenant_code (Tenant Code, e.g., <KP-TENANT-CODE>)
--    - v_env (Environment: DEV / QA / UAT / INTEG / PROD)
-- 📤 Output    : Adds env-based icon URLs inside tenant_attr.assets.icons
-- 📝 Notes     :
--   - Keeps existing icons intact
--   - Adds or updates `questionSyncUrlFinal`
--   - URL is dynamically built using the environment base path
-- ============================================================================

DO $$
DECLARE
  v_tenant_code      TEXT := '<KP-TENANT-CODE>';  -- Replace with actual tenant code
  v_env              TEXT := '<ENV>';             -- DEV / QA / UAT / INTEG / PROD
  v_env_specific_url TEXT;
  v_now              TIMESTAMP := NOW();
  v_user             TEXT := 'SYSTEM';
BEGIN
  -- Environment-specific base URL
  CASE v_env
    WHEN 'DEV'   THEN v_env_specific_url := 'https://app-static.dev.sunnyrewards.com';
    WHEN 'QA'    THEN v_env_specific_url := 'https://app-static.qa.sunnyrewards.com';
    WHEN 'UAT'   THEN v_env_specific_url := 'https://app-static.uat.sunnyrewards.com';
    WHEN 'INTEG' THEN v_env_specific_url := 'https://app-static.integ.sunnyrewards.com';
    WHEN 'PROD'  THEN v_env_specific_url := 'https://app-static.sunnyrewards.com';
    ELSE RAISE EXCEPTION 'Invalid environment [%]. Choose DEV / QA / UAT / INTEG / PROD.', v_env;
  END CASE;

  -- Update tenant_attr with new key directly
  UPDATE tenant.tenant
  SET tenant_attr = tenant_attr || jsonb_build_object(
      'questionSyncUrlFinal', v_env_specific_url || '/assets/icons/questionSyncUrlFinal.svg'
  ),
  update_user = v_user,
  update_ts   = v_now
  WHERE tenant_code = v_tenant_code
    AND delete_nbr  = 0;

  RAISE NOTICE '✅ Added env-specific questionSyncUrlFinal icon for tenant=%, env=%', v_tenant_code, v_env;
END $$;
