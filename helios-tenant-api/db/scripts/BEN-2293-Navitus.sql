-- ============================================================================
-- 🚀 Script    : Update button colors under ux.themeColors for NAVITUS tenant
-- 📌 Purpose   : Updates primary button and text colors to match NAVITUS brand
-- 🧑 Author    : Bhojesh 
-- 📅 Date      : 2025-11-19
-- 🧾 Jira      : 
-- ⚠️ Inputs    : v_tenant_code 
-- 📤 Output    : tenant_attr.ux.themeColors.headerBgColor, tenant_attr.ux.themeColors.taskGradient1, tenant_attr.ux.themeColors.taskGradient2,tenant_attr.ux.themeColors.entriesGradient1, tenant_attr.ux.themeColors.entriesGradient2 and tenant_attr.ux.themeColors.rActivityTextColor,
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
               (
                 COALESCE(t.tenant_attr::jsonb -> 'ux', '{}'::jsonb)
                 || jsonb_build_object(
                     'themeColors',
                     (
                       COALESCE(t.tenant_attr::jsonb #> '{ux,themeColors}', '{}'::jsonb)
                       || jsonb_build_object(
                           'headerBgColor', '#0B0C0E',
                           'taskGradient1', '#326F91',
                           'taskGradient2', '#E9EBED',
                           'entriesGradient1','#3B83AB',
                           'entriesGradient2','#21495F',
                           'rActivityTextColor','#0B0C0E'
                       )
                     )
                 )
               )
           ),
           update_user = v_user,
           update_ts   = v_now
       WHERE t.tenant_code = v_tenant_code
         AND t.delete_nbr = 0;

       RAISE NOTICE '✅ Updated ux.themeColors.headerBgColor, taskGradient1, taskGradient2, entriesGradient1, entriesGradient2 and rActivityTextColor for tenant %', v_tenant_code;
   END LOOP;
END $$;