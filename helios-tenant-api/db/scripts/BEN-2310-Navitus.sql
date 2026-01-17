-- ============================================================================
-- 🚀 Script    : Update commonColors under ux.themeColors for NAVITUS tenant
-- 📌 Purpose   : Updates commonColors text colors to match NAVITUS brand
-- 🧑 Author    : Bhojesh 
-- 📅 Date      : 2025-11-21
-- 🧾 Jira      : 
-- ⚠️ Inputs    : v_tenant_code 
-- 📤 Output    : tenant_attr.ux.commonColors.textColor,
--                updated
-- 📝 Notes     : Safe to rerun; existing keys remain intact
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
           COALESCE(t.tenant_attr::jsonb, '{}'::jsonb)
           || jsonb_build_object(
               'ux',
               COALESCE(t.tenant_attr::jsonb -> 'ux', '{}'::jsonb)
               || jsonb_build_object(
                     'commonColors',
                     COALESCE(t.tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                     || jsonb_build_object(
                           'textColor', '#0B0C0E'
                       )
                 )
           ),
           update_user = v_user,
           update_ts   = v_now
       WHERE t.tenant_code = v_tenant_code
         AND t.delete_nbr = 0;

       RAISE NOTICE '✅ Updated ux.commonColors.textColor for tenant %', v_tenant_code;
   END LOOP;
END $$;
