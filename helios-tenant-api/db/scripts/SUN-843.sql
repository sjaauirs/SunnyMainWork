-- ============================================================================  
-- 🚀 Script    : SUN-842  
-- 📌 Purpose   : Update banner title and banner background color for Haleon  
-- 🧑 Author    : Preeti  
-- 📅 Date      : 2025-10-24  
-- 🧾 Jira      : SUN-842  
-- ⚠️ Inputs    : Navitus-Tenant-Code  
-- ============================================================================

DO $$
DECLARE
    v_tenant_codes TEXT[] := ARRAY[
        'NAVITUS-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_rowcount INT := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE task.task_reward
        SET task_reward_config_json = jsonb_set(
            jsonb_set(
                task_reward_config_json,
                '{bannerBackgroundColor}', '"#E27025"',  -- Update background color
                false
            ),
            '{bannerTitle}', 
            '"Click here for $10 to spend at Costco on Advil®, Sensodyne Pronamel®, Centrum Silver®, Nexium®, Flonase®"',  -- Update title
            false
        )
        || jsonb_build_object(
            'taskDescriptionImage',
            to_jsonb(
                REPLACE(
                    task_reward_config_json->>'taskDescriptionImage',
                    'haleon.png',
                    'haleon.jpg'
                )
            )  
        ),
        update_ts = NOW(),
        update_user = 'SYSTEM'
        WHERE tenant_code = v_tenant_code
          AND task_external_code = 'haleon_costco'
          AND delete_nbr = 0;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        RAISE NOTICE '✅ Tenant % → % rows updated', v_tenant_code, v_rowcount;
    END LOOP;
END $$;