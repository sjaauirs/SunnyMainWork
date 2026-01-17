-- =================================================================================================================================
-- 🚀 Script    : Reverts Consumer Task -> ProgressDetail for a user to reset sleep task
-- 📌 Purpose   : Reverts Consumer Task -> ProgressDetail for a user to reset sleep task
-- 🧑 Author    : Pranav
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : Rollback SUN-1092
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Reverts changes for progress detail for all consumer whose config is wrong for Get your Z's
-- =================================================================================================================================
DO $$
DECLARE
    v_tenant_codes  TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code   TEXT;
    v_updated_count INTEGER := 0;
    v_total_updated INTEGER := 0;
    v_consumer_code TEXT := 'cmr-13d6066ec7e54023993d83a02ba3567a';
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE task.consumer_task c
        SET progress_detail = '{
            "detailType": "OTHER",
            "healthProgress": {
                "totalUnits": 0,
                "activityLog": [],
                "healthReport": []
            }
        }'::jsonb
        WHERE c.tenant_code   = v_tenant_code
          AND c.consumer_code = v_consumer_code
          -- only restore rows that were nulled out
          AND c.progress_detail IS NULL
          AND c.task_id IN (
              SELECT tr.task_id
              FROM task.task_reward tr
              WHERE tr.task_external_code = 'get_your_z_s_2026'
                AND tr.tenant_code        = v_tenant_code
                AND tr.delete_nbr         = 0
          );

        GET DIAGNOSTICS v_updated_count = ROW_COUNT;
        v_total_updated := v_total_updated + v_updated_count;

        IF v_updated_count > 0 THEN
            RAISE NOTICE '✅ Rollback: restored % record(s) for tenant % consumer %',
                v_updated_count, v_tenant_code, v_consumer_code;
        ELSE
            RAISE NOTICE '⚠️ Rollback: no NULL progress_detail found for tenant % consumer %',
                v_tenant_code, v_consumer_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Rollback process completed for all tenants. Total rows updated: %', v_total_updated;
END $$;
