-- ============================================================================
-- 🔄 Rollback Script : Remove splashScreenBgColor under ux.commonColors
-- 📌 Purpose         : Deletes ONLY splashScreenBgColor safely
-- 🧑 Author          : Bhojesh
-- 📅 Date            : 2025-12-03
-- ⚠️ Inputs          : v_tenant_codes
-- 📤 Output          : tenant_attr rolled back to previous state
-- 📝 Notes           : Safe to rerun; all other keys remain unchanged
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
           -- Remove ONLY splashScreenBgColor key
           t.tenant_attr::jsonb #- '{ux,commonColors,splashScreenBgColor}',
           
           update_user = v_user,
           update_ts   = v_now
       WHERE t.tenant_code = v_tenant_code
         AND t.delete_nbr = 0;

       RAISE NOTICE '🔄 Rolled back ux.commonColors.splashScreenBgColor for tenant %', v_tenant_code;
   END LOOP;
END $$;
