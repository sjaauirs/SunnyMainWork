-- =================================================================================================================================
-- 🚀 Script    : Update Task Detail -> task description text max trackable days to 24 for Rethink your drink
-- 📌 Purpose   : To Task Detail max trackable days to 24 for Rethink your drink 
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-11-30
-- 🧾 Jira      : SUN-1067
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Updated task description for given tenant(s)
-- 🔗 Script URL: Internal configuration update (QA only)
-- 📝 Notes     : 
--   
--   - Safe to re-run (idempotent)
-- =================================================================================================================================
DO $$
DECLARE

    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_language_code TEXT := 'en-US';
    v_task_id BIGINT;
    v_updated_count INTEGER := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP            
		UPDATE task.task_detail
		SET task_description = 'Staying  well hydrated keeps the body working properly. It helps joints, body temperature, mood, and more.  On 4 or more days a week, log the recommended 92 ounces of hydration your body gets from water and food.  When you consume the 92 ounces on 20 or more days earn rewards for the month.'
		WHERE tenant_code = v_tenant_code
        AND task_id IN (SELECT task_id FROM task.task_reward 
                       WHERE tenant_code = v_tenant_code
		  AND task_external_code = 'hydr_to_feel_your_best')
          AND v_language_code = language_code
          ;

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated task detail for tenant %', v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No matching record task id found or already updated for tenant %', v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Update process completed for all tenants.';
END $$;