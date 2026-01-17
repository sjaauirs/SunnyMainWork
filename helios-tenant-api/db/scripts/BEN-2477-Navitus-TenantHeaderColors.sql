

-- ============================================================================
-- 🚀 Script    : Update headerColors under ux.themeColors for NAVITUS tenant
-- 📌 Purpose   : Updates headerColors text colors to match NAVITUS brand
-- 🧑 Author    : Bhojesh 
-- 📅 Date      : 2025-12-03
-- 🧾 Jira      : 
-- ⚠️ Inputs    : v_tenant_code 
-- 📤 Output    : tenant_attr.ux.headerColors.headerTopBorderColor , headerBottomBorderColor,
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
                    'headerColors',
                    COALESCE(t.tenant_attr::jsonb #> '{ux,headerColors}', '{}'::jsonb)
                    || jsonb_build_object(
                           'headerBottomBorderColor', '#CBCCCD',
                           'headerTopBorderColor', '#CBCCCD'
                       )
                  )
           ),
           update_user = v_user,
           update_ts   = v_now
       WHERE t.tenant_code = v_tenant_code
         AND t.delete_nbr = 0;

       RAISE NOTICE 
       '✅ Updated ux.headerColors.headerTopBorderColor and headerBottomBorderColor for tenant %', 
       v_tenant_code;
   END LOOP;
END $$;
