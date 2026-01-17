-- ============================================================================
-- 🔄 Rollback Script : Reset headerTopBorderColor & headerBottomBorderColor
-- 📌 Purpose         : Sets both colors back to #0D1C3D under ux.headerColors
-- 🧑 Author          : Bhojesh
-- 📅 Date            : 2025-12-03
-- ⚠️ Inputs          : v_tenant_codes
-- 📤 Output          : header border colors restored to original
-- 📝 Notes           : Safe to rerun; preserves all other keys
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
               jsonb_set(
                   COALESCE(t.tenant_attr::jsonb, '{}'::jsonb),
                   '{ux,headerColors,headerTopBorderColor}',
                   to_jsonb('#0D1C3D'::text),
                   true
               ),
               '{ux,headerColors,headerBottomBorderColor}',
               to_jsonb('#0D1C3D'::text),
               true
           ),
           update_user = v_user,
           update_ts   = v_now
       WHERE t.tenant_code = v_tenant_code
         AND t.delete_nbr = 0;

       RAISE NOTICE 
       '🔄 Rolled back header border colors to #0D1C3D for tenant %',
       v_tenant_code;
   END LOOP;
END $$;
