-- =================================================================================================================================
-- 🚀 Script    : Reverts Consumer Task -> ProgressDetail for a user to reset sleep task
-- 📌 Purpose   : Reverts Consumer Task -> ProgressDetail for a user to reset sleep task
-- 🧑 Author    : Neel
-- 📅 Date      : 2025-11-11
-- 🧾 Jira      : SUN-1092
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Reverts progress detail for consumer
-- 🔗 Script URL: Internal configuration update (QA only)
-- 📝 Notes     : 
--   - Safe to re-run (idempotent)
-- =================================================================================================================================
DO $$
DECLARE

    v_tenant_codes TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code TEXT;
    v_updated_count INTEGER := 0;
    v_consumer_code TEXT := 'cmr-13d6066ec7e54023993d83a02ba3567a';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP            

		UPDATE task.consumer_task
		SET progress_detail = 
        '{
            "detailType": "OTHER",
            "healthProgress": {
                "totalUnits": 0,
                "activityLog": [],
                "healthReport": []
            }
            }'::jsonb
		WHERE tenant_code = v_tenant_code
		  AND consumer_code = v_consumer_code
          AND task_id IN (
            SELECT task_id
            FROM task.task_reward
            WHERE task_external_code = 'get_your_z_s_2026'
              AND tenant_code = v_tenant_code
              AND delete_nbr = 0
          );

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;
        
        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Updated consumer task for tenant % consumer %', v_tenant_code, v_consumer_code;
        ELSE
            RAISE NOTICE '⚠️ No matching record found or already updated for tenant % consumer %', v_tenant_code, v_consumer_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Update process completed for all tenants.';
END $$;