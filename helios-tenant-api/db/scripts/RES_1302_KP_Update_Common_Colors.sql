-- ============================================================================
-- 🚀 Script    : Update requiredFieldColor under ux.commonColors
-- 📌 Purpose   : Adds/updates requiredFieldColor without removing existing data
-- 🧑 Author    : Bhojesh
-- 📅 Date      : 2025-12-05
-- ⚠️ Inputs    : v_tenant_codes
-- 📤 Output    : tenant_attr.ux.commonColors.requiredFieldColor updated safely
-- 📝 Notes     : Safe to rerun; preserves existing keys
-- ============================================================================

DO $$
DECLARE
   v_tenant_codes TEXT[] := ARRAY[
       'KP-TENANT-CODE'
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
               '{ux,commonColors,requiredFieldColor}',
               to_jsonb('#4A546A'::text),
               true
           ),
           update_user = v_user,
           update_ts   = v_now
       WHERE t.tenant_code = v_tenant_code
         AND t.delete_nbr = 0;

       RAISE NOTICE '✅ Updated ux.commonColors.requiredFieldColor for tenant %', v_tenant_code;
   END LOOP;
END $$;
