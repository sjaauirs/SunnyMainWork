-- ============================================================================
-- 🚀 Script    : Update splashScreenBgColor under ux.commonColors
-- 📌 Purpose   : Adds/updates splashScreenBgColor without removing existing data
-- 🧑 Author    : Bhojesh
-- 📅 Date      : 2025-12-03
-- ⚠️ Inputs    : v_tenant_codes
-- 📤 Output    : tenant_attr.ux.commonColors.splashScreenBgColor updated safely
-- 📝 Notes     : Safe to rerun; preserves existing keys
-- ============================================================================

DO $$
DECLARE
   v_tenant_codes TEXT[] := ARRAY[
       'NAVITUS-TENANT-CODE'
   ];
   v_tenant_code TEXT;
   v_now         TIMESTAMP := NOW();
   v_user        TEXT := 'SYSTEM';
BEGIN
   FOREACH v_tenant_code IN ARRAY v_tenant_codes
   LOOP
       UPDATE tenant.tenant t
       SET tenant_attr =
           jsonb_set(
               COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),
               '{ux,commonColors,splashScreenBgColor}',
               to_jsonb('#F7F7F7'),
               true
           ),
           update_user = v_user,
           update_ts   = v_now
       WHERE t.tenant_code = v_tenant_code
         AND t.delete_nbr = 0;

       RAISE NOTICE '✅ Updated ux.commonColors.splashScreenBgColor for tenant %', v_tenant_code;
   END LOOP;
END $$;
