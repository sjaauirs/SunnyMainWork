-- =================================================================================================================================
-- 🚀 Script    : Update Consumer Task -> ProgressDetail for a user to reset sleep task
-- 📌 Purpose   : Update Consumer Task -> ProgressDetail for a user to reset sleep task
-- 🧑 Author    : Pranav
-- 📅 Date      : 2025-11-13
-- 🧾 Jira      : SUN-1092
-- ⚠️ Inputs    : TENANT_CODE
-- 📤 Output    : Updated progress detail for all consumer whose config is wrong for Get your Z's
--   - Safe to re-run (idempotent)
-- =================================================================================================================================
DO $$
DECLARE
    v_tenant_codes   TEXT[] := ARRAY[
        'KP-TENANT-CODE'
    ];
    v_tenant_code    TEXT;
    v_updated_count  INTEGER;
    v_total_updated  INTEGER := 0;
BEGIN
    FOREACH v_tenant_code IN ARRAY v_tenant_codes
    LOOP
        UPDATE task.consumer_task c
        SET progress_detail = NULL
        WHERE c.tenant_code   = v_tenant_code
          AND c.task_status   = 'IN_PROGRESS'
          AND c.progress_detail->>'detailType' = 'OTHER'
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
            RAISE NOTICE '✅ Updated % record(s) for tenant %',
                v_updated_count, v_tenant_code;
        ELSE
            RAISE NOTICE '⚠️ No matching IN_PROGRESS task with OTHER detailType for tenant %',
                v_tenant_code;
        END IF;
    END LOOP;

    RAISE NOTICE 'Update process completed for all tenants. Total rows updated: %', v_total_updated;
END $$;
