-- 🚀 Script    : Update buttonDisableColor under ux.commonColors for KP tenant
-- 📌 Purpose   : Updates activate card button color to #D3D6DC
-- 🧑 Author    : Riaz Ahmed
-- 📅 Date      : 2025-11-07
-- 🧾 Jira      : SUN-851
-- ⚠️ Inputs    : v_tenant_code 
-- 📤 Output    : tenant_attr.ux.commonColors.buttonDisableColor updated to "#D3D6DC"
-- 📝 Notes     : Safe to rerun; existing keys remain intact

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
        SET tenant_attr = jsonb_set(
                              tenant_attr::jsonb,
                              '{ux,commonColors}',
                              COALESCE(tenant_attr::jsonb #> '{ux,commonColors}', '{}'::jsonb)
                              || jsonb_build_object('buttonDisableColor', '#D3D6DC'),
                              true
                          ),
            update_user = v_user,
            update_ts   = v_now
        WHERE t.tenant_code = v_tenant_code
          AND t.delete_nbr = 0;

        RAISE NOTICE '✅ Updated ux.commonColors.buttonDisableColor=#D3D6DC for tenant %', v_tenant_code;
    END LOOP;
END $$;